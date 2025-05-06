using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories.Interfaces
{
    public interface IBorrowingRepository
    {
        Task<int> GetAvailableBookCopiesAsync(int bookId);
        Task<BorrowedBook> BorrowBookAsync(int bookId, int userId, int bookCopyId);
        Task<BorrowedBook> ReturnBookAsync(int copyId, int userId);
        Task<BorrowedBook> ReturnBookAsync(int borrowingId);
        Task<IList<BorrowedBook>> GetBorrowedBooksByUserIdAsync(int userId);
        Task<int> GetRemainingFineByUserIdAsync(int userId);
        Task<IList<BorrowedBook>> GetAllBorrowedBooksAsync();
        Task<IList<BorrowedBook>> GetAllOverDueBooksAsync();
        Task<IList<BorrowedBook>> GetAllBorrowedBooksDueTomorrowAsync();
        Task<IList<BorrowedBookNotificationDTO>> GetAllBorrowedBooksDueTomorrowForEmailAsync();
        Task<IList<BorrowedBookNotificationDTO>> GetAllOverDueBooksForEmailAsync();
        Task<bool> UpdateOverdueBooksStatusAsync();
    }
}
