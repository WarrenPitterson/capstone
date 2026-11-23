namespace BookWorm.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int GiftPointsBalance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<UserFollowedAuthor> FollowedAuthors { get; set; } = new List<UserFollowedAuthor>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<GiftPointTransaction> GiftPointTransactions { get; set; } = new List<GiftPointTransaction>();
    public Cart? Cart { get; set; }
}

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
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

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
}

public class BookGenre
{
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public int GenreId { get; set; }
    public Genre? Genre { get; set; }
}

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string OtherWorks { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();
    public ICollection<UserFollowedAuthor> Followers { get; set; } = new List<UserFollowedAuthor>();
}

public class UserFollowedAuthor
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public int AuthorId { get; set; }
    public Author? Author { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Publisher
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public Author? Author { get; set; }
    public int PublisherId { get; set; }
    public Publisher? Publisher { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public BookFormat Format { get; set; }
    public string Language { get; set; } = "English";
    public decimal Price { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public string FrontCoverUrl { get; set; } = string.Empty;
    public string BackCoverUrl { get; set; } = string.Empty;
    public int CopiesSold { get; set; }
    public int StockQuantity { get; set; }
    public bool IsNewLaunch { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<RelatedBook> RelatedFromBooks { get; set; } = new List<RelatedBook>();
    public ICollection<RelatedBook> RelatedToBooks { get; set; } = new List<RelatedBook>();
}

public class RelatedBook
{
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public int RelatedBookId { get; set; }
    public Book? RelatedToBook { get; set; }
}

public class Review
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class WishlistItem
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string? CouponCode { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public Cart? Cart { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public int Quantity { get; set; }
}

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int AddressId { get; set; }
    public Address? Address { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal DeliveryCharge { get; set; }
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public int GiftPointsRedeemed { get; set; }
    public decimal Total { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
}

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Last4 { get; set; }
    public decimal Amount { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class GiftPointTransaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
