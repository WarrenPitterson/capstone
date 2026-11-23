using BookWorm.Api.Models;

namespace BookWorm.Api.Services.Interfaces;

public interface IOrderService
{
    Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId);
    Task<OrderDetailDto> GetOrderByIdAsync(int userId, int orderId);
    Task<OrderDetailDto> CreateOrderAsync(int userId, CreateOrderRequest request);
    Task<OrderDetailDto> CancelOrderAsync(int userId, int orderId);
    Task<CartDto> BuyAgainAsync(int userId, int orderId);
}
