using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Unique constraints ────────────────────────────────────────────────
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // ── Seed: books ───────────────────────────────────────────────────────
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Purple Hibiscus",
                    Author = "Chimamanda Ngozi Adichie",
                    ISBN = "9781616953638",
                    CopiesAvailable = 5
                },
                new Book
                {
                    Id = 2,
                    Title = "Half of a Yellow Sun",
                    Author = "Chimamanda Ngozi Adichie",
                    ISBN = "9781400095209",
                    CopiesAvailable = 4
                },
                new Book
                {
                    Id = 3,
                    Title = "Americanah",
                    Author = "Chimamanda Ngozi Adichie",
                    ISBN = "9780307455925",
                    CopiesAvailable = 6
                },
                new Book
                {
                    Id = 4,
                    Title = "12 Rules for Life",
                    Author = "Jordan Peterson",
                    ISBN = "9780345816023",
                    CopiesAvailable = 3
                },
                new Book
                {
                    Id = 5,
                    Title = "Beyond Order",
                    Author = "Jordan Peterson",
                    ISBN = "9780593084649",
                    CopiesAvailable = 2
                },
                new Book
                {
                    Id = 6,
                    Title = "Things Fall Apart",
                    Author = "Chinua Achebe",
                    ISBN = "9780385474542",
                    CopiesAvailable = 7
                },
                new Book
                {
                    Id = 7,
                    Title = "The Famished Road",
                    Author = "Ben Okri",
                    ISBN = "9780385425148",
                    CopiesAvailable = 3
                }
            );

            // ── Seed: mock borrow records (used for "top 3 borrowed" query) ───────
            modelBuilder.Entity<BorrowRecord>().HasData(
                // Book 1 — 5 borrows
                new BorrowRecord { Id = 1, BookId = 1, BorrowedBy = "alice", BorrowedAt = new DateTime(2024, 1, 5), ReturnedAt = new DateTime(2024, 1, 15) },
                new BorrowRecord { Id = 2, BookId = 1, BorrowedBy = "bob", BorrowedAt = new DateTime(2024, 2, 3), ReturnedAt = new DateTime(2024, 2, 13) },
                new BorrowRecord { Id = 3, BookId = 1, BorrowedBy = "carol", BorrowedAt = new DateTime(2024, 3, 7), ReturnedAt = new DateTime(2024, 3, 17) },
                new BorrowRecord { Id = 4, BookId = 1, BorrowedBy = "dave", BorrowedAt = new DateTime(2024, 4, 1), ReturnedAt = new DateTime(2024, 4, 11) },
                new BorrowRecord { Id = 5, BookId = 1, BorrowedBy = "eve", BorrowedAt = new DateTime(2024, 5, 9), ReturnedAt = null },

                // Book 3 — 4 borrows
                new BorrowRecord { Id = 6, BookId = 3, BorrowedBy = "frank", BorrowedAt = new DateTime(2024, 1, 12), ReturnedAt = new DateTime(2024, 1, 22) },
                new BorrowRecord { Id = 7, BookId = 3, BorrowedBy = "grace", BorrowedAt = new DateTime(2024, 2, 19), ReturnedAt = new DateTime(2024, 2, 29) },
                new BorrowRecord { Id = 8, BookId = 3, BorrowedBy = "hank", BorrowedAt = new DateTime(2024, 3, 15), ReturnedAt = new DateTime(2024, 3, 25) },
                new BorrowRecord { Id = 9, BookId = 3, BorrowedBy = "ivan", BorrowedAt = new DateTime(2024, 4, 20), ReturnedAt = null },

                // Book 5 — 3 borrows
                new BorrowRecord { Id = 10, BookId = 5, BorrowedBy = "judy", BorrowedAt = new DateTime(2024, 1, 25), ReturnedAt = new DateTime(2024, 2, 4) },
                new BorrowRecord { Id = 11, BookId = 5, BorrowedBy = "kyle", BorrowedAt = new DateTime(2024, 3, 3), ReturnedAt = new DateTime(2024, 3, 13) },
                new BorrowRecord { Id = 12, BookId = 5, BorrowedBy = "lisa", BorrowedAt = new DateTime(2024, 4, 17), ReturnedAt = null },

                // Book 2 — 2 borrows
                new BorrowRecord { Id = 13, BookId = 2, BorrowedBy = "mike", BorrowedAt = new DateTime(2024, 2, 8), ReturnedAt = new DateTime(2024, 2, 18) },
                new BorrowRecord { Id = 14, BookId = 2, BorrowedBy = "nina", BorrowedAt = new DateTime(2024, 5, 1), ReturnedAt = null },

                // Book 4 — 1 borrow
                new BorrowRecord { Id = 15, BookId = 4, BorrowedBy = "omar", BorrowedAt = new DateTime(2024, 3, 28), ReturnedAt = new DateTime(2024, 4, 7) }
            );

            // ── Seed: users (passwords are BCrypt hashes of "Admin@123" / "User@123")
            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin"
                },
                new ApplicationUser
                {
                    Id = 2,
                    Username = "libraryuser",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                    Role = "User"
                }
            );
        }
    }
}
