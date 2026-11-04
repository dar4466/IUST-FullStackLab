using System.ComponentModel.DataAnnotations;
namespace MyMvcApp.Models
{
public class Book
{
public int Id { get; set; }
[Required(ErrorMessage = "The Title field is required.")]
[StringLength(100, MinimumLength = 2, ErrorMessage ="Title must be between 2 and 100 characters.")]
public string Title { get; set; } = string.Empty;
[Required(ErrorMessage = "Please provide the author's name.")]
[StringLength(50, ErrorMessage = "Author name cannot exceed 50 characters.")]
public string Author { get; set; } = string.Empty;
}
}