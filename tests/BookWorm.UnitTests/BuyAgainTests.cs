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

public class BuyAgainTests
{
    private readonly BookWormDbContext _dbContext;
    private readonly Mock<IDeliveryEstimator> _deliveryEstimatorMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly IOptions<PricingOptions> _pricingOptions;
    private readonly OrderService _sut;

    public BuyAgainTests()
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
    public async Task BuyAgainAsync_ValidOrder_ReAddsOrderItemsToCart()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com" };
        var book = new Book { Id = 1, Title = "Book 1", Price = 100.00m };
        var address = new Address { Id = 1, UserId = 1, Line1 = "Street", City = "City", Pin = "123", State = "State", Country = "India", Phone = "123" };
        var order = new Order
        {
            Id = 1,
            UserId = 1,
            AddressId = 1,
            Status = OrderStatus.Placed,
            Total = 200.00m,
            PlacedAt = DateTime.UtcNow,
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

        _cartServiceMock
            .Setup(c => c.GetCartAsync(1))
            .ReturnsAsync(new CartDto
            {
                Id = 1,
                Items = new List<CartItemDto>
                {
                    new() { BookId = 1, Title = "Book 1", Price = 100.00m, Quantity = 2 }
                },
                Subtotal = 200.00m,
                Total = 274.40m
            });

        // Act
        var result = await _sut.BuyAgainAsync(1, 1);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Quantity);

        var cart = await _dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == 1);
        Assert.NotNull(cart);
        Assert.Single(cart.Items);
        Assert.Equal(2, cart.Items.First().Quantity);
    }
}
