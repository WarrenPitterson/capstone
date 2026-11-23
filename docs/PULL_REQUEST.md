# Pull Request: Implement C# / ASP.NET Core 8 Web API for Book Worm E-Commerce

## PR Title
`feat(backend): implement C# .NET 8 Web API and Microsoft SQL Server backend for Book Worm`

## Target Branch
- **Base**: `main`
- **Head (Compare)**: `feature/api-implementation`

---

## Description & Summary of Changes

This Pull Request delivers the backend implementation for **Book Worm**, an online e-commerce bookstore, following the architecture, user journeys, and wireframes specified in the Capstone brief.

The solution is implemented in **C# on .NET 8**, using **ASP.NET Core Web API (Controllers pattern)** and **Microsoft SQL Server** via **Entity Framework Core**.

---

## 1. Key Features & Business Logic Implemented

- **Catalogue & Search**:
  - Filterable by Category, Publisher (Brand), Language, Format (Paperback, Hardcover, EBook), Price range (min/max), and Sort criteria (Relevance, PriceAsc, PriceDesc, Newest, Rating).
  - Dynamic discovery endpoints: `/api/books/recommended`, `/api/books/bestsellers`, `/api/books/new-launches`.
- **Product Detail**:
  - Full metadata with front/back cover art, copies sold counter, average ratings, About the Writer bio, reviews list, and Related Reads.
- **Estimated Delivery Date Engine**:
  - Dynamically computes order date + 3 business days, automatically skipping Sundays (`DeliveryEstimator.cs`).
- **Cart & Order Calculations**:
  - Subtotal, configurable tax (12.2%), free delivery threshold (₹500, else ₹50), active coupon discount validation (`SAVE100`, `WELCOME50`, `BOOKWORM200`), and gift points redemption (1 point = 1 INR).
- **48-Hour Order Cancellation Rule**:
  - Orders can be cancelled only within 48 hours of placement (`PlacedAt`).
  - Cancellation restores book inventory (`StockQuantity`), decreases copies sold, refunds redeemed gift points, and refunds payment transactions.
- **Buy Again**:
  - Re-adds all line items from a past order directly into the customer's active shopping cart (`/api/orders/{id}/buy-again`).
- **Mock Payment Gateway**:
  - Simulates Credit Card, Debit Card, UPI, and Wallet processing (`IPaymentGateway`).
  - Cards ending in `0000` trigger an issuing bank decline; all other valid inputs succeed. Full card numbers and CVV are never stored (stores only `Last4`).
- **Reviews & Community**:
  - Enforces 1–5 star ratings, maximum 100 character review length, and one review per user per book.
- **Writers / Followed Authors**:
  - "My Writers" endpoints to follow/unfollow authors and list followed writers.

---

## 2. API Endpoint Documentation

