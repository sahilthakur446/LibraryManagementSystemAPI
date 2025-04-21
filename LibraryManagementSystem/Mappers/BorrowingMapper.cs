using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Mappers
{
    public class BorrowingMapper
    {
        public static BorrowingResponseDTO FromModel(BorrowedBook book)
        {
            return new BorrowingResponseDTO
            {
                BorrowId = book.BorrowId,
                UserId = book.UserId,
                UserName = $"{book.User?.FirstName} {book.User?.LastName}" ?? "N/A",
                BookId = book.BookCopy.BookId,
                BookName = book.BookCopy.Book?.Title ?? "N/A",
                BorrowDate = book.BorrowDate?.ToString("yyyy-MM-dd") ?? "N/A",
                DueDate = book.DueDate.ToString("yyyy-MM-dd"),
                ReturnDate = book.ReturnDate?.ToString("yyyy-MM-dd") ?? "Not Returned",
                FineAmount = book.FineAmount,
                BorrowStatus = book.BorrowedBookStatus?.StatusName ?? "Unknown"
            };
        }

        public static BorrowedBook ToModel(BorrowingRequestDTO book)
        {
            return new BorrowedBook
            {
                UserId = book.UserId,
                BookId = book.BookId
            };
        }
    }
}
