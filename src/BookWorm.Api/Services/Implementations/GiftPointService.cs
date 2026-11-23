using BookWorm.Api.Data;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Api.Services.Implementations;

public class GiftPointService : IGiftPointService
{
    private readonly BookWormDbContext _dbContext;

    public GiftPointService(BookWormDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GiftPointsSummaryDto> GetUserGiftPointsAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found.");
        }

        var transactions = await _dbContext.GiftPointTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new GiftPointTransactionDto
            {
                Id = t.Id,
                Points = t.Points,
                Reason = t.Reason,
                OrderId = t.OrderId,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new GiftPointsSummaryDto
        {
            Balance = user.GiftPointsBalance,
            Transactions = transactions
        };
    }
}
