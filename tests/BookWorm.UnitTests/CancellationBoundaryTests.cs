using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Options;
using BookWorm.Api.Services.Implementations;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BookWorm.UnitTests;

public class CancellationBoundaryTests
{
    private readonly BookWormDbContext _dbContext;
    private readonly Mock<IDeliveryEstimator> _deliveryEstimatorMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly IOptions<PricingOptions> _pricingOptions;
    private readonly OrderService _sut;

    public CancellationBoundaryTests()
    {
        var options = new DbContextOptionsBuilder<BookWormDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new BookWormDbContext(options);
        _deliveryEstimatorMock = new Mock<IDeliveryEstimator>();
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _cartServiceMock = new Mock<ICartService>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _pricingOptions = Options.Create(new PricingOptions());

        _sut = new OrderService(
            _dbContext,
            _deliveryEstimatorMock.Object,
            _paymentGatewayMock.Object,
            _cartServiceMock.Object,
            _pricingOptions,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CancelOrderAsync_Placed47Hours59MinutesAgo_AllowsCancellation()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com", GiftPointsBalance = 50 };
        var book = new Book { Id = 1, Title = "Book 1", Price = 100.00m, StockQuantity = 8, CopiesSold = 2 };
        var address = new Address { Id = 1, UserId = 1, Line1 = "Street", City = "City", Pin = "123", State = "State", Country = "India", Phone = "123" };
        var order = new Order
        {
            Id = 1,
            UserId = 1,
            AddressId = 1,
            Status = OrderStatus.Placed,
            Subtotal = 200.00m,
            Tax = 24.40m,
            DeliveryCharge = 50.00m,
            Discount = 0.00m,
            GiftPointsRedeemed = 50,
            Total = 224.40m,
            PlacedAt = DateTime.UtcNow.AddHours(-47).AddMinutes(-59),
            Items = new List<OrderItem>
            {
                new() { OrderId = 1, BookId = 1, UnitPrice = 100.00m, Quantity = 2, EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3) }
            },
            Payments = new List<Payment>
            {
                new() { Id = 1, OrderId = 1, Method = PaymentMethod.CreditCard, Status = PaymentStatus.Succeeded, Amount = 224.40m, TransactionReference = "TXN123" }
            }
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Books.AddAsync(book);
        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        _paymentGatewayMock
            .Setup(p => p.RefundPaymentAsync("TXN123", 224.40m))
            .ReturnsAsync(new PaymentGatewayResult { IsSuccess = true, TransactionReference = "REF123" });

        // Act
        var result = await _sut.CancelOrderAsync(1, 1);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, result.Status);
        Assert.NotNull(result.CancelledAt);

        var updatedBook = await _dbContext.Books.FindAsync(1);
        Assert.Equal(10, updatedBook!.StockQuantity);
        Assert.Equal(0, updatedBook.CopiesSold);

        var updatedUser = await _dbContext.Users.FindAsync(1);
        Assert.Equal(100, updatedUser!.GiftPointsBalance);
    }

    [Fact]
    public async Task CancelOrderAsync_Placed48Hours01MinutesAgo_ThrowsConflictException()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com", GiftPointsBalance = 50 };
        var book = new Book { Id = 1, Title = "Book 1", Price = 100.00m, StockQuantity = 8, CopiesSold = 2 };
        var address = new Address { Id = 1, UserId = 1, Line1 = "Street", City = "City", Pin = "123", State = "State", Country = "India", Phone = "123" };
        var order = new Order
        {
            Id = 1,
            UserId = 1,
            AddressId = 1,
            Status = OrderStatus.Placed,
            Subtotal = 200.00m,
            Tax = 24.40m,
            DeliveryCharge = 50.00m,
            Discount = 0.00m,
            GiftPointsRedeemed = 0,
            Total = 274.40m,
            PlacedAt = DateTime.UtcNow.AddHours(-48).AddMinutes(-1),
            Items = new List<OrderItem>
            {
                new() { OrderId = 1, BookId = 1, UnitPrice = 100.00m, Quantity = 2, EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3) }
            }
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Books.AddAsync(book);
        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _sut.CancelOrderAsync(1, 1));
    }
}
