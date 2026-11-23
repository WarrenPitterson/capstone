using BookWorm.Api.Entities;
using BookWorm.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookWorm.Api.Services.Implementations;

public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<PaymentGatewayResult> ProcessPaymentAsync(
        PaymentMethod method,
        decimal amount,
        string? cardNumber,
        string? nameOnCard,
        string? cvv,
        string? expiryMonthYear,
        string? upiId,
        string? walletName)
    {
        var cleanCard = cardNumber?.Replace(" ", "").Replace("-", "").Trim();
        var last4 = cleanCard != null && cleanCard.Length >= 4 ? cleanCard[^4..] : null;

        if (cleanCard != null && cleanCard.EndsWith("0000"))
        {
            _logger.LogWarning("Mock payment declined for method {PaymentMethod} with amount {Amount}", method, amount);
            return Task.FromResult(new PaymentGatewayResult
            {
                IsSuccess = false,
                Last4 = last4,
                ErrorMessage = "Payment was declined by issuing bank (test card ending in 0000)."
            });
        }

        var transactionRef = $"TXN_{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        _logger.LogInformation("Mock payment succeeded: TxRef {TxRef} for method {PaymentMethod} with amount {Amount}", transactionRef, method, amount);

        return Task.FromResult(new PaymentGatewayResult
        {
            IsSuccess = true,
            TransactionReference = transactionRef,
            Last4 = last4
        });
    }

    public Task<PaymentGatewayResult> RefundPaymentAsync(string transactionReference, decimal amount)
    {
        var refundRef = $"REF_{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        _logger.LogInformation("Mock refund processed: RefundRef {RefundRef} for TxRef {TxRef} with amount {Amount}", refundRef, transactionReference, amount);

        return Task.FromResult(new PaymentGatewayResult
        {
            IsSuccess = true,
            TransactionReference = refundRef
        });
    }
}
