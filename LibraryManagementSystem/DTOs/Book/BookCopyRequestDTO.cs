using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DTOs.Book
{
    public class BookCopyRequestDTO
    {
        [Required(ErrorMessage = "Please Provide Book Id")]
        public int BookId { get; set; }
        [Required(ErrorMessage = "Please Provide is Book Available")]
        public bool IsAvailable { get; set; }
    }
}
