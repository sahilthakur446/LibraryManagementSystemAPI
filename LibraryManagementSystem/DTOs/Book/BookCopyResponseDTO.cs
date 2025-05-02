using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.DTOs.Book
{
    public class BookCopyResponseDTO
    {
        public int CopyId { get; set; }

        public int BookId { get; set; }

        public bool IsAvailable { get; set; }
    }
}
