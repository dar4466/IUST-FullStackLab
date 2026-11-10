using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers
{
    [ApiController]          // Enables automatic ModelState validation & JSON body binding
    [Route("api/books")]     // Base resource URI: /api/books
    public class BooksApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksApiController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET ALL BOOKS OR FILTER BY AUTHOR
        // Endpoint: GET /api/books OR GET /api/books?author=Martin
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] string? author)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(b => b.Author.Contains(author));
            }

            var books = await query.ToListAsync();
            return Ok(books); // Returns HTTP 200 OK + JSON array
        }

        // 2. GET A SINGLE BOOK BY ID
        // Endpoint: GET /api/books/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound(new { message = $"Book with ID {id} was not found." }); // HTTP 404
            }

            return Ok(book); // Returns HTTP 200 OK + JSON object
        }

        // 3. CREATE A NEW BOOK
        // Endpoint: POST /api/books
        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook(Book book)
        {
            // Note: [ApiController] automatically returns 400 Bad Request if Book validation fails
            try
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                // Returns HTTP 201 Created + Location Header (/api/books/{id}) + created JSON object
                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
            }
            catch (DbUpdateException)
            {
                // Triggered when the unique Title constraint in AppDbContext is violated
                return Conflict(new { message = "Unable to save. A book with this title already exists." }); // HTTP 409
            }
        }

        // 4. UPDATE AN EXISTING BOOK
        // Endpoint: PUT /api/books/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBook(int id, Book updatedBook)
        {
            if (id != updatedBook.Id)
            {
                return BadRequest(new { message = "URL route ID does not match the Book ID in the JSON body." }); // HTTP 400
            }

            try
            {
                _context.Entry(updatedBook).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent(); // Returns HTTP 204 No Content (lightweight confirmation)
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BookExistsAsync(id))
                {
                    return NotFound(new { message = $"Book with ID {id} was not found." }); // HTTP 404
                }

                return Conflict(new { message = "Another user modified this record. Please refresh and try again." }); // HTTP 409
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Unable to update. Ensure the book title is unique." }); // HTTP 409
            }
        }

        // 5. DELETE A BOOK
        // Endpoint: DELETE /api/books/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { message = $"Book with ID {id} was not found." }); // HTTP 404
            }

            try
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return NoContent(); // Returns HTTP 204 No Content
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Deletion failed due to a database error." }); // HTTP 500
            }
        }

        private async Task<bool> BookExistsAsync(int id)
        {
            return await _context.Books.AnyAsync(e => e.Id == id);
        }
    }
}