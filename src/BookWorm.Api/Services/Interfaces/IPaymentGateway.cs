using BookWorm.Api.Entities;

namespace BookWorm.Api.Services.Interfaces;

public class PaymentGatewayResult
{
    public bool IsSuccess { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string? Last4 { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ProcessPaymentAsync(PaymentMethod method, decimal amount, string? cardNumber, string? nameOnCard, string? cvv, string? expiryMonthYear, string? upiId, string? walletName);
    Task<PaymentGatewayResult> RefundPaymentAsync(string transactionReference, decimal amount);
}
