using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Api.Services.Implementations;

public class WishlistService : IWishlistService
{
    private readonly BookWormDbContext _dbContext;
    private readonly IDeliveryEstimator _deliveryEstimator;

    public WishlistService(
        BookWormDbContext dbContext,
        IDeliveryEstimator deliveryEstimator)
    {
        _dbContext = dbContext;
        _deliveryEstimator = deliveryEstimator;
    }

    public async Task<List<BookSummaryDto>> GetWishlistAsync(int userId)
    {
        var estimatedDeliveryDate = _deliveryEstimator.EstimateDeliveryDate();

        var wishlistItems = await _dbContext.WishlistItems
            .Include(w => w.Book)
                .ThenInclude(b => b!.Author)
            .Include(w => w.Book)
                .ThenInclude(b => b!.Publisher)
            .Include(w => w.Book)
                .ThenInclude(b => b!.Category)
            .Include(w => w.Book)
                .ThenInclude(b => b!.Reviews)
            .Include(w => w.Book)
                .ThenInclude(b => b!.BookGenres)
                    .ThenInclude(bg => bg.Genre)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => w.Book!)
            .ToListAsync();

        return wishlistItems.Select(b => new BookSummaryDto
        {
            Id = b.Id,
            Title = b.Title,
            ShortDescription = b.Description.Length > 100 ? b.Description[..97] + "..." : b.Description,
            AuthorName = b.Author?.Name ?? string.Empty,
            AuthorId = b.AuthorId,
            PublisherName = b.Publisher?.Name ?? string.Empty,
            CategoryName = b.Category?.Name ?? string.Empty,
            Format = b.Format,
            Language = b.Language,
            Price = b.Price,
            FrontCoverUrl = b.FrontCoverUrl,
            BackCoverUrl = b.BackCoverUrl,
            AverageRating = b.Reviews.Any() ? Math.Round(b.Reviews.Average(r => r.Rating), 1) : 0,
            CopiesSold = b.CopiesSold,
            IsNewLaunch = b.IsNewLaunch,
            GenreTags = b.BookGenres.Select(bg => bg.Genre?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList(),
            EstimatedDeliveryDate = estimatedDeliveryDate
        }).ToList();
    }

    public async Task AddToWishlistAsync(int userId, int bookId)
    {
        var bookExists = await _dbContext.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists)
        {
            throw new NotFoundException($"Book with ID {bookId} not found.");
        }

        var exists = await _dbContext.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.BookId == bookId);

        if (!exists)
        {
            await _dbContext.WishlistItems.AddAsync(new WishlistItem
            {
                UserId = userId,
                BookId = bookId,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RemoveFromWishlistAsync(int userId, int bookId)
    {
        var item = await _dbContext.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId);

        if (item != null)
        {
            _dbContext.WishlistItems.Remove(item);
            await _dbContext.SaveChangesAsync();
        }
    }
}
