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

            try
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException) // Catches Database-Level rule violations (like duplicate Titles)
            {
                // Add a custom error message to the ModelState to show on the UI
                ModelState.AddModelError("", "Unable to save changes. A book with this title might already exist in the database.");
                return View(book);
            }
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

            try
            {
                _context.Update(updatedBook);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException) // Catches simultaneous edit conflicts
            {
                if (!BookExists(updatedBook.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "Another user modified this record while you were editing it. Please refresh and try again.");
                    return View(updatedBook);
                }
            }
            catch (DbUpdateException) // Catches constraint violations
            {
                ModelState.AddModelError("", "Unable to update. Ensure the title is unique and data is correct.");
                return View(updatedBook);
            }
        }

        // --- DELETE ---
        [HttpGet("{id:int}/Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book != null) 
                {
                    _context.Books.Remove(book);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // In case the DB is locked or fails during deletion
                TempData["ErrorMessage"] = "Deletion failed. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // --- SEARCH ---
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string author)
        {
            if (string.IsNullOrWhiteSpace(author))
            {
                return RedirectToAction(nameof(Index));
            }

            var filteredBooks = await _context.Books
                .Where(b => b.Author.Contains(author))
                .ToListAsync();

            return View("Index", filteredBooks);
        }

        // --- API ---
        [HttpGet("api/all")]
        public async Task<IActionResult> GetBooksApi()
        {
            var books = await _context.Books.ToListAsync();
            return Ok(books);
        }

        // Helper Method
        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
