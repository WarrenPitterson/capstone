using BookWorm.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(BookWormDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Romance" },
            new() { Name = "Mystery" },
            new() { Name = "Science Fiction" },
            new() { Name = "Fantasy" },
            new() { Name = "Historical" },
            new() { Name = "Biography" },
            new() { Name = "Self-help" },
            new() { Name = "Memoir" },
            new() { Name = "Travel" },
            new() { Name = "Cooking" },
            new() { Name = "Children's" },
            new() { Name = "Young Adult" },
            new() { Name = "Comics & Graphic Novels" },
            new() { Name = "Poetry" },
            new() { Name = "Drama" },
            new() { Name = "Science" },
            new() { Name = "Philosophy" },
            new() { Name = "Religion" },
            new() { Name = "Language Learning" }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var genres = new List<Genre>
        {
            new() { Name = "Non-fiction" },
            new() { Name = "Fiction" },
            new() { Name = "Thriller" },
            new() { Name = "Horror" },
            new() { Name = "Love" },
            new() { Name = "Children" },
            new() { Name = "Self Help" },
            new() { Name = "Drama" }
        };
        await context.Genres.AddRangeAsync(genres);
        await context.SaveChangesAsync();

        var publishers = new List<Publisher>
        {
            new() { Name = "ABC Publishers" },
            new() { Name = "HarperCollins" },
            new() { Name = "Penguin Books" },
            new() { Name = "Vintage Classics" }
        };
        await context.Publishers.AddRangeAsync(publishers);
        await context.SaveChangesAsync();

        var authors = new List<Author>
        {
            new()
            {
                Name = "Daniel Reed",
                Bio = "Daniel Reed is a writer, minimalist, and productivity coach based in San Francisco. With a passion for intentional living, Daniel has dedicated his career to helping individuals simplify their lives — one habit, one space, and one thought at a time.",
                PhotoUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=400",
                OtherWorks = "Less, But Better and The Focus Reset"
            },
            new()
            {
                Name = "Arjun Patel",
                Bio = "Arjun Patel is a seasoned entrepreneur and mindfulness advocate specializing in deep work principles.",
                PhotoUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400",
                OtherWorks = "Deep Focus in a Distracted World"
            },
            new()
            {
                Name = "Raj Patel",
                Bio = "Raj Patel is an educator and cognitive psychologist focused on rapid skill acquisition and lifelong learning.",
                PhotoUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=400",
                OtherWorks = "The Accelerated Mind"
            },
            new()
            {
                Name = "James Wright",
                Bio = "James Wright is a motivational speaker and executive leadership consultant.",
                PhotoUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=400",
                OtherWorks = "Habits of High Achievers"
            },
            new()
            {
                Name = "James Adams",
                Bio = "James Adams writes atmospheric thrillers and gothic mysteries set in rural Britain.",
                PhotoUrl = "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=400",
                OtherWorks = "Echoes in the Fog"
            },
            new()
            {
                Name = "Jessica Martin",
                Bio = "Jessica Martin is a bestselling author of emotional contemporary romance and drama.",
                PhotoUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=400",
                OtherWorks = "Whispers at Dawn"
            },
            new()
            {
                Name = "Laura Mitchell",
                Bio = "Laura Mitchell is an astrophysicist and hard science fiction novelist.",
                PhotoUrl = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=400",
                OtherWorks = "Orbiting Infinity"
            },
            new()
            {
                Name = "Clara Nelson",
                Bio = "Clara Nelson crafts psychological horror and supernatural suspense fiction.",
                PhotoUrl = "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=400",
                OtherWorks = "Shadows in the Wood"
            },
            new()
            {
                Name = "Emily Parker",
                Bio = "Emily Parker writes heartwarming children's tales and illustrated animal adventures.",
                PhotoUrl = "https://images.unsplash.com/photo-1567532939604-b6b5b0db2604?w=400",
                OtherWorks = "The Brave Little Puppy"
            },
            new()
            {
                Name = "Michael Brown",
                Bio = "Michael Brown is a culinary historian and award-winning chef.",
                PhotoUrl = "https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?w=400",
                OtherWorks = "Flavors of the Spice Route"
            }
        };
        await context.Authors.AddRangeAsync(authors);
        await context.SaveChangesAsync();

        var catSelfHelp = categories.First(c => c.Name == "Self-help");
        var catMystery = categories.First(c => c.Name == "Mystery");
        var catRomance = categories.First(c => c.Name == "Romance");
        var catSciFi = categories.First(c => c.Name == "Science Fiction");
        var catChildren = categories.First(c => c.Name == "Children's");
        var catCooking = categories.First(c => c.Name == "Cooking");
        var catHistorical = categories.First(c => c.Name == "Historical");
        var catPhilosophy = categories.First(c => c.Name == "Philosophy");

        var pubAbc = publishers.First(p => p.Name == "ABC Publishers");
        var pubHarper = publishers.First(p => p.Name == "HarperCollins");
        var pubPenguin = publishers.First(p => p.Name == "Penguin Books");

        var authDaniel = authors.First(a => a.Name == "Daniel Reed");
        var authArjun = authors.First(a => a.Name == "Arjun Patel");
        var authRaj = authors.First(a => a.Name == "Raj Patel");
        var authWright = authors.First(a => a.Name == "James Wright");
        var authAdams = authors.First(a => a.Name == "James Adams");
        var authMartin = authors.First(a => a.Name == "Jessica Martin");
        var authMitchell = authors.First(a => a.Name == "Laura Mitchell");
        var authNelson = authors.First(a => a.Name == "Clara Nelson");
        var authParker = authors.First(a => a.Name == "Emily Parker");
        var authBrown = authors.First(a => a.Name == "Michael Brown");

        var books = new List<Book>
        {
            new()
            {
                Title = "Joy of Minimalism",
                Description = "Declutter your life to uncover peace, clarity, and joy. Discover how less can truly be more through practical daily habits.",
                AuthorId = authDaniel.Id,
                PublisherId = pubAbc.Id,
                CategoryId = catSelfHelp.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 149.00m,
                Isbn = "978-0-123456-78-9",
                FrontCoverUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400",
                CopiesSold = 145,
                StockQuantity = 50,
                IsNewLaunch = true
            },
            new()
            {
                Title = "The Art of Focus",
                Description = "Practical guide to mastering focus and boosting productivity every day in a distraction-filled world.",
                AuthorId = authArjun.Id,
                PublisherId = pubHarper.Id,
                CategoryId = catSelfHelp.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 399.00m,
                Isbn = "978-0-987654-32-1",
                FrontCoverUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1516979187457-637abb4f9353?w=400",
                CopiesSold = 220,
                StockQuantity = 40,
                IsNewLaunch = false
            },
            new()
            {
                Title = "The Art of Learning",
                Description = "Master the mindset and methods for effective lifelong learning and cognitive performance.",
                AuthorId = authRaj.Id,
                PublisherId = pubPenguin.Id,
                CategoryId = catSelfHelp.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 259.00m,
                Isbn = "978-1-234567-89-0",
                FrontCoverUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?w=400",
                CopiesSold = 180,
                StockQuantity = 35,
                IsNewLaunch = false
            },
            new()
            {
                Title = "The Path to Success",
                Description = "A practical guide to achieving goals with clarity, resilience, and unshakeable confidence.",
                AuthorId = authWright.Id,
                PublisherId = pubAbc.Id,
                CategoryId = catSelfHelp.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 359.00m,
                Isbn = "978-0-112233-44-5",
                FrontCoverUrl = "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400",
                CopiesSold = 310,
                StockQuantity = 60,
                IsNewLaunch = false
            },
            new()
            {
                Title = "The Midnight Hour",
                Description = "Haunting tale of a man's journey into the shadows of a forgotten past and ancient village secrets.",
                AuthorId = authAdams.Id,
                PublisherId = pubHarper.Id,
                CategoryId = catMystery.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 299.00m,
                Isbn = "978-0-223344-55-6",
                FrontCoverUrl = "https://images.unsplash.com/photo-1509281373149-e957c6296406?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=400",
                CopiesSold = 450,
                StockQuantity = 25,
                IsNewLaunch = false
            },
            new()
            {
                Title = "Beneath the Stars",
                Description = "A heartwarming tale where two souls discover what they truly need beneath the night sky.",
                AuthorId = authMartin.Id,
                PublisherId = pubPenguin.Id,
                CategoryId = catRomance.Id,
                Format = BookFormat.Hardcover,
                Language = "English",
                Price = 499.00m,
                Isbn = "978-0-334455-66-7",
                FrontCoverUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1509281373149-e957c6296406?w=400",
                CopiesSold = 520,
                StockQuantity = 30,
                IsNewLaunch = false
            },
            new()
            {
                Title = "The Final Frontier",
                Description = "A daring mission into space to uncover cosmic secrets that will change humanity forever.",
                AuthorId = authMitchell.Id,
                PublisherId = pubHarper.Id,
                CategoryId = catSciFi.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 359.00m,
                Isbn = "978-0-445566-77-8",
                FrontCoverUrl = "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1506703719100-a0f3a48c0f86?w=400",
                CopiesSold = 380,
                StockQuantity = 45,
                IsNewLaunch = false
            },
            new()
            {
                Title = "The Vanishing House",
                Description = "A chilling mystery unfolds within a grand Victorian house that appears and disappears at will.",
                AuthorId = authNelson.Id,
                PublisherId = pubPenguin.Id,
                CategoryId = catMystery.Id,
                Format = BookFormat.EBook,
                Language = "English",
                Price = 99.00m,
                Isbn = "978-0-556677-88-9",
                FrontCoverUrl = "https://images.unsplash.com/photo-1516979187457-637abb4f9353?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400",
                CopiesSold = 195,
                StockQuantity = 100,
                IsNewLaunch = true
            },
            new()
            {
                Title = "The Lost Kitten",
                Description = "A heartwarming tale of courage, friendship, and feline adventure across a vibrant city.",
                AuthorId = authParker.Id,
                PublisherId = pubAbc.Id,
                CategoryId = catChildren.Id,
                Format = BookFormat.Hardcover,
                Language = "English",
                Price = 339.00m,
                Isbn = "978-0-667788-99-0",
                FrontCoverUrl = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1533738363-b7f9aef128ce?w=400",
                CopiesSold = 275,
                StockQuantity = 50,
                IsNewLaunch = true
            },
            new()
            {
                Title = "Flavors of the Spice Route",
                Description = "An aromatic culinary voyage exploring ancient spice blends and timeless traditional recipes.",
                AuthorId = authBrown.Id,
                PublisherId = pubAbc.Id,
                CategoryId = catCooking.Id,
                Format = BookFormat.Hardcover,
                Language = "English",
                Price = 599.00m,
                Isbn = "978-0-778899-00-1",
                FrontCoverUrl = "https://images.unsplash.com/photo-1495147466023-ac5c588e2e94?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400",
                CopiesSold = 160,
                StockQuantity = 20,
                IsNewLaunch = false
            },
            new()
            {
                Title = "Mindful Mornings",
                Description = "Transform your life with morning rituals designed to cultivate presence, energy, and mindfulness.",
                AuthorId = authDaniel.Id,
                PublisherId = pubAbc.Id,
                CategoryId = catSelfHelp.Id,
                Format = BookFormat.EBook,
                Language = "English",
                Price = 129.00m,
                Isbn = "978-0-889900-11-2",
                FrontCoverUrl = "https://images.unsplash.com/photo-1506126613408-eca07ce68773?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400",
                CopiesSold = 130,
                StockQuantity = 100,
                IsNewLaunch = true
            },
            new()
            {
                Title = "Echoes of Rome",
                Description = "An immersive historical chronicle tracing the rise, glory, and fall of ancient Rome's heroes.",
                AuthorId = authWright.Id,
                PublisherId = pubHarper.Id,
                CategoryId = catHistorical.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 420.00m,
                Isbn = "978-0-990011-22-3",
                FrontCoverUrl = "https://images.unsplash.com/photo-1552832230-c0197dd311b5?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1529156069898-49953e39b3ac?w=400",
                CopiesSold = 210,
                StockQuantity = 30,
                IsNewLaunch = false
            },
            new()
            {
                Title = "The Stoic Journey",
                Description = "Timeless philosophy for modern challenges, exploring clarity, fortitude, and inner peace.",
                AuthorId = authRaj.Id,
                PublisherId = pubPenguin.Id,
                CategoryId = catPhilosophy.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 280.00m,
                Isbn = "978-1-001122-33-4",
                FrontCoverUrl = "https://images.unsplash.com/photo-1499209974431-9dddcece7f88?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1506126613408-eca07ce68773?w=400",
                CopiesSold = 340,
                StockQuantity = 40,
                IsNewLaunch = false
            },
            new()
            {
                Title = "Shadows in the Wood",
                Description = "A spine-tingling tale of ancient folklore awakening in an isolated mountain village.",
                AuthorId = authNelson.Id,
                PublisherId = pubHarper.Id,
                CategoryId = catMystery.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 249.00m,
                Isbn = "978-1-112233-44-5",
                FrontCoverUrl = "https://images.unsplash.com/photo-1448375240586-882707db888b?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=400",
                CopiesSold = 185,
                StockQuantity = 25,
                IsNewLaunch = true
            },
            new()
            {
                Title = "Whispers at Dawn",
                Description = "A poetic romance exploring forgiveness, second chances, and the healing power of love.",
                AuthorId = authMartin.Id,
                PublisherId = pubPenguin.Id,
                CategoryId = catRomance.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 310.00m,
                Isbn = "978-1-223344-55-6",
                FrontCoverUrl = "https://images.unsplash.com/photo-1518895949257-7621c3c786d7?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=400",
                CopiesSold = 410,
                StockQuantity = 35,
                IsNewLaunch = false
            },
            new()
            {
                Title = "Starlight Odyssey",
                Description = "An epic interstellar exploration across uncharted galaxies and alien civilizations.",
                AuthorId = authMitchell.Id,
                PublisherId = pubHarper.Id,
                CategoryId = catSciFi.Id,
                Format = BookFormat.Hardcover,
                Language = "English",
                Price = 480.00m,
                Isbn = "978-1-334455-66-7",
                FrontCoverUrl = "https://images.unsplash.com/photo-1446776811953-b23d57bd21aa?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=400",
                CopiesSold = 290,
                StockQuantity = 20,
                IsNewLaunch = false
            },
            new()
            {
                Title = "The Secret Meadow",
                Description = "A delightful illustrated story about woodland creatures discovering friendship and unity.",
                AuthorId = authParker.Id,
                PublisherId = pubAbc.Id,
                CategoryId = catChildren.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 199.00m,
                Isbn = "978-1-445566-77-8",
                FrontCoverUrl = "https://images.unsplash.com/photo-1502082553048-f009c37129b9?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=400",
                CopiesSold = 150,
                StockQuantity = 45,
                IsNewLaunch = true
            },
            new()
            {
                Title = "The Focused Executive",
                Description = "Strategic habits and time-blocking principles for modern leadership excellence.",
                AuthorId = authArjun.Id,
                PublisherId = pubHarper.Id,
                CategoryId = catSelfHelp.Id,
                Format = BookFormat.Hardcover,
                Language = "English",
                Price = 450.00m,
                Isbn = "978-1-556677-88-9",
                FrontCoverUrl = "https://images.unsplash.com/photo-1507679799987-c73779587ccf?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400",
                CopiesSold = 230,
                StockQuantity = 30,
                IsNewLaunch = false
            },
            new()
            {
                Title = "The Accelerated Mind",
                Description = "Unlocking neuroplasticity and memory techniques for rapid comprehension and retention.",
                AuthorId = authRaj.Id,
                PublisherId = pubPenguin.Id,
                CategoryId = catSelfHelp.Id,
                Format = BookFormat.EBook,
                Language = "English",
                Price = 150.00m,
                Isbn = "978-1-667788-99-0",
                FrontCoverUrl = "https://images.unsplash.com/photo-1457369804613-52c61a468e7d?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?w=400",
                CopiesSold = 310,
                StockQuantity = 100,
                IsNewLaunch = false
            },
            new()
            {
                Title = "Simplicity in Practice",
                Description = "A 30-day workbook to declutter physical spaces, mental chatter, and digital overload.",
                AuthorId = authDaniel.Id,
                PublisherId = pubAbc.Id,
                CategoryId = catSelfHelp.Id,
                Format = BookFormat.Paperback,
                Language = "English",
                Price = 189.00m,
                Isbn = "978-1-778899-00-1",
                FrontCoverUrl = "https://images.unsplash.com/photo-1499750310107-5fef28a66643?w=400",
                BackCoverUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400",
                CopiesSold = 140,
                StockQuantity = 40,
                IsNewLaunch = true
            }
        };

        await context.Books.AddRangeAsync(books);
        await context.SaveChangesAsync();

        var gNonFiction = genres.First(g => g.Name == "Non-fiction");
        var gFiction = genres.First(g => g.Name == "Fiction");
        var gSelfHelp = genres.First(g => g.Name == "Self Help");
        var gThriller = genres.First(g => g.Name == "Thriller");
        var gHorror = genres.First(g => g.Name == "Horror");
        var gLove = genres.First(g => g.Name == "Love");
        var gDrama = genres.First(g => g.Name == "Drama");
        var gChildren = genres.First(g => g.Name == "Children");

        var bookGenres = new List<BookGenre>
        {
            new() { BookId = books[0].Id, GenreId = gNonFiction.Id },
            new() { BookId = books[0].Id, GenreId = gSelfHelp.Id },
            new() { BookId = books[1].Id, GenreId = gNonFiction.Id },
            new() { BookId = books[1].Id, GenreId = gSelfHelp.Id },
            new() { BookId = books[2].Id, GenreId = gNonFiction.Id },
            new() { BookId = books[2].Id, GenreId = gSelfHelp.Id },
            new() { BookId = books[3].Id, GenreId = gNonFiction.Id },
            new() { BookId = books[3].Id, GenreId = gSelfHelp.Id },
            new() { BookId = books[4].Id, GenreId = gFiction.Id },
            new() { BookId = books[4].Id, GenreId = gThriller.Id },
            new() { BookId = books[4].Id, GenreId = gHorror.Id },
            new() { BookId = books[5].Id, GenreId = gFiction.Id },
            new() { BookId = books[5].Id, GenreId = gLove.Id },
            new() { BookId = books[5].Id, GenreId = gDrama.Id },
            new() { BookId = books[6].Id, GenreId = gFiction.Id },
            new() { BookId = books[6].Id, GenreId = gThriller.Id },
            new() { BookId = books[7].Id, GenreId = gFiction.Id },
            new() { BookId = books[7].Id, GenreId = gHorror.Id },
            new() { BookId = books[8].Id, GenreId = gFiction.Id },
            new() { BookId = books[8].Id, GenreId = gChildren.Id }
        };
        await context.BookGenres.AddRangeAsync(bookGenres);

        // Related Reads for Joy of Minimalism (books[0]) -> books[1], books[2], books[3]
        var relatedBooks = new List<RelatedBook>
        {
            new() { BookId = books[0].Id, RelatedBookId = books[1].Id },
            new() { BookId = books[0].Id, RelatedBookId = books[2].Id },
            new() { BookId = books[0].Id, RelatedBookId = books[3].Id }
        };
        await context.RelatedBooks.AddRangeAsync(relatedBooks);

        // Coupons
        var coupons = new List<Coupon>
        {
            new() { Code = "SAVE100", DiscountAmount = 100.00m, ExpiresAt = DateTime.UtcNow.AddYears(1), IsActive = true },
            new() { Code = "WELCOME50", DiscountAmount = 50.00m, ExpiresAt = DateTime.UtcNow.AddYears(1), IsActive = true },
            new() { Code = "BOOKWORM200", DiscountAmount = 200.00m, ExpiresAt = DateTime.UtcNow.AddYears(1), IsActive = true }
        };
        await context.Coupons.AddRangeAsync(coupons);

        // Test User
        var testUser = new User
        {
            Email = "testuser@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FirstName = "John",
            LastName = "Smith",
            Phone = "9876543210",
            GiftPointsBalance = 100,
            CreatedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(testUser);
        await context.SaveChangesAsync();

        // Sample Review from John Smith for Joy of Minimalism
        var review = new Review
        {
            BookId = books[0].Id,
            UserId = testUser.Id,
            Rating = 5,
            Text = "The accordion component delivers large amounts of content in a small space through progressive disclosure.",
            CreatedAt = DateTime.UtcNow
        };
        await context.Reviews.AddAsync(review);

        // Sample Address for test user
        var address = new Address
        {
            UserId = testUser.Id,
            FirstName = "John",
            LastName = "Smith",
            Email = "testuser@example.com",
            Line1 = "Flat 402, Green Heights",
            Line2 = "MG Road",
            City = "Mumbai",
            Pin = "400001",
            State = "Maharashtra",
            Country = "India",
            PhoneCountryCode = "+91",
            Phone = "9876543210",
            IsSaved = true
        };
        await context.Addresses.AddAsync(address);
        await context.SaveChangesAsync();
    }
}
