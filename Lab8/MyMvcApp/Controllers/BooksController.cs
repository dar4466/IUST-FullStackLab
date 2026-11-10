using System.Net.Http.Json; // Required for GetFromJsonAsync
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
        private readonly IHttpClientFactory _httpClientFactory; // [NEW FOR LAB 8]

        // Inject both AppDbContext and IHttpClientFactory
        public BooksController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // --- PUBLIC ENDPOINTS ---

        [AllowAnonymous]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        [AllowAnonymous]
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string author)
        {
            if (string.IsNullOrWhiteSpace(author)) return RedirectToAction(nameof(Index));

            var filteredBooks = await _context.Books
                .Where(b => b.Author.Contains(author))
                .ToListAsync();

            return View("Index", filteredBooks);
        }

        // --- [NEW FOR LAB 8]: Enriched Book Details with External APIs ---
        // GET: /Library/Books/5/Details
        [AllowAnonymous]
        [HttpGet("{id:int}/Details")]
        public async Task<IActionResult> Details(int id)
        {
            // 1. Fetch core book record from local SQLite database
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new BookDetailsViewModel
            {
                LocalBook = book
            };

            // 2. Call External API #1: OpenLibrary API (Book Cover & Global Editions)
            try
            {
                var bookClient = _httpClientFactory.CreateClient("OpenLibraryClient");

                // URL-encode the title so spaces and special characters (e.g., "C# & .NET") don't break the URL
                string encodedTitle = Uri.EscapeDataString(book.Title);
                var openLibResponse = await bookClient.GetFromJsonAsync<OpenLibrarySearchResponse>(
                    $"search.json?title={encodedTitle}&limit=1");

                if (openLibResponse != null && openLibResponse.Docs.Any())
                {
                    viewModel.ExternalMetadata = openLibResponse.Docs.First();
                }
            }
            catch (Exception)
            {
                viewModel.ApiWarnings.Add("OpenLibrary API is currently unreachable. Live cover art and global edition counts are hidden.");
            }

            // 3. Call External API #2: Currency Exchange API (Live USD -> INR Rate)
            try
            {
                var currencyClient = _httpClientFactory.CreateClient("CurrencyClient");
                var rateResponse = await currencyClient.GetFromJsonAsync<ExchangeRateApiResponse>(
                    "v6/latest/USD");

                if (rateResponse != null && rateResponse.Rates.TryGetValue("INR", out decimal inrRate))
                {
                    viewModel.UsdToInrRate = inrRate;
                }
            }
            catch (Exception)
            {
                viewModel.ApiWarnings.Add("Currency Exchange API is currently unreachable. Showing local USD price only.");
            }

            return View(viewModel);
        }

        // --- AUTHENTICATED ENDPOINTS (Requires Login) ---

        [HttpGet("New")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("New")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (!ModelState.IsValid) return View(book);

            try
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. A book with this title might already exist in the database.");
                return View(book);
            }
        }

        [HttpGet("{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost("{id:int}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book updatedBook)
        {
            if (id != updatedBook.Id) return BadRequest();
            if (!ModelState.IsValid) return View(updatedBook);

            try
            {
                _context.Update(updatedBook);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(updatedBook.Id)) return NotFound();
                ModelState.AddModelError("", "Another user modified this record while you were editing it. Please refresh and try again.");
                return View(updatedBook);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to update. Ensure the title is unique and data is correct.");
                return View(updatedBook);
            }
        }

        // --- RESTRICTED ENDPOINTS (Requires Admin Role) ---

        [Authorize(Roles = "Admin")]
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
                TempData["ErrorMessage"] = "Deletion failed. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}