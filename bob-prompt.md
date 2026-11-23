# Bob prompts: Book Worm e-bookstore backend (C# / ASP.NET Core / PostgreSQL)

How to use: attach the wireframe provided images paste Part 0, then run Stages 1-4 one at a time. Review and correct between stages. Keep the corrections, they are your human-in-the-loop evidence for the video and `docs/ai-usage.md`.

The data model section in Part 0 is a DRAFT derived from the wireframes. Slide 11 says the data model is manual, so review it, change it, and make it yours before pasting.

---

## Part 0: Shared context (paste first, once)

```
You are helping me build the backend for "Book Worm", an e-commerce bookstore. The attached wireframes show: architecture (image5), catalogue home (image6/8), product detail (image10), cart + checkout (image11), payment (image12) and purchase confirmation (image13).

STACK (decided, do not change)
- C# on .NET 8, ASP.NET Core Web API (controllers, not minimal APIs)
- PostgreSQL via EF Core + Npgsql, schema managed by EF Core migrations generated with `dotnet ef`
- JWT bearer auth. Guests can browse the catalogue; only registered users can use cart, checkout, orders, wishlist, reviews
- Swagger/OpenAPI via Swashbuckle, health check at /health
- Tests: xUnit + Moq, plus integration tests with WebApplicationFactory
- Currency is INR. Money is decimal(12,2), never float
- Secrets and connection strings come from environment variables / user-secrets, never committed

ARCHITECTURE (from image5): modules Member, Store, Catalog, Order, Payment, Shipping. Guest and registered users reach the home page, then Browse, Add to Cart, Payment Processing, Shipment.

DRAFT DATA MODEL (author's to review)
- User(Id, Email unique, PasswordHash, FirstName, LastName, Phone, GiftPointsBalance, CreatedAt)
- Address(Id, UserId, FirstName, LastName, Email, Line1, Line2, City, Pin, State, Country, PhoneCountryCode, Phone, IsSaved)
- Category(Id, Name unique) e.g. Romance, Mystery, Science Fiction, Fantasy, Historical, Biography, Self-help, Memoir, Travel, Cooking, Children's, Young Adult, Comics & Graphic Novels, Poetry, Drama, Science, Philosophy, Religion, Language Learning
- Genre tag (e.g. Non-fiction, Fiction, Thriller, Horror, Love, Children): many-to-many with Book
- Author(Id, Name, Bio, PhotoUrl, OtherWorks)
- Publisher(Id, Name)  // wireframe shows "Published by"; slide 6 says "browse the brands", so Publisher is the brand
- Book(Id, Title, Description, AuthorId, PublisherId, CategoryId, Format [Paperback|Hardcover|EBook], Language, Price, Isbn, FrontCoverUrl, BackCoverUrl, CopiesSold, StockQuantity, IsNewLaunch, CreatedAt)
- RelatedBook(BookId, RelatedBookId)
- Review(Id, BookId, UserId, Rating 1-5, Text max 100 chars, CreatedAt)
- WishlistItem(UserId, BookId)
- Cart(Id, UserId) and CartItem(CartId, BookId, Quantity >= 1)
- Coupon(Code unique, DiscountAmount, ExpiresAt, IsActive)
- Order(Id, UserId, AddressId, Status [Placed|Cancelled|Shipped|Delivered], Subtotal, Tax, DeliveryCharge, CouponCode, Discount, GiftPointsRedeemed, Total, PlacedAt, CancelledAt)
- OrderItem(OrderId, BookId, UnitPrice, Quantity, EstimatedDeliveryDate)
- Payment(Id, OrderId, Method [CreditCard|DebitCard|Upi|Wallet], Status [Pending|Succeeded|Failed|Refunded], Last4, Amount, TransactionReference, CreatedAt)
- GiftPointTransaction(Id, UserId, Points, Reason, OrderId, CreatedAt)

FEATURES SEEN IN THE WIREFRAMES
- Catalogue: left list of categories; search box; filters Language, Format, Price Range; sort (default Relevance); sections "Recommended for You" (based on order history), "Bestsellers this Month", "New Launches". Each card: cover, title, author, short description, format, genre tags, price, "Delivery by <date>".
- Product detail: front and back cover, title, author, description, publisher, format, genre tags, price, estimated delivery date, Add to Cart, Add to Wishlist, language, average rating, copies sold, About the Writer (photo, bio, other works), reviews list, leave-a-review (100 char limit + 1-5 stars), "Related Reads" (3 books).
- Cart + checkout: line items with quantity stepper, "Use Saved Address" or enter address (first name, last name, email, address, city, pin, phone with country code, state, country), grand total (price for N items, tax, delivery charges, coupon code with Apply, discount, total), Pay Now.
- Payment: methods Credit Card, Debit Card, UPI, Wallet. Card fields: number, name on card, CVV, expiry (MM/YYYY).
- Confirmation: success message listing purchased books, "Continue your Shopping".
- Header: My Orders, My Wishlist, My Writers (followed authors), cart badge with item count, account menu.

EXTRA BUSINESS RULES FROM THE BRIEF (slide 3)
- Order history with a "Buy Again" action that re-adds a past order's items to the cart
- Gift points can be redeemed at checkout (1 point = 1 INR, cannot exceed order total, cannot exceed balance)
- An order can be cancelled only within 48 hours of PlacedAt; after that return 409 with a clear problem-details message. Cancelling refunds the payment and restores redeemed gift points and stock

CODE STANDARDS
- Layers: Controllers -> Services -> EF Core DbContext. DTOs for every request/response, never expose entities
- Constructor DI, private readonly _camelCase fields, early returns, small private helpers
- Each interface in its own I<Name>.cs file
- Input validation on every request model; global exception handling returning RFC 7807 ProblemDetails; correct HTTP status codes (201 + Location on create, 404, 409, 422/400)
- Structured logging with message templates, no string interpolation, never log card data or passwords
- Do NOT store CVV or full card numbers anywhere. Payment is a MOCK gateway behind an IPaymentGateway interface that stores only the last 4 digits
- No /// XML doc comments and no comments that restate the code
- Seed data is synthetic only (about 20 books across categories, 3 coupons, 1 test user)

First reply with a short plan and any ambiguities you see in the wireframes. Do not write code yet.
```

