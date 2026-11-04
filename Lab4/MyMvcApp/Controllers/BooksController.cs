using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers
{
    [Route("Library/Books")]
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // --- INDEX ---
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // --- CREATE (GET) ---
        [HttpGet("New")]
        public IActionResult Create()
        {
            return View();
        }

        // --- CREATE (POST) ---
        [HttpPost("New")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (!ModelState.IsValid)
            {
                return View(book); 
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- EDIT (GET) ---
        [HttpGet("{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            
            return View(book);
        }

        // --- EDIT (POST) ---
        [HttpPost("{id:int}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book updatedBook)
        {
            if (id != updatedBook.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(updatedBook); 
            }

            _context.Update(updatedBook);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // --- DELETE ---
        // Matches GET /Books/{id}/Delete
        [HttpGet("{id:int}/Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Find the book in the database asynchronously
            var book = await _context.Books.FindAsync(id);
            
            if (book != null) 
            {
                // 2. Mark it for deletion and save changes to the database
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // --- SEARCH ---
        // Matches GET /Books/Search?author=Name
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string author)
        {
            if (string.IsNullOrWhiteSpace(author))
            {
                return RedirectToAction(nameof(Index));
            }

            // EF Core translates .Contains() directly into a SQL LIKE query.
            var filteredBooks = await _context.Books
                .Where(b => b.Author.Contains(author))
                .ToListAsync();

            // Reuses the Index view to display the filtered results
            return View("Index", filteredBooks);
        }

        // --- API ---
        // Matches GET /Books/api/all
        [HttpGet("api/all")]
        public async Task<IActionResult> GetBooksApi()
        {
            // Fetch the data from the database asynchronously
            var books = await _context.Books.ToListAsync();
            
            // Return as raw JSON
            return Ok(books);
        }
    }
}
