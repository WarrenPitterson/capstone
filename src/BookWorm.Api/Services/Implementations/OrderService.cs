using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Options;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookWorm.Api.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly BookWormDbContext _dbContext;
    private readonly IDeliveryEstimator _deliveryEstimator;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ICartService _cartService;
    private readonly PricingOptions _pricingOptions;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        BookWormDbContext dbContext,
        IDeliveryEstimator deliveryEstimator,
        IPaymentGateway paymentGateway,
        ICartService cartService,
        IOptions<PricingOptions> pricingOptions,
        ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _deliveryEstimator = deliveryEstimator;
        _paymentGateway = paymentGateway;
        _cartService = cartService;
        _pricingOptions = pricingOptions.Value;
        _logger = logger;
    }

    public async Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Book)
                    .ThenInclude(b => b!.Author)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync();

        return orders.Select(o => new OrderSummaryDto
        {
            Id = o.Id,
            Status = o.Status,
            PlacedAt = o.PlacedAt,
            CancelledAt = o.CancelledAt,
            Total = o.Total,
            ItemCount = o.Items.Sum(i => i.Quantity),
            Items = o.Items.Select(MapToOrderItemDto).ToList()
        }).ToList();
    }

    public async Task<OrderDetailDto> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Address)
            .Include(o => o.Payments)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Book)
                    .ThenInclude(b => b!.Author)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found.");
        }

        return MapToOrderDetailDto(order);
    }

    public async Task<OrderDetailDto> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found.");
        }

        var cart = await _dbContext.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Book)
                    .ThenInclude(b => b!.Author)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || cart.Items.Count == 0)
        {
            throw new ValidationException("Cart", "Cannot checkout with an empty cart.");
        }

        int addressId;
        if (request.AddressId.HasValue && request.AddressId.Value > 0)
        {
            var address = await _dbContext.Addresses
                .FirstOrDefaultAsync(a => a.Id == request.AddressId.Value && a.UserId == userId);

            if (address == null)
            {
                throw new NotFoundException($"Address with ID {request.AddressId.Value} not found.");
            }

            addressId = address.Id;
        }
        else if (request.NewAddress != null)
        {
            var newAddress = new Address
            {
                UserId = userId,
                FirstName = request.NewAddress.FirstName.Trim(),
                LastName = request.NewAddress.LastName.Trim(),
                Email = request.NewAddress.Email.Trim().ToLowerInvariant(),
                Line1 = request.NewAddress.Line1.Trim(),
                Line2 = request.NewAddress.Line2?.Trim(),
                City = request.NewAddress.City.Trim(),
                Pin = request.NewAddress.Pin.Trim(),
                State = request.NewAddress.State.Trim(),
                Country = request.NewAddress.Country.Trim(),
                PhoneCountryCode = request.NewAddress.PhoneCountryCode?.Trim() ?? "+91",
                Phone = request.NewAddress.Phone.Trim(),
                IsSaved = request.NewAddress.IsSaved
            };
            await _dbContext.Addresses.AddAsync(newAddress);
            await _dbContext.SaveChangesAsync();
            addressId = newAddress.Id;
        }
        else
        {
            throw new ValidationException("Address", "Delivery address is required.");
        }

        var subtotal = cart.Items.Sum(ci => ci.Quantity * (ci.Book?.Price ?? 0m));
        var tax = Math.Round(subtotal * _pricingOptions.TaxRate, 2);
        var deliveryCharge = subtotal >= _pricingOptions.FreeDeliveryThreshold ? 0.00m : _pricingOptions.StandardDeliveryCharge;

        var couponCodeToUse = !string.IsNullOrWhiteSpace(request.CouponCode) ? request.CouponCode : cart.CouponCode;
        var discount = 0.00m;
        string? appliedCouponCode = null;

        if (!string.IsNullOrWhiteSpace(couponCodeToUse))
        {
            var code = couponCodeToUse.Trim().ToUpperInvariant();
            var coupon = await _dbContext.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code && c.IsActive && c.ExpiresAt > DateTime.UtcNow);

            if (coupon != null)
            {
                discount = Math.Min(coupon.DiscountAmount, subtotal);
                appliedCouponCode = coupon.Code;
            }
            else if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                throw new ValidationException("CouponCode", "Coupon code is invalid or expired.");
            }
        }

        var amountBeforeGiftPoints = subtotal + tax + deliveryCharge - discount;
        var giftPointsToRedeem = 0;

        if (request.GiftPointsToRedeem > 0)
        {
            if (request.GiftPointsToRedeem > user.GiftPointsBalance)
            {
                throw new ValidationException("GiftPointsToRedeem", $"Cannot redeem {request.GiftPointsToRedeem} points. Your balance is {user.GiftPointsBalance}.");
            }

            giftPointsToRedeem = (int)Math.Min((decimal)request.GiftPointsToRedeem, amountBeforeGiftPoints);
        }

        var total = Math.Max(0.00m, amountBeforeGiftPoints - giftPointsToRedeem);
        var placedAt = DateTime.UtcNow;
        var estimatedDeliveryDate = _deliveryEstimator.EstimateDeliveryDate(placedAt);

        var order = new Order
        {
            UserId = userId,
            AddressId = addressId,
            Status = OrderStatus.Placed,
            Subtotal = subtotal,
            Tax = tax,
            DeliveryCharge = deliveryCharge,
            CouponCode = appliedCouponCode,
            Discount = discount,
            GiftPointsRedeemed = giftPointsToRedeem,
            Total = total,
            PlacedAt = placedAt
        };

        foreach (var cartItem in cart.Items)
        {
            var book = await _dbContext.Books.FindAsync(cartItem.BookId);
            if (book != null)
            {
                book.StockQuantity = Math.Max(0, book.StockQuantity - cartItem.Quantity);
                book.CopiesSold += cartItem.Quantity;
            }

            order.Items.Add(new OrderItem
            {
                BookId = cartItem.BookId,
                UnitPrice = cartItem.Book?.Price ?? 0m,
                Quantity = cartItem.Quantity,
                EstimatedDeliveryDate = estimatedDeliveryDate
            });
        }

        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        if (giftPointsToRedeem > 0)
        {
            user.GiftPointsBalance -= giftPointsToRedeem;
            await _dbContext.GiftPointTransactions.AddAsync(new GiftPointTransaction
            {
                UserId = userId,
                Points = -giftPointsToRedeem,
                Reason = $"Redeemed for Order #{order.Id}",
                OrderId = order.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        _dbContext.CartItems.RemoveRange(cart.Items);
        cart.CouponCode = null;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} placed successfully for User {UserId} with Total {Total}", order.Id, userId, order.Total);

        return await GetOrderByIdAsync(userId, order.Id);
    }

    public async Task<OrderDetailDto> CancelOrderAsync(int userId, int orderId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new ConflictException("Order is already cancelled.");
        }

        var hoursSincePlaced = (DateTime.UtcNow - order.PlacedAt).TotalHours;
        if (hoursSincePlaced > 48)
        {
            _logger.LogWarning("Cancellation rejected: Order {OrderId} was placed {Hours} hours ago (limit is 48 hours)", orderId, hoursSincePlaced);
            throw new ConflictException("Orders can only be cancelled within 48 hours of placement.");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;

        foreach (var item in order.Items)
        {
            var book = await _dbContext.Books.FindAsync(item.BookId);
            if (book != null)
            {
                book.StockQuantity += item.Quantity;
                book.CopiesSold = Math.Max(0, book.CopiesSold - item.Quantity);
            }
        }

        if (order.GiftPointsRedeemed > 0 && order.User != null)
        {
            order.User.GiftPointsBalance += order.GiftPointsRedeemed;
            await _dbContext.GiftPointTransactions.AddAsync(new GiftPointTransaction
            {
                UserId = userId,
                Points = order.GiftPointsRedeemed,
                Reason = $"Refund for cancelled Order #{order.Id}",
                OrderId = order.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var payment in order.Payments.Where(p => p.Status == PaymentStatus.Succeeded))
        {
            payment.Status = PaymentStatus.Refunded;
            await _paymentGateway.RefundPaymentAsync(payment.TransactionReference, payment.Amount);
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} was cancelled and refunded successfully", orderId);

        return await GetOrderByIdAsync(userId, orderId);
    }

    public async Task<CartDto> BuyAgainAsync(int userId, int orderId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found.");
        }

        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            await _dbContext.Carts.AddAsync(cart);
            await _dbContext.SaveChangesAsync();
        }

        foreach (var item in order.Items)
        {
            var cartItem = cart.Items.FirstOrDefault(ci => ci.BookId == item.BookId);
            if (cartItem != null)
            {
                cartItem.Quantity += item.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    CartId = cart.Id,
                    BookId = item.BookId,
                    Quantity = item.Quantity
                });
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Buy Again executed: re-added items from Order {OrderId} to Cart for User {UserId}", orderId, userId);

        return await _cartService.GetCartAsync(userId);
    }

    private static OrderItemDto MapToOrderItemDto(OrderItem item)
    {
        return new OrderItemDto
        {
            BookId = item.BookId,
            Title = item.Book?.Title ?? string.Empty,
            AuthorName = item.Book?.Author?.Name ?? string.Empty,
            FrontCoverUrl = item.Book?.FrontCoverUrl ?? string.Empty,
            Format = item.Book?.Format ?? BookFormat.Paperback,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            EstimatedDeliveryDate = item.EstimatedDeliveryDate
        };
    }

    private static OrderDetailDto MapToOrderDetailDto(Order o)
    {
        var successfulPayment = o.Payments
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();

        return new OrderDetailDto
        {
            Id = o.Id,
            UserId = o.UserId,
            Address = new AddressDto
            {
                Id = o.Address?.Id ?? 0,
                UserId = o.Address?.UserId ?? 0,
                FirstName = o.Address?.FirstName ?? string.Empty,
                LastName = o.Address?.LastName ?? string.Empty,
                Email = o.Address?.Email ?? string.Empty,
                Line1 = o.Address?.Line1 ?? string.Empty,
                Line2 = o.Address?.Line2,
                City = o.Address?.City ?? string.Empty,
                Pin = o.Address?.Pin ?? string.Empty,
                State = o.Address?.State ?? string.Empty,
                Country = o.Address?.Country ?? string.Empty,
                PhoneCountryCode = o.Address?.PhoneCountryCode ?? "+91",
                Phone = o.Address?.Phone ?? string.Empty,
                IsSaved = o.Address?.IsSaved ?? false
            },
            Status = o.Status,
            Subtotal = o.Subtotal,
            Tax = o.Tax,
            DeliveryCharge = o.DeliveryCharge,
            CouponCode = o.CouponCode,
            Discount = o.Discount,
            GiftPointsRedeemed = o.GiftPointsRedeemed,
            Total = o.Total,
            PlacedAt = o.PlacedAt,
            CancelledAt = o.CancelledAt,
            Items = o.Items.Select(MapToOrderItemDto).ToList(),
            Payment = successfulPayment != null ? new PaymentDto
            {
                Id = successfulPayment.Id,
                OrderId = successfulPayment.OrderId,
                Method = successfulPayment.Method,
                Status = successfulPayment.Status,
                Last4 = successfulPayment.Last4,
                Amount = successfulPayment.Amount,
                TransactionReference = successfulPayment.TransactionReference,
                CreatedAt = successfulPayment.CreatedAt
            } : null
        };
    }
}
