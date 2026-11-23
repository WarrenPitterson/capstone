namespace BookWorm.Api.Entities;

public enum BookFormat
{
    Paperback,
    Hardcover,
    EBook
}

public enum OrderStatus
{
    Placed,
    Cancelled,
    Shipped,
    Delivered
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    Upi,
    Wallet
}

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Refunded
}
