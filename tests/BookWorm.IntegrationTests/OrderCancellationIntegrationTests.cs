using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookWorm.Api.Data;
using BookWorm.Api.Entities;
using BookWorm.Api.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookWorm.IntegrationTests;

public class OrderCancellationIntegrationTests : IClassFixture<BookWormTestFactory>
{
    private readonly BookWormTestFactory _factory;
    private readonly HttpClient _client;

    public OrderCancellationIntegrationTests(BookWormTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CancelOrder_Within48Hours_SucceedsAndUpdatesStatus()
    {
        // Arrange: Login as test user
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "testuser@example.com",
            Password = "Password123!"
        }, TestJsonOptions.Default);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Add to cart & checkout
        await _client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest { BookId = 1, Quantity = 1 }, TestJsonOptions.Default);
        var addresses = await _client.GetFromJsonAsync<List<AddressDto>>("/api/addresses", TestJsonOptions.Default);
        var orderResponse = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest { AddressId = addresses![0].Id }, TestJsonOptions.Default);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDetailDto>(TestJsonOptions.Default);

        // Act: Cancel within 48h
        var cancelResponse = await _client.PostAsync($"/api/orders/{order!.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelledOrder = await cancelResponse.Content.ReadFromJsonAsync<OrderDetailDto>(TestJsonOptions.Default);
        Assert.Equal(OrderStatus.Cancelled, cancelledOrder!.Status);
        Assert.NotNull(cancelledOrder.CancelledAt);
    }

    [Fact]
    public async Task CancelOrder_After48Hours_Returns409Conflict()
    {
        // Arrange: Login as test user
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "testuser@example.com",
            Password = "Password123!"
        }, TestJsonOptions.Default);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Add to cart & checkout
        await _client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest { BookId = 2, Quantity = 1 }, TestJsonOptions.Default);
        var addresses = await _client.GetFromJsonAsync<List<AddressDto>>("/api/addresses", TestJsonOptions.Default);
        var orderResponse = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest { AddressId = addresses![0].Id }, TestJsonOptions.Default);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDetailDto>(TestJsonOptions.Default);

        // Manually adjust placed date in database to 49 hours ago
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BookWormDbContext>();
            var dbOrder = await db.Orders.FindAsync(order!.Id);
            dbOrder!.PlacedAt = DateTime.UtcNow.AddHours(-49);
            await db.SaveChangesAsync();
        }

        // Act: Attempt to cancel
        var cancelResponse = await _client.PostAsync($"/api/orders/{order!.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, cancelResponse.StatusCode);
    }
}
