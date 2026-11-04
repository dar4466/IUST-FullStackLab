using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace MyMvcApp.Controllers
{
    public class BooksController : Controller
    {
        // Simulating a database using a static list
        private static List<Book> _books = new List<Book>();
        private static int _nextId = 1;

        // READ: Show all books
        public IActionResult Index() => View(_books);

        // CREATE: Show the form
        public IActionResult Create() => View();

        // CREATE: Receive the submitted form data
        [HttpPost]
        public IActionResult Create(Book book)
        {
            book.Id = _nextId++;
            _books.Add(book);
            return RedirectToAction("Index");
        }

        // UPDATE: Show the edit form for a specific book
        public IActionResult Edit(int id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }

        // UPDATE: Receive the edited data
        [HttpPost]
        public IActionResult Edit(Book updatedBook)
        {
            var book = _books.FirstOrDefault(b => b.Id == updatedBook.Id);
            if (book != null)
            {
                book.Title = updatedBook.Title;
                book.Author = updatedBook.Author;
            }
            return RedirectToAction("Index");
        }

        // DELETE: Remove the book immediately
        public IActionResult Delete(int id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book != null) _books.Remove(book);
            return RedirectToAction("Index");
        }
    }
}