---

## Stage 1: OpenAPI specification (slide 12, step 3)

```
Generate an OpenAPI 3.0 specification (openapi.yaml) for this backend, covering every journey in the wireframes and the business rules above. Include:
- auth: POST /api/auth/register, POST /api/auth/login
- catalogue: GET /api/categories, GET /api/publishers, GET /api/books (search, category, publisher, language, format, minPrice, maxPrice, sort, page, pageSize), GET /api/books/{id} (with estimated delivery date, average rating, copies sold, author, related books), GET /api/books/recommended, GET /api/books/bestsellers, GET /api/books/new-launches
- reviews: GET/POST /api/books/{id}/reviews
- wishlist: GET /api/wishlist, PUT/DELETE /api/wishlist/{bookId}
- authors: GET /api/authors/{id}, followed authors under /api/me/writers
- cart: GET /api/cart, POST /api/cart/items, PUT /api/cart/items/{bookId}, DELETE /api/cart/items/{bookId}, POST /api/cart/coupon
- addresses: GET/POST /api/addresses, PUT/DELETE /api/addresses/{id}
- checkout/orders: POST /api/orders (cart -> order, applies coupon + gift points), GET /api/orders, GET /api/orders/{id}, POST /api/orders/{id}/cancel, POST /api/orders/{id}/buy-again
- payments: POST /api/orders/{id}/payments (mock gateway), GET /api/orders/{id}/payments
- gift points: GET /api/me/gift-points
Define reusable component schemas, bearer security scheme, ProblemDetails error responses, pagination envelope, and an example for each request/response. Do not generate code. Save as openapi.yaml and list any assumptions you made.
```

