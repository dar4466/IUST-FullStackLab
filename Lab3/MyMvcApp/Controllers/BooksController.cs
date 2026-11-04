using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace MyMvcApp.Controllers
{
    // Establishes a custom base URL for every action in this controller
    [Route("Library/Books")]
    public class BooksController : Controller
    {
        private static List<Book> _books = new List<Book>();
        private static int _nextId = 1;

        // READ: Matches exactly GET /Library/Books
        [HttpGet("")]
        public IActionResult Index() => View(_books);

        // CREATE: Matches GET /Library/Books/New
        [HttpGet("New")]
        public IActionResult Create() => View();

        // CREATE: Matches POST /Library/Books/New
        // UPDATE ONLY the POST Create method
[HttpPost("New")]
public IActionResult Create(Book book)
{
// Error Handling: Check if the submitted data passes our Data Annotations
if (!ModelState.IsValid)
{
// Return the view immediately, passing back theinvalid book object
// so the user doesn't lose what they already typed.
return View(book);
}
book.Id = _nextId++;
_books.Add(book);
return RedirectToAction("Index");
}

        // UPDATE: Matches GET /Library/Books/5/Edit
        // The ":int" constraint ensures this only fires if the ID is a number
        [HttpGet("{id:int}/Edit")]
        public IActionResult Edit(int id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }

        // UPDATE: Matches POST /Library/Books/5/Edit
        // UPDATE ONLY the POST Edit method
[HttpPost("{id:int}/Edit")]
public IActionResult Edit(int id, Book updatedBook)
{
if (id != updatedBook.Id) return BadRequest();
// Error Handling: Validate before updating
if (!ModelState.IsValid)
{
return View(updatedBook);
}
var book = _books.FirstOrDefault(b => b.Id ==
updatedBook.Id);
if (book != null)
{
book.Title = updatedBook.Title;
book.Author = updatedBook.Author;
}
return RedirectToAction("Index");
}

        // DELETE: Matches GET /Library/Books/5/Delete
        [HttpGet("{id:int}/Delete")]
        public IActionResult Delete(int id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book != null) _books.Remove(book);
            return RedirectToAction("Index");
        }

        // SEARCH: Matches GET /Library/Books/Search?author=Name
        // Uses [FromQuery] to handle optional URL parameters
        [HttpGet("Search")]
        public IActionResult Search([FromQuery] string author)
        {
            if (string.IsNullOrWhiteSpace(author))
            {
                return RedirectToAction("Index");
            }

            var filteredBooks = _books
                .Where(b => b.Author.Contains(author, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Reuses the Index view to display the filtered results
            return View("Index", filteredBooks);
        }

        // API: Matches GET /Library/Books/api/all
        // Returns raw JSON data instead of an HTML view
        [HttpGet("api/all")]
        public IActionResult GetBooksApi()
        {
            return Ok(_books);
        }
    }
}
