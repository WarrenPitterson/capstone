using BookWorm.Api.Models;

namespace BookWorm.Api.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddItemToCartAsync(int userId, AddCartItemRequest request);
    Task<CartDto> UpdateItemQuantityAsync(int userId, int bookId, UpdateCartItemRequest request);
    Task<CartDto> RemoveItemFromCartAsync(int userId, int bookId);
    Task<CartDto> ApplyCouponAsync(int userId, ApplyCouponRequest request);
}
