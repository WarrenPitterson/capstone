using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Options;
using BookWorm.Api.Services.Implementations;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BookWorm.UnitTests;

public class CouponAndGiftPointTests
{
    private readonly BookWormDbContext _dbContext;
    private readonly Mock<IDeliveryEstimator> _deliveryEstimatorMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly IOptions<PricingOptions> _pricingOptions;
    private readonly OrderService _sut;

    public CouponAndGiftPointTests()
    {
        var options = new DbContextOptionsBuilder<BookWormDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new BookWormDbContext(options);
        _deliveryEstimatorMock = new Mock<IDeliveryEstimator>();
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _cartServiceMock = new Mock<ICartService>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _pricingOptions = Options.Create(new PricingOptions
        {
            TaxRate = 0.122m,
            FreeDeliveryThreshold = 500.00m,
            StandardDeliveryCharge = 50.00m,
            EstimatedDaysToDeliver = 3
        });

        _deliveryEstimatorMock
            .Setup(d => d.EstimateDeliveryDate(It.IsAny<DateTime?>()))
            .Returns(new DateTime(2026, 7, 21));

        _sut = new OrderService(
            _dbContext,
            _deliveryEstimatorMock.Object,
            _paymentGatewayMock.Object,
            _cartServiceMock.Object,
            _pricingOptions,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidCouponAndGiftPoints_AppliesDiscountsCorrectly()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com", GiftPointsBalance = 100 };
        var book = new Book { Id = 1, Title = "Book 1", Price = 508.00m, StockQuantity = 10 };
        var coupon = new Coupon { Id = 1, Code = "SAVE100", DiscountAmount = 100.00m, ExpiresAt = DateTime.UtcNow.AddDays(10), IsActive = true };
        var address = new Address { Id = 1, UserId = 1, Line1 = "Street", City = "City", Pin = "123", State = "State", Country = "India", Phone = "123" };
        var cart = new Cart
        {
            Id = 1,
            UserId = 1,
            Items = new List<CartItem> { new() { CartId = 1, BookId = 1, Book = book, Quantity = 1 } }
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Books.AddAsync(book);
        await _dbContext.Coupons.AddAsync(coupon);
        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.Carts.AddAsync(cart);
        await _dbContext.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            AddressId = 1,
            CouponCode = "SAVE100",
            GiftPointsToRedeem = 50
        };

        // Act
        var result = await _sut.CreateOrderAsync(1, request);

        // Assert
        Assert.Equal(508.00m, result.Subtotal);
        Assert.Equal(61.98m, result.Tax);
        Assert.Equal(0.00m, result.DeliveryCharge);
        Assert.Equal(100.00m, result.Discount);
        Assert.Equal(50, result.GiftPointsRedeemed);
        Assert.Equal(419.98m, result.Total);

        var updatedUser = await _dbContext.Users.FindAsync(1);
        Assert.Equal(50, updatedUser!.GiftPointsBalance);
    }

    [Fact]
    public async Task CreateOrderAsync_ExpiredCoupon_ThrowsValidationException()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com", GiftPointsBalance = 0 };
        var book = new Book { Id = 1, Title = "Book 1", Price = 200.00m, StockQuantity = 10 };
        var coupon = new Coupon { Id = 1, Code = "EXPIRED", DiscountAmount = 50.00m, ExpiresAt = DateTime.UtcNow.AddDays(-1), IsActive = true };
        var address = new Address { Id = 1, UserId = 1, Line1 = "Street", City = "City", Pin = "123", State = "State", Country = "India", Phone = "123" };
        var cart = new Cart
        {
            Id = 1,
            UserId = 1,
            Items = new List<CartItem> { new() { CartId = 1, BookId = 1, Book = book, Quantity = 1 } }
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Books.AddAsync(book);
        await _dbContext.Coupons.AddAsync(coupon);
        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.Carts.AddAsync(cart);
        await _dbContext.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            AddressId = 1,
            CouponCode = "EXPIRED"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateOrderAsync(1, request));
    }

    [Fact]
    public async Task CreateOrderAsync_GiftPointsExceedingBalance_ThrowsValidationException()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com", GiftPointsBalance = 20 };
        var book = new Book { Id = 1, Title = "Book 1", Price = 200.00m, StockQuantity = 10 };
        var address = new Address { Id = 1, UserId = 1, Line1 = "Street", City = "City", Pin = "123", State = "State", Country = "India", Phone = "123" };
        var cart = new Cart
        {
            Id = 1,
            UserId = 1,
            Items = new List<CartItem> { new() { CartId = 1, BookId = 1, Book = book, Quantity = 1 } }
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Books.AddAsync(book);
        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.Carts.AddAsync(cart);
        await _dbContext.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            AddressId = 1,
            GiftPointsToRedeem = 50
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateOrderAsync(1, request));
    }
}
