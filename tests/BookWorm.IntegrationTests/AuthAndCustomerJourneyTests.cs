using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookWorm.Api.Entities;
using BookWorm.Api.Models;
using Xunit;

namespace BookWorm.IntegrationTests;

public class AuthAndCustomerJourneyTests : IClassFixture<BookWormTestFactory>
{
    private readonly HttpClient _client;

    public AuthAndCustomerJourneyTests(BookWormTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CustomerJourney_FromRegistrationToPaymentAndOrderHistory_CompletesSuccessfully()
    {
        // Arrange & Act 1: Health check
        var healthResponse = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);

        // Arrange & Act 2: Register a new user
        var registerRequest = new RegisterRequest
        {
            Email = $"journey_{Guid.NewGuid():N}@example.com",
            Password = "Password123!",
            FirstName = "Alice",
            LastName = "Wonder",
            Phone = "9876543210"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest, TestJsonOptions.Default);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        Assert.NotNull(authResult);
        Assert.NotEmpty(authResult.Token);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);

        // Arrange & Act 3: Browse categories and catalogue
        var categoriesResponse = await _client.GetAsync("/api/categories");
        Assert.Equal(HttpStatusCode.OK, categoriesResponse.StatusCode);

        var booksResponse = await _client.GetAsync("/api/books?search=Minimalism");
        Assert.Equal(HttpStatusCode.OK, booksResponse.StatusCode);
        var books = await booksResponse.Content.ReadFromJsonAsync<PaginatedBooksResponse>(TestJsonOptions.Default);
        Assert.NotNull(books);
        Assert.NotEmpty(books.Items);

        var bookId = books.Items[0].Id;

        // Arrange & Act 4: Get Product Detail
        var detailResponse = await _client.GetAsync($"/api/books/{bookId}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<BookDetailDto>(TestJsonOptions.Default);
        Assert.NotNull(detail);
        Assert.Equal(bookId, detail.Id);

        // Arrange & Act 5: Add to Wishlist and Follow Author
        var wishlistResponse = await _client.PutAsync($"/api/wishlist/{bookId}", null);
        Assert.Equal(HttpStatusCode.NoContent, wishlistResponse.StatusCode);

        var authorId = detail.Author.Id;
        var followAuthorResponse = await _client.PutAsync($"/api/me/writers/{authorId}", null);
        Assert.Equal(HttpStatusCode.NoContent, followAuthorResponse.StatusCode);

        var myWritersResponse = await _client.GetAsync("/api/me/writers");
        Assert.Equal(HttpStatusCode.OK, myWritersResponse.StatusCode);

        // Arrange & Act 6: Add items to Cart
        var addCartResponse = await _client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest
        {
            BookId = bookId,
            Quantity = 2
        }, TestJsonOptions.Default);
        Assert.Equal(HttpStatusCode.OK, addCartResponse.StatusCode);

        // Arrange & Act 7: Apply Coupon
        var couponResponse = await _client.PostAsJsonAsync("/api/cart/coupon", new ApplyCouponRequest
        {
            CouponCode = "SAVE100"
        }, TestJsonOptions.Default);
        Assert.Equal(HttpStatusCode.OK, couponResponse.StatusCode);
        var cartWithCoupon = await couponResponse.Content.ReadFromJsonAsync<CartDto>(TestJsonOptions.Default);
        Assert.NotNull(cartWithCoupon);
        Assert.Equal(100.00m, cartWithCoupon.Discount);

        // Arrange & Act 8: Create Address
        var addressRequest = new CreateAddressRequest
        {
            FirstName = "Alice",
            LastName = "Wonder",
            Email = registerRequest.Email,
            Line1 = "123 Wonderland Ave",
            City = "Mumbai",
            Pin = "400001",
            State = "Maharashtra",
            Country = "India",
            Phone = "9876543210",
            IsSaved = true
        };
        var addressResponse = await _client.PostAsJsonAsync("/api/addresses", addressRequest, TestJsonOptions.Default);
        Assert.Equal(HttpStatusCode.Created, addressResponse.StatusCode);
        var address = await addressResponse.Content.ReadFromJsonAsync<AddressDto>(TestJsonOptions.Default);
        Assert.NotNull(address);

        // Arrange & Act 9: Checkout / Place Order
        var orderResponse = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest
        {
            AddressId = address.Id,
            CouponCode = "SAVE100"
        }, TestJsonOptions.Default);
        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDetailDto>(TestJsonOptions.Default);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Placed, order.Status);

        // Arrange & Act 10: Process Payment
        var paymentRequest = new ProcessPaymentRequest
        {
            Method = PaymentMethod.CreditCard,
            CardNumber = "4111111111111234",
            NameOnCard = "Alice Wonder",
            Cvv = "123",
            ExpiryMonthYear = "12/2028"
        };
        var paymentResponse = await _client.PostAsJsonAsync($"/api/orders/{order.Id}/payments", paymentRequest, TestJsonOptions.Default);
        Assert.Equal(HttpStatusCode.OK, paymentResponse.StatusCode);
        var payment = await paymentResponse.Content.ReadFromJsonAsync<PaymentDto>(TestJsonOptions.Default);
        Assert.NotNull(payment);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);

        // Arrange & Act 11: Get Order History
        var ordersResponse = await _client.GetAsync("/api/orders");
        Assert.Equal(HttpStatusCode.OK, ordersResponse.StatusCode);
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<OrderSummaryDto>>(TestJsonOptions.Default);
        Assert.NotNull(orders);
        Assert.NotEmpty(orders);

        // Arrange & Act 12: Buy Again
        var buyAgainResponse = await _client.PostAsync($"/api/orders/{order.Id}/buy-again", null);
        Assert.Equal(HttpStatusCode.OK, buyAgainResponse.StatusCode);
    }
}
