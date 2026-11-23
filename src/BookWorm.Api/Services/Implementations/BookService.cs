using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Api.Services.Implementations;

public class BookService : IBookService
{
    private readonly BookWormDbContext _dbContext;
    private readonly IDeliveryEstimator _deliveryEstimator;

    public BookService(
        BookWormDbContext dbContext,
        IDeliveryEstimator deliveryEstimator)
    {
        _dbContext = dbContext;
        _deliveryEstimator = deliveryEstimator;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return await _dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();
    }

    public async Task<List<PublisherDto>> GetPublishersAsync()
    {
        return await _dbContext.Publishers
            .OrderBy(p => p.Name)
            .Select(p => new PublisherDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToListAsync();
    }

    public async Task<PaginatedBooksResponse> GetBooksAsync(BookQueryParameters parameters)
    {
        var query = _dbContext.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Category)
            .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
            .Include(b => b.Reviews)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(search) ||
                b.Author!.Name.ToLower().Contains(search) ||
                b.Description.ToLower().Contains(search));
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.PublisherId.HasValue)
        {
            query = query.Where(b => b.PublisherId == parameters.PublisherId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Language))
        {
            query = query.Where(b => b.Language.ToLower() == parameters.Language.Trim().ToLower());
        }

        if (parameters.Format.HasValue)
        {
            query = query.Where(b => b.Format == parameters.Format.Value);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(b => b.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(b => b.Price <= parameters.MaxPrice.Value);
        }

        query = parameters.Sort?.ToLower() switch
        {
            "priceasc" => query.OrderBy(b => b.Price),
            "pricedesc" => query.OrderByDescending(b => b.Price),
            "newest" => query.OrderByDescending(b => b.CreatedAt),
            "rating" => query.OrderByDescending(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0),
            _ => query.OrderByDescending(b => b.CopiesSold)
        };

        var totalCount = await query.CountAsync();
        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 12;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var estimatedDeliveryDate = _deliveryEstimator.EstimateDeliveryDate();

        return new PaginatedBooksResponse
        {
            Items = items.Select(b => MapToBookSummaryDto(b, estimatedDeliveryDate)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<BookDetailDto> GetBookByIdAsync(int id)
    {
        var book = await _dbContext.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Category)
            .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
            .Include(b => b.Reviews)
            .Include(b => b.RelatedFromBooks)
                .ThenInclude(rfb => rfb.RelatedToBook)
                    .ThenInclude(rtb => rtb!.Author)
            .Include(b => b.RelatedFromBooks)
                .ThenInclude(rfb => rfb.RelatedToBook)
                    .ThenInclude(rtb => rtb!.Reviews)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            throw new NotFoundException($"Book with ID {id} not found.");
        }

        var estimatedDeliveryDate = _deliveryEstimator.EstimateDeliveryDate();
        var relatedBooks = book.RelatedFromBooks
            .Where(rfb => rfb.RelatedToBook != null)
            .Select(rfb => MapToBookSummaryDto(rfb.RelatedToBook!, estimatedDeliveryDate))
            .ToList();

        if (relatedBooks.Count == 0)
        {
            var fallbackRelated = await _dbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Reviews)
                .Where(b => b.CategoryId == book.CategoryId && b.Id != book.Id)
                .Take(3)
                .ToListAsync();

            relatedBooks = fallbackRelated
                .Select(b => MapToBookSummaryDto(b, estimatedDeliveryDate))
                .ToList();
        }

        return new BookDetailDto
        {
            Id = book.Id,
            Title = book.Title,
            Description = book.Description,
            Author = new AuthorSummaryDto
            {
                Id = book.Author!.Id,
                Name = book.Author.Name,
                Bio = book.Author.Bio,
                PhotoUrl = book.Author.PhotoUrl
            },
            Publisher = new PublisherDto
            {
                Id = book.Publisher!.Id,
                Name = book.Publisher.Name
            },
            Category = new CategoryDto
            {
                Id = book.Category!.Id,
                Name = book.Category.Name
            },
            Format = book.Format,
            Language = book.Language,
            Price = book.Price,
            Isbn = book.Isbn,
            FrontCoverUrl = book.FrontCoverUrl,
            BackCoverUrl = book.BackCoverUrl,
            AverageRating = book.Reviews.Any() ? Math.Round(book.Reviews.Average(r => r.Rating), 1) : 0,
            CopiesSold = book.CopiesSold,
            StockQuantity = book.StockQuantity,
            IsNewLaunch = book.IsNewLaunch,
            GenreTags = book.BookGenres.Select(bg => bg.Genre?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList(),
            EstimatedDeliveryDate = estimatedDeliveryDate,
            RelatedReads = relatedBooks
        };
    }

    public async Task<List<BookSummaryDto>> GetRecommendedBooksAsync(int? userId)
    {
        var estimatedDeliveryDate = _deliveryEstimator.EstimateDeliveryDate();

        if (userId.HasValue)
        {
            var purchasedBookIds = await _dbContext.OrderItems
                .Where(oi => oi.Order!.UserId == userId.Value && oi.Order.Status != OrderStatus.Cancelled)
                .Select(oi => oi.BookId)
                .Distinct()
                .ToListAsync();

            var topCategoryIds = await _dbContext.OrderItems
                .Where(oi => oi.Order!.UserId == userId.Value && oi.Order.Status != OrderStatus.Cancelled)
                .GroupBy(oi => oi.Book!.CategoryId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToListAsync();

            if (topCategoryIds.Count > 0)
            {
                var recommended = await _dbContext.Books
                    .Include(b => b.Author)
                    .Include(b => b.Reviews)
                    .Include(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                    .Where(b => topCategoryIds.Contains(b.CategoryId) && !purchasedBookIds.Contains(b.Id))
                    .OrderByDescending(b => b.CopiesSold)
                    .Take(6)
                    .ToListAsync();

                if (recommended.Count > 0)
                {
                    return recommended.Select(b => MapToBookSummaryDto(b, estimatedDeliveryDate)).ToList();
                }
            }
        }

        return await GetBestsellersAsync();
    }

    public async Task<List<BookSummaryDto>> GetBestsellersAsync()
    {
        var estimatedDeliveryDate = _deliveryEstimator.EstimateDeliveryDate();

        var books = await _dbContext.Books
            .Include(b => b.Author)
            .Include(b => b.Reviews)
            .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
            .OrderByDescending(b => b.CopiesSold)
            .Take(6)
            .ToListAsync();

        return books.Select(b => MapToBookSummaryDto(b, estimatedDeliveryDate)).ToList();
    }

    public async Task<List<BookSummaryDto>> GetNewLaunchesAsync()
    {
        var estimatedDeliveryDate = _deliveryEstimator.EstimateDeliveryDate();

        var books = await _dbContext.Books
            .Include(b => b.Author)
            .Include(b => b.Reviews)
            .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
            .Where(b => b.IsNewLaunch)
            .OrderByDescending(b => b.CreatedAt)
            .Take(6)
            .ToListAsync();

        return books.Select(b => MapToBookSummaryDto(b, estimatedDeliveryDate)).ToList();
    }

    private static BookSummaryDto MapToBookSummaryDto(Book b, DateTime estimatedDeliveryDate)
    {
        return new BookSummaryDto
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
        };
    }
}
