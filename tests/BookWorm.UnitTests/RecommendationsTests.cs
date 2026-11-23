using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Services.Implementations;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BookWorm.UnitTests;

public class RecommendationsTests
{
    private readonly BookWormDbContext _dbContext;
    private readonly Mock<IDeliveryEstimator> _deliveryEstimatorMock;
    private readonly BookService _sut;

    public RecommendationsTests()
    {
        var options = new DbContextOptionsBuilder<BookWormDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new BookWormDbContext(options);
        _deliveryEstimatorMock = new Mock<IDeliveryEstimator>();

        _deliveryEstimatorMock
            .Setup(d => d.EstimateDeliveryDate(It.IsAny<DateTime?>()))
            .Returns(new DateTime(2026, 7, 21));

        _sut = new BookService(_dbContext, _deliveryEstimatorMock.Object);
    }

    [Fact]
    public async Task GetRecommendedBooksAsync_GuestUser_ReturnsBestsellersFallback()
    {
        // Arrange
        var cat = new Category { Id = 1, Name = "Self-help" };
        var auth = new Author { Id = 1, Name = "Author 1" };
        var pub = new Publisher { Id = 1, Name = "Pub 1" };
        var book1 = new Book { Id = 1, Title = "Bestseller 1", CategoryId = 1, AuthorId = 1, PublisherId = 1, CopiesSold = 500 };
        var book2 = new Book { Id = 2, Title = "Bestseller 2", CategoryId = 1, AuthorId = 1, PublisherId = 1, CopiesSold = 300 };

        await _dbContext.Categories.AddAsync(cat);
        await _dbContext.Authors.AddAsync(auth);
        await _dbContext.Publishers.AddAsync(pub);
        await _dbContext.Books.AddRangeAsync(book1, book2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetRecommendedBooksAsync(null);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Bestseller 1", result[0].Title);
        Assert.Equal("Bestseller 2", result[1].Title);
    }

    [Fact]
    public async Task GetRecommendedBooksAsync_UserWithOrderHistory_RecommendsTopCategoryExcludingPurchased()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com" };
        var catSelfHelp = new Category { Id = 1, Name = "Self-help" };
        var catMystery = new Category { Id = 2, Name = "Mystery" };
        var auth = new Author { Id = 1, Name = "Author 1" };
        var pub = new Publisher { Id = 1, Name = "Pub 1" };
        var address = new Address { Id = 1, UserId = 1, Line1 = "Street", City = "City", Pin = "123", State = "State", Country = "India", Phone = "123" };

        var bookPurchased = new Book { Id = 1, Title = "Purchased Book", CategoryId = 1, AuthorId = 1, PublisherId = 1, CopiesSold = 100 };
        var bookRecommended = new Book { Id = 2, Title = "Unpurchased Self-help", CategoryId = 1, AuthorId = 1, PublisherId = 1, CopiesSold = 200 };
        var bookMystery = new Book { Id = 3, Title = "Mystery Book", CategoryId = 2, AuthorId = 1, PublisherId = 1, CopiesSold = 150 };

        var order = new Order
        {
            Id = 1,
            UserId = 1,
            AddressId = 1,
            Status = OrderStatus.Placed,
            Items = new List<OrderItem>
            {
                new() { OrderId = 1, BookId = 1, Book = bookPurchased, UnitPrice = 100.00m, Quantity = 1 }
            }
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Categories.AddRangeAsync(catSelfHelp, catMystery);
        await _dbContext.Authors.AddAsync(auth);
        await _dbContext.Publishers.AddAsync(pub);
        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.Books.AddRangeAsync(bookPurchased, bookRecommended, bookMystery);
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetRecommendedBooksAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Unpurchased Self-help", result[0].Title);
    }
}
