using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Options;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookWorm.Api.Services.Implementations;

public class CartService : ICartService
{
    private readonly BookWormDbContext _dbContext;
    private readonly IDeliveryEstimator _deliveryEstimator;
    private readonly PricingOptions _pricingOptions;

    public CartService(
        BookWormDbContext dbContext,
        IDeliveryEstimator deliveryEstimator,
        IOptions<PricingOptions> pricingOptions)
    {
        _dbContext = dbContext;
        _deliveryEstimator = deliveryEstimator;
        _pricingOptions = pricingOptions.Value;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await CalculateCartDtoAsync(cart);
    }

    public async Task<CartDto> AddItemToCartAsync(int userId, AddCartItemRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new ValidationException("Quantity", "Quantity must be greater than or equal to 1.");
        }

        var book = await _dbContext.Books.FindAsync(request.BookId);
        if (book == null)
        {
            throw new NotFoundException($"Book with ID {request.BookId} not found.");
        }

        var cart = await GetOrCreateCartAsync(userId);

        var existingItem = cart.Items.FirstOrDefault(ci => ci.BookId == request.BookId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                BookId = request.BookId,
                Quantity = request.Quantity
            });
        }

        await _dbContext.SaveChangesAsync();

        return await CalculateCartDtoAsync(cart);
    }

    public async Task<CartDto> UpdateItemQuantityAsync(int userId, int bookId, UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new ValidationException("Quantity", "Quantity must be greater than or equal to 1.");
        }

        var cart = await GetOrCreateCartAsync(userId);

        var item = cart.Items.FirstOrDefault(ci => ci.BookId == bookId);
        if (item == null)
        {
            throw new NotFoundException($"Book with ID {bookId} not found in cart.");
        }

        item.Quantity = request.Quantity;
        await _dbContext.SaveChangesAsync();

        return await CalculateCartDtoAsync(cart);
    }

    public async Task<CartDto> RemoveItemFromCartAsync(int userId, int bookId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var item = cart.Items.FirstOrDefault(ci => ci.BookId == bookId);
        if (item != null)
        {
            cart.Items.Remove(item);
            await _dbContext.SaveChangesAsync();
        }

        return await CalculateCartDtoAsync(cart);
    }

    public async Task<CartDto> ApplyCouponAsync(int userId, ApplyCouponRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);

        if (string.IsNullOrWhiteSpace(request.CouponCode))
        {
            cart.CouponCode = null;
            await _dbContext.SaveChangesAsync();
            return await CalculateCartDtoAsync(cart);
        }

        var code = request.CouponCode.Trim().ToUpperInvariant();
        var coupon = await _dbContext.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code && c.IsActive && c.ExpiresAt > DateTime.UtcNow);

        if (coupon == null)
        {
            throw new ValidationException("CouponCode", "Coupon code is invalid or expired.");
        }

        cart.CouponCode = coupon.Code;
        await _dbContext.SaveChangesAsync();

        return await CalculateCartDtoAsync(cart);
    }

    private async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Book)
                    .ThenInclude(b => b!.Author)
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Book)
                    .ThenInclude(b => b!.BookGenres)
                        .ThenInclude(bg => bg.Genre)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId
            };
            await _dbContext.Carts.AddAsync(cart);
            await _dbContext.SaveChangesAsync();
        }

        return cart;
    }

    private async Task<CartDto> CalculateCartDtoAsync(Cart cart)
    {
        var estimatedDeliveryDate = _deliveryEstimator.EstimateDeliveryDate();

        var items = cart.Items.Select(ci => new CartItemDto
        {
            BookId = ci.BookId,
            Title = ci.Book?.Title ?? string.Empty,
            AuthorName = ci.Book?.Author?.Name ?? string.Empty,
            Format = ci.Book?.Format ?? BookFormat.Paperback,
            Price = ci.Book?.Price ?? 0m,
            Quantity = ci.Quantity,
            FrontCoverUrl = ci.Book?.FrontCoverUrl ?? string.Empty,
            GenreTags = ci.Book?.BookGenres.Select(bg => bg.Genre?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>(),
            EstimatedDeliveryDate = estimatedDeliveryDate
        }).ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var tax = Math.Round(subtotal * _pricingOptions.TaxRate, 2);
        var deliveryCharge = subtotal == 0 || subtotal >= _pricingOptions.FreeDeliveryThreshold ? 0.00m : _pricingOptions.StandardDeliveryCharge;

        var discount = 0.00m;
        if (!string.IsNullOrWhiteSpace(cart.CouponCode))
        {
            var coupon = await _dbContext.Coupons
                .FirstOrDefaultAsync(c => c.Code == cart.CouponCode && c.IsActive && c.ExpiresAt > DateTime.UtcNow);

            if (coupon != null)
            {
                discount = Math.Min(coupon.DiscountAmount, subtotal);
            }
            else
            {
                cart.CouponCode = null;
                await _dbContext.SaveChangesAsync();
            }
        }

        var total = Math.Max(0.00m, subtotal + tax + deliveryCharge - discount);

        return new CartDto
        {
            Id = cart.Id,
            Items = items,
            Subtotal = subtotal,
            Tax = tax,
            DeliveryCharge = deliveryCharge,
            CouponCode = cart.CouponCode,
            Discount = discount,
            Total = total
        };
    }
}
