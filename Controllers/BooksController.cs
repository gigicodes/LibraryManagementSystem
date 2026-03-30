using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static LibraryManagementSystem.DTOs.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] //authorization required for all endpoints                     
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDbContext _db;
        private readonly IExternalBookService _externalBook;

        public BooksController(LibraryDbContext db, IExternalBookService externalBook)
        {
            _db = db;
            _externalBook = externalBook;
        }

        //  READ — available to both Admin and User

        // Get all the books
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var books = await _db.Books
                .AsNoTracking()
                .OrderBy(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => ToResponse(b))
                .ToListAsync();

            var total = await _db.Books.CountAsync();

            Response.Headers.Append("X-Total-Count", total.ToString());
            Response.Headers.Append("X-Page", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());

            return Ok(books);
        }

        /// Get a single book by ID
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            return book is null ? NotFound(new { message = $"Book {id} not found." }) : Ok(ToResponse(book));
        }

        //  LINQ QUERY 1 — Books grouped by Author
        
        [HttpGet("grouped-by-author")]
        [ProducesResponseType(typeof(IEnumerable<AuthorGroup>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroupedByAuthor()
        {
            // Pull all books into memory first (SQLite can't translate GroupBy directly)
            var books = await _db.Books.AsNoTracking().OrderBy(b => b.Author).ToListAsync();

            var grouped = books
                .GroupBy(b => b.Author)
                .Select(g => new AuthorGroup(
                    Author: g.Key,
                    Books: g.Select(b => ToResponse(b)).ToList()
                ))
                .OrderBy(g => g.Author)
                .ToList();

            return Ok(grouped);
        }

        //  LINQ QUERY 2 — Top 3 most borrowed books

        [HttpGet("top-borrowed")]
        [ProducesResponseType(typeof(IEnumerable<PopularBook>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopBorrowed()
        {
            var top3 = await _db.BorrowRecords
                .AsNoTracking()
                .GroupBy(br => br.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    TotalBorrows = g.Count()
                })
                .OrderByDescending(x => x.TotalBorrows)
                .Take(3)
                .Join(_db.Books,
                    br => br.BookId,
                    book => book.Id,
                    (br, book) => new PopularBook(book.Id, book.Title, book.Author, br.TotalBorrows))
                .ToListAsync();

            return Ok(top3);
        }

        //  EXTERNAL API — async/await

        [HttpGet("external/{isbn}")]
        [ProducesResponseType(typeof(ExternalBookDetail), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExternalDetails(string isbn, CancellationToken ct)
        {
            var detail = await _externalBook.FetchBookDetailAsync(isbn, ct);
            return Ok(detail);
        }

        //  WRITE — Admin only

        //Add a new book
        [HttpPost] 
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] BookRequest request)
        {
            if (await _db.Books.AnyAsync(b => b.ISBN == request.ISBN))
                return Conflict(new { message = $"A book with ISBN '{request.ISBN}' already exists." });

            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                ISBN = request.ISBN,
                CopiesAvailable = request.CopiesAvailable
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, ToResponse(book));
        }

        // Update an existing book
        [HttpPut("{id:int}")] 
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] BookRequest request)
        {
            var book = await _db.Books.FindAsync(id);
            if (book is null)
                return NotFound(new { message = $"Book {id} not found." });

            // Ensure the ISBN is not stolen by another book
            if (await _db.Books.AnyAsync(b => b.ISBN == request.ISBN && b.Id != id))
                return Conflict(new { message = $"ISBN '{request.ISBN}' is already used by another book." });

            book.Title = request.Title;
            book.Author = request.Author;
            book.ISBN = request.ISBN;
            book.CopiesAvailable = request.CopiesAvailable;

            await _db.SaveChangesAsync();
            return Ok(ToResponse(book));
        }

        /// Delete a book
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book is null)
                return NotFound(new { message = $"Book {id} not found." });

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── Helper ───────────────────────────────────────────────────────────────
        private static BookResponse ToResponse(Book b) =>
            new(b.Id, b.Title, b.Author, b.ISBN, b.CopiesAvailable);
    }
}
