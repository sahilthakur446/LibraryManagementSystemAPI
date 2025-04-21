using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DTOs.Author
{
    public class AuthorRequestDTO
    {
        [Required(ErrorMessage = "Author Name is required.")]
        [MinLength(3, ErrorMessage = "Author Name should be at least 3 characters long.")]
        [MaxLength(20, ErrorMessage = "Author Name should not exceed 100 characters.")]
        public string AuthorName { get; set; }
    }
}
