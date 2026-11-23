using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookWorm.Api.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly BookWormDbContext _dbContext;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        BookWormDbContext dbContext,
        ILogger<ReviewService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<ReviewDto>> GetBookReviewsAsync(int bookId)
    {
        var bookExists = await _dbContext.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists)
        {
            throw new NotFoundException($"Book with ID {bookId} not found.");
        }

        return await _dbContext.Reviews
            .Include(r => r.User)
            .Where(r => r.BookId == bookId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                BookId = r.BookId,
                UserId = r.UserId,
                UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}".Trim() : "Anonymous",
                Rating = r.Rating,
                Text = r.Text,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ReviewDto> AddReviewAsync(int userId, int bookId, CreateReviewRequest request)
    {
        var book = await _dbContext.Books.FindAsync(bookId);
        if (book == null)
        {
            throw new NotFoundException($"Book with ID {bookId} not found.");
        }

        var existingReview = await _dbContext.Reviews
            .AnyAsync(r => r.BookId == bookId && r.UserId == userId);

        if (existingReview)
        {
            _logger.LogWarning("Duplicate review attempt: User {UserId} already reviewed Book {BookId}", userId, bookId);
            throw new ConflictException("You have already reviewed this book.");
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ValidationException("Rating", "Rating must be between 1 and 5.");
        }

        if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 100)
        {
            throw new ValidationException("Text", "Review text cannot exceed 100 characters and cannot be empty.");
        }

        var review = new Review
        {
            BookId = bookId,
            UserId = userId,
            Rating = request.Rating,
            Text = request.Text.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Reviews.AddAsync(review);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FindAsync(userId);

        _logger.LogInformation("Review {ReviewId} added by User {UserId} for Book {BookId}", review.Id, userId, bookId);

        return new ReviewDto
        {
            Id = review.Id,
            BookId = review.BookId,
            UserId = review.UserId,
            UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "User",
            Rating = review.Rating,
            Text = review.Text,
            CreatedAt = review.CreatedAt
        };
    }
}
