using BookWorm.Api.Models;

namespace BookWorm.Api.Services.Interfaces;

public interface IAuthorService
{
    Task<AuthorDetailDto> GetAuthorByIdAsync(int authorId);
    Task<List<AuthorSummaryDto>> GetFollowedAuthorsAsync(int userId);
    Task FollowAuthorAsync(int userId, int authorId);
    Task UnfollowAuthorAsync(int userId, int authorId);
}
