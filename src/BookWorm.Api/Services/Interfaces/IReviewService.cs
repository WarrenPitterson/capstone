using BookWorm.Api.Models;

namespace BookWorm.Api.Services.Interfaces;

public interface IReviewService
{
    Task<List<ReviewDto>> GetBookReviewsAsync(int bookId);
    Task<ReviewDto> AddReviewAsync(int userId, int bookId, CreateReviewRequest request);
}
