using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookWorm.Api.Entities;
using BookWorm.Api.Models;
using Xunit;

namespace BookWorm.IntegrationTests;

public class SecurityAndEdgeCaseIntegrationTests : IClassFixture<BookWormTestFactory>
{
    private readonly HttpClient _client;

    public SecurityAndEdgeCaseIntegrationTests(BookWormTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProtectedRoutes_WithoutBearerToken_Returns401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var cartResponse = await _client.GetAsync("/api/cart");
        var ordersResponse = await _client.GetAsync("/api/orders");
        var wishlistResponse = await _client.GetAsync("/api/wishlist");
        var giftPointsResponse = await _client.GetAsync("/api/me/gift-points");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, cartResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, ordersResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wishlistResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, giftPointsResponse.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyCart_Returns400BadRequest()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "testuser@example.com",
            Password = "Password123!"
        }, TestJsonOptions.Default);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Ensure cart is empty by removing any existing items
        var cart = await _client.GetFromJsonAsync<CartDto>("/api/cart", TestJsonOptions.Default);
        if (cart?.Items != null)
        {
            foreach (var item in cart.Items)
            {
                await _client.DeleteAsync($"/api/cart/items/{item.BookId}");
            }
        }

        // Act
        var orderResponse = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest { AddressId = 1 }, TestJsonOptions.Default);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, orderResponse.StatusCode);
    }

    [Fact]
    public async Task AddToCart_UnknownBookId_Returns404NotFound()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "testuser@example.com",
            Password = "Password123!"
        }, TestJsonOptions.Default);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest { BookId = 99999, Quantity = 1 }, TestJsonOptions.Default);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_ZeroOrNegativeQuantity_Returns400BadRequest()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "testuser@example.com",
            Password = "Password123!"
        }, TestJsonOptions.Default);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest { BookId = 1, Quantity = 0 }, TestJsonOptions.Default);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_CardEndingIn0000_Returns409Conflict()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "testuser@example.com",
            Password = "Password123!"
        }, TestJsonOptions.Default);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        await _client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest { BookId = 1, Quantity = 1 }, TestJsonOptions.Default);
        var addresses = await _client.GetFromJsonAsync<List<AddressDto>>("/api/addresses", TestJsonOptions.Default);
        var orderResponse = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest { AddressId = addresses![0].Id }, TestJsonOptions.Default);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDetailDto>(TestJsonOptions.Default);

        var paymentRequest = new ProcessPaymentRequest
        {
            Method = PaymentMethod.CreditCard,
            CardNumber = "4111111111110000",
            NameOnCard = "Declined User",
            Cvv = "123",
            ExpiryMonthYear = "12/2028"
        };

        // Act
        var paymentResponse = await _client.PostAsJsonAsync($"/api/orders/{order!.Id}/payments", paymentRequest, TestJsonOptions.Default);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, paymentResponse.StatusCode);
    }
}
