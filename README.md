# Book Worm — E-Commerce Bookstore Backend

Book Worm is a robust e-commerce bookstore backend built with **C# on .NET 8**, **ASP.NET Core Web API**, and **Microsoft SQL Server** via **Entity Framework Core**.

---

## 1. Architecture Overview

The solution adheres to clean layered architectural principles and domain-driven design standards:

- **Language & Runtime**: C# (.NET 8 SDK), ASP.NET Core Web API (Controllers pattern).
- **Controllers Layer (`src/BookWorm.Api/Controllers`)**: Exposes RESTful endpoints, handles HTTP binding, route validation, and returns strict DTO responses.
- **Services Layer (`src/BookWorm.Api/Services`)**: Implements core business logic, delivery estimations, discount computations, cancellation boundary validation, and mock payment gateway orchestration.
- **Data & Entity Framework Core Layer (`src/BookWorm.Api/Data`)**: Manages Microsoft SQL Server persistence using `Microsoft.EntityFrameworkCore.SqlServer`, entity type configurations (`IEntityTypeConfiguration`), migrations, and synthetic seed data.
- **Cross-Cutting Middleware (`src/BookWorm.Api/Middleware`)**: Global RFC 7807 `ProblemDetails` exception handling.
- **Security**: JWT Bearer token authentication for registered user journeys.

---

## 2. Key Business Rules & Features

- **Catalogue & Discovery**: Category hierarchy, keyword search, price range / language / format filters, sort criteria, "Recommended for You", "Bestsellers this Month", and "New Launches".
- **Product Detail**: Front and back cover art, copies sold counter, average ratings, About the Writer, reviews (100 char limit + 1–5 stars), and Related Reads.
- **Estimated Delivery Date**: Calculated dynamically as order date + 3 business days, skipping Sundays. Configurable via `Pricing:EstimatedDaysToDeliver`.
- **Pricing & Discounts**: Tax calculation at configurable rate (default 12.2%), free delivery threshold (default ₹500, else ₹50), active coupon discount validation, and gift points redemption (1 point = 1 INR).
- **Order Cancellation (48-Hour Boundary)**: Orders can be cancelled only within 48 hours of placement (`PlacedAt`). Cancellation automatically restores book stock quantities, decreases copies sold, refunds redeemed gift points, and issues gateway refunds.
- **Buy Again**: Re-adds all line items from a past order directly into the customer's active shopping cart.
- **Mock Payment Gateway**: Simulates Credit Card, Debit Card, UPI, and Wallet processing. Cards ending in `0000` are declined; all other valid inputs succeed. Only the last 4 card digits are stored; CVV and full card numbers are never logged or stored.

---

## 3. Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft SQL Server 2019+](https://www.microsoft.com/sql-server) (or Azure SQL / Docker MSSQL)
- [EF Core CLI Tools](https://learn.microsoft.com/ef/core/cli/dotnet) (`dotnet tool install --global dotnet-ef`)

### Local Setup (Running Against Microsoft SQL Server)

1. **Configure Connection String**:

   In `src/BookWorm.Api/appsettings.json` or as an environment variable:

   ```powershell
   $env:DatabaseConnectionString = "Server=127.0.0.1;Database=Capstone;User Id=SA;Password=23412adfasASD;TrustServerCertificate=True"
   ```

2. **Apply EF Core Migrations**:

   ```powershell
   dotnet ef database update --project src/BookWorm.Api
   ```

3. **Run the API**:

   ```powershell
   dotnet run --project src/BookWorm.Api
   ```

4. **Access Swagger UI & Health Check**:

   - Swagger UI: [http://localhost:5285/swagger](http://localhost:5285/swagger)
   - Health Check: [http://localhost:5285/health](http://localhost:5285/health)

---

## 4. Running with Docker & Docker Compose

1. **Copy the Environment Template**:

   ```powershell
   Copy-Item .env.example .env
   ```

2. **Start the Stack (MSSQL Server 2022 + Web API)**:

   ```powershell
   docker compose up --build -d
   ```

3. **Check Container Status**:

   ```powershell
   docker compose ps
   ```

---

## 5. Running Automated Tests

Run the full xUnit test suite (17 Unit Tests + 8 Integration Tests):

```powershell
dotnet test
```

### Test Coverage Highlights

- **Order Pricing & Discount Calculations**: Subtotal, tax, free vs standard delivery, coupon discount caps, and gift point bounds.
- **48-Hour Cancellation Boundary**: 47h 59m allowed vs 48h 01m rejected with `409 Conflict`.
- **Payment Gateway Outcomes**: Approval flow vs card ending in `0000` decline.
- **Customer Journey Integration**: Full flow from register/login $\rightarrow$ browse $\rightarrow$ cart $\rightarrow$ address & coupon $\rightarrow$ order $\rightarrow$ payment $\rightarrow$ cancel & buy-again.
- **Edge Cases**: Empty cart checkout, negative quantities, expired coupons, duplicate reviews, 100-character review limits, and unauthorized route access.

---

## 6. API Endpoint Summary

| Module | Method | Path | Description | Auth Required |
| :--- | :--- | :--- | :--- | :--- |
| **Health** | `GET` | `/health` | Service health status | No |
| **Auth** | `POST` | `/api/auth/register` | Register new user | No |
| **Auth** | `POST` | `/api/auth/login` | Login and obtain JWT token | No |
| **Catalogue** | `GET` | `/api/categories` | List all categories | No |
| **Catalogue** | `GET` | `/api/publishers` | List all publishers (brands) | No |
| **Catalogue** | `GET` | `/api/books` | Search and filter books | No |
| **Catalogue** | `GET` | `/api/books/{id}` | Book detail with related reads | No |
| **Catalogue** | `GET` | `/api/books/recommended` | Personalized / fallback recommendations | Optional |
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

## 7. Synthetic Seed Data

- **Test User**: `testuser@example.com` / `Password123!` (Balance: 100 Gift Points)
- **Active Coupons**:
  - `SAVE100`: ₹100 discount
  - `WELCOME50`: ₹50 discount
  - `BOOKWORM200`: ₹200 discount
- **Books**: ~20 books seeded across all categories and genres matching wireframe cards.
