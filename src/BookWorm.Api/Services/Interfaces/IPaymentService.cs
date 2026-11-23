using BookWorm.Api.Models;

namespace BookWorm.Api.Services.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetOrderPaymentsAsync(int userId, int orderId);
    Task<PaymentDto> ProcessOrderPaymentAsync(int userId, int orderId, ProcessPaymentRequest request);
}
