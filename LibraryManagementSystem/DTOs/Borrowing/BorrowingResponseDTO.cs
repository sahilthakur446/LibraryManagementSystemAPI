using UserModel = LibraryManagementSystem.Models.User;
using BookModel = LibraryManagementSystem.Models.Book;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.DTOs.Borrowing
{
    public class BorrowingResponseDTO
    {
        public int BorrowId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string BorrowDate { get; set; }
        public string DueDate { get; set; }
        public string ReturnDate { get; set; }
        public int? FineAmount { get; set; }
        public string BorrowStatus { get; set; }
    }
}
