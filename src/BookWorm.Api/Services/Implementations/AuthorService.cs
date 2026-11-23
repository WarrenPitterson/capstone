using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Api.Services.Implementations;

public class AuthorService : IAuthorService
{
    private readonly BookWormDbContext _dbContext;
    private readonly IDeliveryEstimator _deliveryEstimator;

    public AuthorService(
        BookWormDbContext dbContext,
        IDeliveryEstimator deliveryEstimator)
    {
        _dbContext = dbContext;
        _deliveryEstimator = deliveryEstimator;
    }

    public async Task<AuthorDetailDto> GetAuthorByIdAsync(int authorId)
    {
        var author = await _dbContext.Authors
            .Include(a => a.Books)
                .ThenInclude(b => b.Publisher)
            .Include(a => a.Books)
                .ThenInclude(b => b.Category)
            .Include(a => a.Books)
                .ThenInclude(b => b.Reviews)
            .Include(a => a.Books)
                .ThenInclude(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == authorId);

        if (author == null)
        {
            throw new NotFoundException($"Author with ID {authorId} not found.");
        }

        var estimatedDeliveryDate = _deliveryEstimator.EstimateDeliveryDate();

        return new AuthorDetailDto
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Bio,
            PhotoUrl = author.PhotoUrl,
            OtherWorks = author.OtherWorks,
            Books = author.Books.Select(b => new BookSummaryDto
            {
                Id = b.Id,
                Title = b.Title,
                ShortDescription = b.Description.Length > 100 ? b.Description[..97] + "..." : b.Description,
                AuthorName = author.Name,
                AuthorId = author.Id,
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
            }).ToList()
        };
    }

    public async Task<List<AuthorSummaryDto>> GetFollowedAuthorsAsync(int userId)
    {
        return await _dbContext.UserFollowedAuthors
            .Include(fa => fa.Author)
            .Where(fa => fa.UserId == userId)
            .OrderByDescending(fa => fa.CreatedAt)
            .Select(fa => new AuthorSummaryDto
            {
                Id = fa.Author!.Id,
                Name = fa.Author.Name,
                Bio = fa.Author.Bio,
                PhotoUrl = fa.Author.PhotoUrl
            })
            .ToListAsync();
    }

    public async Task FollowAuthorAsync(int userId, int authorId)
    {
        var authorExists = await _dbContext.Authors.AnyAsync(a => a.Id == authorId);
        if (!authorExists)
        {
            throw new NotFoundException($"Author with ID {authorId} not found.");
        }

        var exists = await _dbContext.UserFollowedAuthors
            .AnyAsync(fa => fa.UserId == userId && fa.AuthorId == authorId);

        if (!exists)
        {
            await _dbContext.UserFollowedAuthors.AddAsync(new UserFollowedAuthor
            {
                UserId = userId,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UnfollowAuthorAsync(int userId, int authorId)
    {
        var item = await _dbContext.UserFollowedAuthors
            .FirstOrDefaultAsync(fa => fa.UserId == userId && fa.AuthorId == authorId);

        if (item != null)
        {
            _dbContext.UserFollowedAuthors.Remove(item);
            await _dbContext.SaveChangesAsync();
        }
    }
}
