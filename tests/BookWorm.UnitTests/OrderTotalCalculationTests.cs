using BookWorm.Api.Data;
using BookWorm.Api.Entities;
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

public class OrderTotalCalculationTests
{
    private readonly BookWormDbContext _dbContext;
    private readonly Mock<IDeliveryEstimator> _deliveryEstimatorMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly IOptions<PricingOptions> _pricingOptions;
    private readonly OrderService _sut;

    public OrderTotalCalculationTests()
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
    public async Task CreateOrderAsync_CartBelowFreeDeliveryThreshold_AppliesStandardDeliveryCharge()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com", GiftPointsBalance = 0 };
        var book = new Book { Id = 1, Title = "Book 1", Price = 300.00m, StockQuantity = 10 };
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

        var request = new CreateOrderRequest { AddressId = 1 };

        // Act
        var result = await _sut.CreateOrderAsync(1, request);

        // Assert
        Assert.Equal(300.00m, result.Subtotal);
        Assert.Equal(36.60m, result.Tax);
        Assert.Equal(50.00m, result.DeliveryCharge);
        Assert.Equal(386.60m, result.Total);
    }

    [Fact]
    public async Task CreateOrderAsync_CartAboveFreeDeliveryThreshold_AppliesFreeDelivery()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com", GiftPointsBalance = 0 };
        var book1 = new Book { Id = 1, Title = "Book 1", Price = 149.00m, StockQuantity = 10 };
        var book2 = new Book { Id = 2, Title = "Book 2", Price = 359.00m, StockQuantity = 10 };
        var address = new Address { Id = 1, UserId = 1, Line1 = "Street", City = "City", Pin = "123", State = "State", Country = "India", Phone = "123" };
        var cart = new Cart
        {
            Id = 1,
            UserId = 1,
            Items = new List<CartItem>
            {
                new() { CartId = 1, BookId = 1, Book = book1, Quantity = 1 },
                new() { CartId = 1, BookId = 2, Book = book2, Quantity = 1 }
            }
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Books.AddRangeAsync(book1, book2);
        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.Carts.AddAsync(cart);
        await _dbContext.SaveChangesAsync();

        var request = new CreateOrderRequest { AddressId = 1 };

        // Act
        var result = await _sut.CreateOrderAsync(1, request);

        // Assert
        Assert.Equal(508.00m, result.Subtotal);
        Assert.Equal(61.98m, result.Tax);
        Assert.Equal(0.00m, result.DeliveryCharge);
        Assert.Equal(569.98m, result.Total);
    }
}
