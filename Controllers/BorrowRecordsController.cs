using System.Security.Claims;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers;

        // ── DTOs (borrow-specific, kept close to the controller) ────────────────────

        /// <summary>Payload to borrow a book.</summary>
        public record BorrowRequest(int BookId);

        /// <summary>Response returned after a borrow or return action.</summary>
        public record BorrowRecordResponse(
            int Id,
            int BookId,
            string BookTitle,
            string BorrowedBy,
            DateTime BorrowedAt,
            DateTime? ReturnedAt,
            bool IsOnLoan
        );

        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        [Produces("application/json")]
        public class BorrowRecordsController : ControllerBase
        {
            private readonly LibraryDbContext _db;

            public BorrowRecordsController(LibraryDbContext db) => _db = db;

            // ── GET /api/borrowrecords  (Admin only — full list)
            [HttpGet]
            [Authorize(Roles = "Admin")]
            [ProducesResponseType(typeof(IEnumerable<BorrowRecordResponse>), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetAll([FromQuery] bool onLoanOnly = false)
            {
                var query = _db.BorrowRecords
                    .AsNoTracking()
                    .Include(br => br.Book);

                if (onLoanOnly)
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<BorrowRecord, Book>)
                            query.Where(br => br.ReturnedAt == null);

                var records = await query
                    .OrderBy(br => br.Id)
                    .Select(br => Map(br))
                    .ToListAsync();

                return Ok(records);
            }

            // ── GET /api/borrowrecords/my  (current user's own records) ───────────────

            /// <summary>List the current user's own borrow history.</summary>
            [HttpGet("my")]
            [ProducesResponseType(typeof(IEnumerable<BorrowRecordResponse>), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetMyRecords()
            {
                var username = User.FindFirstValue(ClaimTypes.Name)!;

                var records = await _db.BorrowRecords
                    .AsNoTracking()
                    .Include(br => br.Book)
                    .Where(br => br.BorrowedBy == username)
                    .OrderBy(br => br.Id)
                    .Select(br => Map(br))
                    .ToListAsync();

                return Ok(records);
            }

            // ── POST /api/borrowrecords/borrow  ───────────────────────────────────────

            [HttpPost("borrow")]
            [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status201Created)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public async Task<IActionResult> Borrow([FromBody] BorrowRequest request)
            {
                var book = await _db.Books.FindAsync(request.BookId);
                if (book is null)
                    return NotFound(new { message = $"Book {request.BookId} not found." });

                if (book.CopiesAvailable <= 0)
                    return BadRequest(new { message = $"No copies of '{book.Title}' are currently available." });

                var username = User.FindFirstValue(ClaimTypes.Name)!;

                // Check: user has not already borrowed this book without returning it
                var alreadyBorrowed = await _db.BorrowRecords.AnyAsync(br =>
                    br.BookId == request.BookId &&
                    br.BorrowedBy == username &&
                    br.ReturnedAt == null);

                if (alreadyBorrowed)
                    return BadRequest(new { message = "You already have this book on loan. Please return it first." });

                // Create borrow record and decrement copies atomically
                var record = new BorrowRecord
                {
                    BookId = request.BookId,
                    BorrowedBy = username,
                    BorrowedAt = DateTime.UtcNow
                };

                book.CopiesAvailable--;
                _db.BorrowRecords.Add(record);
                await _db.SaveChangesAsync();

                // Reload to get navigation property
                await _db.Entry(record).Reference(r => r.Book).LoadAsync();

                return CreatedAtAction(nameof(GetAll), new { }, Map(record));
            }

            // ── POST /api/borrowrecords/return/{id}  ──────────────────────────────────

            [HttpPost("return/{id:int}")]
            [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status403Forbidden)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public async Task<IActionResult> Return(int id)
            {
                var record = await _db.BorrowRecords
                    .Include(br => br.Book)
                    .FirstOrDefaultAsync(br => br.Id == id);

                if (record is null)
                    return NotFound(new { message = $"Borrow record {id} not found." });

                if (record.ReturnedAt is not null)
                    return BadRequest(new { message = "This book has already been returned." });

                var username = User.FindFirstValue(ClaimTypes.Name)!;
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && record.BorrowedBy != username)
                    return Forbid();   // can't return someone else's loan

                record.ReturnedAt = DateTime.UtcNow;
                record.Book.CopiesAvailable++;

                await _db.SaveChangesAsync();
                return Ok(Map(record));
            }

            // ── Helper ────────────────────────────────────────────────────────────────

            private static BorrowRecordResponse Map(BorrowRecord br) =>
                new(br.Id, br.BookId, br.Book.Title, br.BorrowedBy,
                    br.BorrowedAt, br.ReturnedAt, br.ReturnedAt is null);
        }

