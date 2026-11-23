using BookWorm.Api.Models;

namespace BookWorm.Api.Services.Interfaces;

public interface IBookService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<List<PublisherDto>> GetPublishersAsync();
    Task<PaginatedBooksResponse> GetBooksAsync(BookQueryParameters parameters);
    Task<BookDetailDto> GetBookByIdAsync(int id);
    Task<List<BookSummaryDto>> GetRecommendedBooksAsync(int? userId);
    Task<List<BookSummaryDto>> GetBestsellersAsync();
    Task<List<BookSummaryDto>> GetNewLaunchesAsync();
}
