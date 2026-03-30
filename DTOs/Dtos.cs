using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DTOs
{
    public class Dtos
    {
        // ─── Auth ────────────────────────────────────────────────────────────────────

        /// <summary>Payload required to log in.</summary>
        public record LoginRequest(
            [Required] string Username,
            [Required] string Password
        );

        /// <summary>Payload required to register a new user (Admin only).</summary>
        public record RegisterRequest(
            [Required] string Username,
            [Required][MinLength(6)] string Password,
            [Required] string Role       // "Admin" or "User"
        );

        /// <summary>Returned after a successful login.</summary>
        public record AuthResponse(
            string Token,
            string Username,
            string Role,
            DateTime ExpiresAt
        );

        // ─── Books ───────────────────────────────────────────────────────────────────

        /// <summary>Payload for creating or updating a book.</summary>
        public record BookRequest(
            [Required][MaxLength(200)] string Title,
            [Required][MaxLength(100)] string Author,
            [Required][MaxLength(20)] string ISBN,
            [Range(0, 10_000)] int CopiesAvailable
        );

        /// <summary>Book details returned to callers.</summary>
        public record BookResponse(
            int Id,
            string Title,
            string Author,
            string ISBN,
            int CopiesAvailable
        );

        // ─── Analytics ───────────────────────────────────────────────────────────────

        /// <summary>Books grouped by a single author.</summary>
        public record AuthorGroup(
            string Author,
            IEnumerable<BookResponse> Books
        );

        /// <summary>A book together with its total borrow count.</summary>
        public record PopularBook(
            int BookId,
            string Title,
            string Author,
            int TotalBorrows
        );

        // ─── External API ─────────────────────────────────────────────────────────────

        /// <summary>Mock payload representing data fetched from an external book API.</summary>
        public record ExternalBookDetail(
            string Title,
            string Author,
            string Publisher,
            string PublishedDate,
            string Description,
            string ThumbnailUrl,
            string Source          
    }
}
