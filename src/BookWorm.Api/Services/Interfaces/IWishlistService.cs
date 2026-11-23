using BookWorm.Api.Models;

namespace BookWorm.Api.Services.Interfaces;

public interface IWishlistService
{
    Task<List<BookSummaryDto>> GetWishlistAsync(int userId);
    Task AddToWishlistAsync(int userId, int bookId);
    Task RemoveFromWishlistAsync(int userId, int bookId);
}
