using System.ComponentModel.DataAnnotations;
using BookWorm.Api.Entities;

namespace BookWorm.Api.Models;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? Phone { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int GiftPointsBalance { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PublisherDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AuthorSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
}

public class AuthorDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string OtherWorks { get; set; } = string.Empty;
    public List<BookSummaryDto> Books { get; set; } = new();
}

public class BookSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string PublisherName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public BookFormat Format { get; set; }
    public string Language { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string FrontCoverUrl { get; set; } = string.Empty;
    public string BackCoverUrl { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int CopiesSold { get; set; }
    public bool IsNewLaunch { get; set; }
    public List<string> GenreTags { get; set; } = new();
    public DateTime EstimatedDeliveryDate { get; set; }
}

public class BookDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AuthorSummaryDto Author { get; set; } = null!;
    public PublisherDto Publisher { get; set; } = null!;
    public CategoryDto Category { get; set; } = null!;
    public BookFormat Format { get; set; }
    public string Language { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public string FrontCoverUrl { get; set; } = string.Empty;
    public string BackCoverUrl { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int CopiesSold { get; set; }
    public int StockQuantity { get; set; }
    public bool IsNewLaunch { get; set; }
    public List<string> GenreTags { get; set; } = new();
    public DateTime EstimatedDeliveryDate { get; set; }
    public List<BookSummaryDto> RelatedReads { get; set; } = new();
}

public class BookQueryParameters
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? PublisherId { get; set; }
    public string? Language { get; set; }
    public BookFormat? Format { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Sort { get; set; } = "Relevance";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class PaginatedBooksResponse
{
    public List<BookSummaryDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 1));
}

public class ReviewDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewRequest
{
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Required]
    [MaxLength(100, ErrorMessage = "Review text cannot exceed 100 characters.")]
    public string Text { get; set; } = string.Empty;
}

public class CartItemDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public BookFormat Format { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string FrontCoverUrl { get; set; } = string.Empty;
    public List<string> GenreTags { get; set; } = new();
    public DateTime EstimatedDeliveryDate { get; set; }
    public decimal LineTotal => Price * Quantity;
}

public class CartDto
{
    public int Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal DeliveryCharge { get; set; }
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}

public class AddCartItemRequest
{
    [Required]
    public int BookId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 1.")]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 1.")]
    public int Quantity { get; set; }
}

public class ApplyCouponRequest
{
    [Required]
    public string CouponCode { get; set; } = string.Empty;
}

public class AddressDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneCountryCode { get; set; } = "+91";
    public string Phone { get; set; } = string.Empty;
    public bool IsSaved { get; set; }
}

public class CreateAddressRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Line1 { get; set; } = string.Empty;

    public string? Line2 { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Pin { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    public string PhoneCountryCode { get; set; } = "+91";

    [Required]
    public string Phone { get; set; } = string.Empty;

    public bool IsSaved { get; set; }
}

public class OrderItemDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string FrontCoverUrl { get; set; } = string.Empty;
    public BookFormat Format { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public DateTime EstimatedDeliveryDate { get; set; }
}

public class OrderSummaryDto
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime PlacedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderDetailDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AddressDto Address { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal DeliveryCharge { get; set; }
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public int GiftPointsRedeemed { get; set; }
    public decimal Total { get; set; }
    public DateTime PlacedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public PaymentDto? Payment { get; set; }
}

public class CreateOrderRequest
{
    public int? AddressId { get; set; }
    public CreateAddressRequest? NewAddress { get; set; }

    [Range(0, int.MaxValue)]
    public int GiftPointsToRedeem { get; set; } = 0;

    public string? CouponCode { get; set; }
}

public class ProcessPaymentRequest
{
    [Required]
    public PaymentMethod Method { get; set; }

    public string? CardNumber { get; set; }
    public string? NameOnCard { get; set; }
    public string? Cvv { get; set; }
    public string? ExpiryMonthYear { get; set; }
    public string? UpiId { get; set; }
    public string? WalletName { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Last4 { get; set; }
    public decimal Amount { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GiftPointTransactionDto
{
    public int Id { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GiftPointsSummaryDto
{
    public int Balance { get; set; }
    public List<GiftPointTransactionDto> Transactions { get; set; } = new();
}
