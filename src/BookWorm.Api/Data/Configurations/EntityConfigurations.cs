using BookWorm.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(20);
    }
}

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.LastName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(256).IsRequired();
        builder.Property(a => a.Line1).HasMaxLength(256).IsRequired();
        builder.Property(a => a.Line2).HasMaxLength(256);
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Pin).HasMaxLength(20).IsRequired();
        builder.Property(a => a.State).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Country).HasMaxLength(100).IsRequired();
        builder.Property(a => a.PhoneCountryCode).HasMaxLength(10).IsRequired();
        builder.Property(a => a.Phone).HasMaxLength(20).IsRequired();

        builder.HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Name).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
    }
}

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.HasKey(g => g.Id);
        builder.HasIndex(g => g.Name).IsUnique();
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
    }
}

public class BookGenreConfiguration : IEntityTypeConfiguration<BookGenre>
{
    public void Configure(EntityTypeBuilder<BookGenre> builder)
    {
        builder.HasKey(bg => new { bg.BookId, bg.GenreId });

        builder.HasOne(bg => bg.Book)
            .WithMany(b => b.BookGenres)
            .HasForeignKey(bg => bg.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bg => bg.Genre)
            .WithMany(g => g.BookGenres)
            .HasForeignKey(bg => bg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Bio).HasMaxLength(2000);
        builder.Property(a => a.PhotoUrl).HasMaxLength(500);
        builder.Property(a => a.OtherWorks).HasMaxLength(1000);
    }
}

public class UserFollowedAuthorConfiguration : IEntityTypeConfiguration<UserFollowedAuthor>
{
    public void Configure(EntityTypeBuilder<UserFollowedAuthor> builder)
    {
        builder.HasKey(fa => new { fa.UserId, fa.AuthorId });

        builder.HasOne(fa => fa.User)
            .WithMany(u => u.FollowedAuthors)
            .HasForeignKey(fa => fa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fa => fa.Author)
            .WithMany(a => a.Followers)
            .HasForeignKey(fa => fa.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
{
    public void Configure(EntityTypeBuilder<Publisher> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Name).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
    }
}

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Title).HasMaxLength(250).IsRequired();
        builder.Property(b => b.Description).HasMaxLength(4000);
        builder.Property(b => b.Format).HasConversion<string>().HasMaxLength(50);
        builder.Property(b => b.Language).HasMaxLength(50).IsRequired();
        builder.Property(b => b.Price).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(b => b.Isbn).HasMaxLength(50);
        builder.Property(b => b.FrontCoverUrl).HasMaxLength(500);
        builder.Property(b => b.BackCoverUrl).HasMaxLength(500);

        builder.HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RelatedBookConfiguration : IEntityTypeConfiguration<RelatedBook>
{
    public void Configure(EntityTypeBuilder<RelatedBook> builder)
    {
        builder.HasKey(rb => new { rb.BookId, rb.RelatedBookId });

        builder.HasOne(rb => rb.Book)
            .WithMany(b => b.RelatedFromBooks)
            .HasForeignKey(rb => rb.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rb => rb.RelatedToBook)
            .WithMany(b => b.RelatedToBooks)
            .HasForeignKey(rb => rb.RelatedBookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.BookId, r.UserId }).IsUnique();
        builder.Property(r => r.Text).HasMaxLength(100).IsRequired();

        builder.HasOne(r => r.Book)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasKey(w => new { w.UserId, w.BookId });

        builder.HasOne(w => w.User)
            .WithMany(u => u.WishlistItems)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Book)
            .WithMany()
            .HasForeignKey(w => w.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.UserId).IsUnique();
        builder.Property(c => c.CouponCode).HasMaxLength(50);

        builder.HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => ci.Id);
        builder.HasIndex(ci => new { ci.CartId, ci.BookId }).IsUnique();

        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Book)
            .WithMany()
            .HasForeignKey(ci => ci.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.DiscountAmount).HasColumnType("decimal(12,2)").IsRequired();
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(o => o.Subtotal).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(o => o.Tax).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(o => o.DeliveryCharge).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(o => o.CouponCode).HasMaxLength(50);
        builder.Property(o => o.Discount).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(o => o.Total).HasColumnType("decimal(12,2)").IsRequired();

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Address)
            .WithMany()
            .HasForeignKey(o => o.AddressId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.UnitPrice).HasColumnType("decimal(12,2)").IsRequired();

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Book)
            .WithMany()
            .HasForeignKey(oi => oi.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.Last4).HasMaxLength(4);
        builder.Property(p => p.Amount).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(p => p.TransactionReference).HasMaxLength(100).IsRequired();

        builder.HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GiftPointTransactionConfiguration : IEntityTypeConfiguration<GiftPointTransaction>
{
    public void Configure(EntityTypeBuilder<GiftPointTransaction> builder)
    {
        builder.HasKey(gpt => gpt.Id);
        builder.Property(gpt => gpt.Reason).HasMaxLength(250).IsRequired();

        builder.HasOne(gpt => gpt.User)
            .WithMany(u => u.GiftPointTransactions)
            .HasForeignKey(gpt => gpt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gpt => gpt.Order)
            .WithMany()
            .HasForeignKey(gpt => gpt.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
