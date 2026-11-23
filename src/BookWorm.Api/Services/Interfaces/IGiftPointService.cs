using BookWorm.Api.Models;

namespace BookWorm.Api.Services.Interfaces;

public interface IGiftPointService
{
    Task<GiftPointsSummaryDto> GetUserGiftPointsAsync(int userId);
}
