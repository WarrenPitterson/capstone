using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookWorm.UnitTests;

public class ReviewServiceTests
{
    private readonly BookWormDbContext _dbContext;
    private readonly Mock<ILogger<ReviewService>> _loggerMock;
    private readonly ReviewService _sut;

    public ReviewServiceTests()
    {
        var options = new DbContextOptionsBuilder<BookWormDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new BookWormDbContext(options);
        _loggerMock = new Mock<ILogger<ReviewService>>();
        _sut = new ReviewService(_dbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task AddReviewAsync_DuplicateReview_ThrowsConflictException()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com" };
        var book = new Book { Id = 1, Title = "Book 1" };
        var existingReview = new Review { Id = 1, BookId = 1, UserId = 1, Rating = 5, Text = "Great!" };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Books.AddAsync(book);
        await _dbContext.Reviews.AddAsync(existingReview);
        await _dbContext.SaveChangesAsync();

        var request = new CreateReviewRequest { Rating = 4, Text = "Second review attempt" };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _sut.AddReviewAsync(1, 1, request));
    }

    [Fact]
    public async Task AddReviewAsync_TextOver100Chars_ThrowsValidationException()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com" };
        var book = new Book { Id = 1, Title = "Book 1" };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        var longText = new string('A', 101);
        var request = new CreateReviewRequest { Rating = 5, Text = longText };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.AddReviewAsync(1, 1, request));
    }

    [Fact]
    public async Task AddReviewAsync_RatingOutOfRange_ThrowsValidationException()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com" };
        var book = new Book { Id = 1, Title = "Book 1" };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        var request = new CreateReviewRequest { Rating = 6, Text = "Superb" };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.AddReviewAsync(1, 1, request));
    }
}
