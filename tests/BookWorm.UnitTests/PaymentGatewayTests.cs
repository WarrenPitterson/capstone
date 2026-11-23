using BookWorm.Api.Entities;
using BookWorm.Api.Services.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookWorm.UnitTests;

public class PaymentGatewayTests
{
    private readonly Mock<ILogger<MockPaymentGateway>> _loggerMock;
    private readonly MockPaymentGateway _sut;

    public PaymentGatewayTests()
    {
        _loggerMock = new Mock<ILogger<MockPaymentGateway>>();
        _sut = new MockPaymentGateway(_loggerMock.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_CardNotEndingIn0000_ApprovesPayment()
    {
        // Arrange
        var cardNumber = "4111 1111 1111 1234";

        // Act
        var result = await _sut.ProcessPaymentAsync(PaymentMethod.CreditCard, 470.00m, cardNumber, "John Doe", "123", "12/28", null, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("1234", result.Last4);
        Assert.StartsWith("TXN_", result.TransactionReference);
    }

    [Fact]
    public async Task ProcessPaymentAsync_CardEndingIn0000_DeclinesPayment()
    {
        // Arrange
        var cardNumber = "4111 1111 1111 0000";

        // Act
        var result = await _sut.ProcessPaymentAsync(PaymentMethod.CreditCard, 470.00m, cardNumber, "John Doe", "123", "12/28", null, null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("0000", result.Last4);
        Assert.NotNull(result.ErrorMessage);
    }
}
