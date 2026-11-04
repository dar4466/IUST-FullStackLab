using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers
{
    [Authorize] // Locks down all endpoints by default
    [Route("Library/Books")]
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // --- PUBLIC ENDPOINTS ---
        
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync(); //[cite: 1]
            return View(books); //[cite: 1]
        }

        [AllowAnonymous]
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string author)
        {
            if (string.IsNullOrWhiteSpace(author)) return RedirectToAction(nameof(Index)); //[cite: 1]
            
            var filteredBooks = await _context.Books
                .Where(b => b.Author.Contains(author))
                .ToListAsync(); //[cite: 1]
                
            return View("Index", filteredBooks); //[cite: 1]
        }

        // --- AUTHENTICATED ENDPOINTS (Requires Login) ---

        [HttpGet("New")]
        public IActionResult Create()
        {
            return View(); //[cite: 1]
        }

        [HttpPost("New")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (!ModelState.IsValid) return View(book); //[cite: 1]
            
            try
            {
                _context.Books.Add(book); //[cite: 1]
                await _context.SaveChangesAsync(); //[cite: 1]
                return RedirectToAction(nameof(Index)); //[cite: 1]
            }
            catch (DbUpdateException) //[cite: 1]
            {
                ModelState.AddModelError("", "Unable to save changes. A book with this title might already exist in the database."); //[cite: 1]
                return View(book); //[cite: 1]
            }
        }

        [HttpGet("{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id); //[cite: 1]
            if (book == null) return NotFound(); //[cite: 1]
            return View(book); //[cite: 1]
        }

        [HttpPost("{id:int}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book updatedBook)
        {
            if (id != updatedBook.Id) return BadRequest(); //[cite: 1]
            if (!ModelState.IsValid) return View(updatedBook); //[cite: 1]
            
            try
            {
                _context.Update(updatedBook); //[cite: 1]
                await _context.SaveChangesAsync(); //[cite: 1]
                return RedirectToAction(nameof(Index)); //[cite: 1]
            }
            catch (DbUpdateConcurrencyException) //[cite: 1]
            {
                if (!BookExists(updatedBook.Id)) return NotFound(); //[cite: 1]
                ModelState.AddModelError("", "Another user modified this record while you were editing it. Please refresh and try again."); //[cite: 1]
                return View(updatedBook); //[cite: 1]
            }
            catch (DbUpdateException) //[cite: 1]
            {
                ModelState.AddModelError("", "Unable to update. Ensure the title is unique and data is correct."); //[cite: 1]
                return View(updatedBook); //[cite: 1]
            }
        }

        // --- RESTRICTED ENDPOINTS (Requires Admin Role) ---
        
        [Authorize(Roles = "Admin")] 
        [HttpGet("{id:int}/Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id); //[cite: 1]
                if (book != null) 
                {
                    _context.Books.Remove(book); //[cite: 1]
                    await _context.SaveChangesAsync(); //[cite: 1]
                }
                return RedirectToAction(nameof(Index)); //[cite: 1]
            }
            catch (DbUpdateException) //[cite: 1]
            {
                TempData["ErrorMessage"] = "Deletion failed. Please try again later."; //[cite: 1]
                return RedirectToAction(nameof(Index)); //[cite: 1]
            }
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id); //[cite: 1]
        }
    }
}