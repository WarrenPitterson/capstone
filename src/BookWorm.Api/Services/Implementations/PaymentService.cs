using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Exceptions;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookWorm.Api.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly BookWormDbContext _dbContext;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        BookWormDbContext dbContext,
        IPaymentGateway paymentGateway,
        ILogger<PaymentService> logger)
    {
        _dbContext = dbContext;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<List<PaymentDto>> GetOrderPaymentsAsync(int userId, int orderId)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found.");
        }

        return await _dbContext.Payments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToPaymentDto(p))
            .ToListAsync();
    }

    public async Task<PaymentDto> ProcessOrderPaymentAsync(int userId, int orderId, ProcessPaymentRequest request)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new ConflictException("Cannot process payment for a cancelled order.");
        }

        var alreadyPaid = order.Payments.Any(p => p.Status == PaymentStatus.Succeeded);
        if (alreadyPaid)
        {
            throw new ConflictException("Order has already been paid for.");
        }

        var result = await _paymentGateway.ProcessPaymentAsync(
            request.Method,
            order.Total,
            request.CardNumber,
            request.NameOnCard,
            request.Cvv,
            request.ExpiryMonthYear,
            request.UpiId,
            request.WalletName);

        var payment = new Payment
        {
            OrderId = orderId,
            Method = request.Method,
            Status = result.IsSuccess ? PaymentStatus.Succeeded : PaymentStatus.Failed,
            Last4 = result.Last4,
            Amount = order.Total,
            TransactionReference = result.TransactionReference.Length > 0 ? result.TransactionReference : $"FAIL_{Guid.NewGuid():N}"[..20],
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Payments.AddAsync(payment);
        await _dbContext.SaveChangesAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Payment failed for Order {OrderId}: {ErrorMessage}", orderId, result.ErrorMessage);
            throw new ConflictException(result.ErrorMessage ?? "Payment processing failed.");
        }

        _logger.LogInformation("Payment {PaymentId} succeeded for Order {OrderId}", payment.Id, orderId);

        return MapToPaymentDto(payment);
    }

    private static PaymentDto MapToPaymentDto(Payment p)
    {
        return new PaymentDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            Method = p.Method,
            Status = p.Status,
            Last4 = p.Last4,
            Amount = p.Amount,
            TransactionReference = p.TransactionReference,
            CreatedAt = p.CreatedAt
        };
    }
}
