using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Title field is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide the author's name.")]
        [StringLength(50, ErrorMessage = "Author name cannot exceed 50 characters.")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the published year.")]
        [Range(1000, 2100, ErrorMessage = "Year must be between 1000 and 2100.")]
        public int PublishedYear { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1.00, 1000.00, ErrorMessage = "Price must be between $1.00 and $1000.00")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}