Your review step: check that each wireframe screen maps to at least one endpoint, and correct anything wrong. Note the correction for the video.

## Stage 2: Generate the application (slide 12, steps 4-7)

```
Using openapi.yaml as the contract, generate the ASP.NET Core 8 solution:
- src/BookWorm.Api (controllers, DTOs, services, EF Core DbContext, entities, configurations, migrations, seed data, Program.cs, appsettings.json with placeholder connection string, Swagger, JWT, ProblemDetails, /health)
- Create the initial migration with `dotnet ef migrations add InitialCreate`, no hand-written edits and no comments in the generated files
- Implement: estimated delivery date (placed date + 3 days, skipping Sunday, configurable), tax at a configurable rate, free delivery above a configurable threshold, coupon and gift-point discount calculation, the 48-hour cancellation rule, buy-again, recommendations (books in the categories the user ordered most, excluding already-purchased books, falling back to bestsellers)
- Payment: IPaymentGateway with a MockPaymentGateway that approves unless the card number ends in 0000
- Connection string from environment variable ConnectionStrings__Default
Then run `dotnet build` and fix every error and warning. Tell me the commands to apply migrations and start the API against a local PostgreSQL.
```

## Stage 3: Tests (slide 11, AI-augmented test generation)

```
Generate tests for the solution:
- Unit tests (xUnit + Moq) for the order total calculation, coupon and gift-point rules, 48-hour cancellation boundary (47h59m allowed, 48h01m rejected), buy-again, recommendations fallback, payment mock outcomes
- Integration tests with WebApplicationFactory covering register/login, browse + filter, product detail, add to cart, checkout with address, payment success and failure, order history, cancel within and after 48 hours
- Naming: Method_Scenario_Expected. Use bare // Arrange, // Act, // Assert labels and no other comments. One behaviour per test. SUT field called _sut, mocks called _thingMock
- Edge cases: empty cart checkout, unknown book id, quantity 0 or negative, expired coupon, gift points above balance, duplicate review, review over 100 chars, unauthenticated access to protected routes
Run `dotnet test` and report the results honestly, including anything failing.
```

## Stage 4: Deployability, docs and API test collection

```
Add:
- Dockerfile (multi-stage, non-root) and docker-compose.yml running the API and PostgreSQL, with the password taken from an .env file that is git-ignored, plus .env.example
- .http file (or Insomnia collection) exercising every endpoint in the order of the customer journey
- README.md: what it is, architecture, how to run locally and with Docker, how to run tests, endpoint summary, design decisions
- docs/ai-usage.md template with columns: step, prompt used, what the AI produced, what I corrected, why
- .gitignore for .NET
Do not commit anything.
```

---

## Wireframe issues to settle before you record

These came from reading the images. Decide each one, then adjust Part 0 if you disagree with my default.

1. The payment screen (image12) shows Payable Amount ₹580, but the checkout total (image11) is ₹470. The checkout figures do add up: 149 + 359 = 508, + 62 tax - 100 discount = 470. Default in the prompt: the payable amount is the order total, so treat 580 as a wireframe mistake.
2. Tax of ₹62 on ₹508 is about 12.2%, not a round rate. Default: tax rate is configurable, so seed values will not match the wireframe exactly.
3. Slide 3 mentions gift points, order history with Buy Again and cancel within 48 hours, but none of them appear in the wireframes. Default: I added them as API-only features from slide 3.
4. Slide 6 says "browse the brands" but the wireframe has no brand filter, only "Published by". Default: Publisher is the brand.
5. The address form's third field is labelled "Address" with the placeholder "Address Line 2". Default: Line1 and Line2.
6. "Hard Cover" (catalogue) and "Hardcover" (cards) differ. Default: one Format enum value, Hardcover.
7. The wireframe has no login or registration screen, although slide 3 lists login and authentication. Default: JWT register/login.
8. "My Writers" in the header is not described anywhere. Default: authors the user follows.