| Module | Method | Path | Description | Auth Required |
| :--- | :--- | :--- | :--- | :--- |
| **Health** | `GET` | `/health` | Health check endpoint | No |
| **Auth** | `POST` | `/api/auth/register` | Register new user | No |
| **Auth** | `POST` | `/api/auth/login` | Login and obtain JWT token | No |
| **Catalogue** | `GET` | `/api/categories` | List all book categories | No |
| **Catalogue** | `GET` | `/api/publishers` | List all publishers (brands) | No |
| **Catalogue** | `GET` | `/api/books` | Search, filter & paginate books | No |
| **Catalogue** | `GET` | `/api/books/{id}` | Book detail with related reads | No |
| **Catalogue** | `GET` | `/api/books/recommended` | Recommendations with bestseller fallback | Optional |
| **Catalogue** | `GET` | `/api/books/bestsellers` | Bestsellers this month | No |
| **Catalogue** | `GET` | `/api/books/new-launches` | New book launches | No |
| **Reviews** | `GET` | `/api/books/{id}/reviews` | List reviews for a book | No |
| **Reviews** | `POST` | `/api/books/{id}/reviews` | Submit a review (1–5 stars, max 100 chars) | Yes |
| **Wishlist** | `GET` | `/api/wishlist` | Get user's wishlist | Yes |
| **Wishlist** | `PUT` | `/api/wishlist/{bookId}` | Add book to wishlist | Yes |
| **Wishlist** | `DELETE` | `/api/wishlist/{bookId}` | Remove book from wishlist | Yes |
| **Authors** | `GET` | `/api/authors/{id}` | Author bio & published works | No |
| **Writers** | `GET` | `/api/me/writers` | Get followed authors (My Writers) | Yes |
| **Writers** | `PUT` | `/api/me/writers/{authorId}` | Follow an author | Yes |
| **Writers** | `DELETE` | `/api/me/writers/{authorId}` | Unfollow an author | Yes |
| **Cart** | `GET` | `/api/cart` | Get shopping cart with price breakdown | Yes |
| **Cart** | `POST` | `/api/cart/items` | Add book to cart | Yes |
| **Cart** | `PUT` | `/api/cart/items/{bookId}` | Update item quantity in cart | Yes |
| **Cart** | `DELETE` | `/api/cart/items/{bookId}` | Remove item from cart | Yes |
| **Cart** | `POST` | `/api/cart/coupon` | Apply coupon code | Yes |
| **Addresses** | `GET` | `/api/addresses` | Get saved user delivery addresses | Yes |
| **Addresses** | `POST` | `/api/addresses` | Save new delivery address | Yes |
| **Addresses** | `PUT` | `/api/addresses/{id}` | Update delivery address | Yes |
| **Addresses** | `DELETE` | `/api/addresses/{id}` | Delete delivery address | Yes |
| **Orders** | `GET` | `/api/orders` | Order history (My Orders) | Yes |
| **Orders** | `POST` | `/api/orders` | Checkout cart into placed order | Yes |
| **Orders** | `GET` | `/api/orders/{id}` | Get order details | Yes |
| **Orders** | `POST` | `/api/orders/{id}/cancel` | Cancel order within 48 hours | Yes |
| **Orders** | `POST` | `/api/orders/{id}/buy-again` | Re-add past order items to cart | Yes |
| **Payments** | `GET` | `/api/orders/{id}/payments` | List payment transactions for order | Yes |
| **Payments** | `POST` | `/api/orders/{id}/payments` | Process payment via Mock Gateway | Yes |
| **Gift Points** | `GET` | `/api/me/gift-points` | View gift points balance & history | Yes |

---

## 3. Test Verification & Results

- **Unit Tests (xUnit + Moq)**: 17/17 Passing
  - Calculations for pricing, tax, free delivery thresholds, coupon discounts, gift points redemption
  - 48-Hour cancellation boundary tests (47h59m allowed vs 48h01m rejected)
  - Mock payment gateway approval and decline outcomes
  - Recommendation engine fallback rules
  - Review constraints (100 char limit, 1–5 stars, duplicate review rejection)
- **Integration Tests (WebApplicationFactory)**: 8/8 Passing
  - Full end-to-end customer journey from registration $\rightarrow$ browse $\rightarrow$ cart $\rightarrow$ address & coupon $\rightarrow$ order $\rightarrow$ payment $\rightarrow$ cancel & buy-again
  - Unauthenticated access rejection (401 Unauthorized)
  - Edge cases (empty cart checkout, negative quantities, expired coupon, gift points above balance)

```
Total Test Results: 25 Passed, 0 Failed, 0 Skipped
```

---

## 4. Deliverables Included in this PR

- [`openapi.yaml`](openapi.yaml) — OpenAPI 3.0 specification
- [`src/BookWorm.Api`](src/BookWorm.Api/Program.cs) — ASP.NET Core 8 Web API solution
- [`tests/BookWorm.UnitTests`](tests/BookWorm.UnitTests) — xUnit unit test project
- [`tests/BookWorm.IntegrationTests`](tests/BookWorm.IntegrationTests) — xUnit integration test project
- [`BookWorm.http`](BookWorm.http) — REST Client test collection
- [`Dockerfile`](Dockerfile) & [`docker-compose.yml`](docker-compose.yml) — Containerization
- [`README.md`](README.md) & [`docs/ai-usage.md`](docs/ai-usage.md) — Documentation & AI prompt log
