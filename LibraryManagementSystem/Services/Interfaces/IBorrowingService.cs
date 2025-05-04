using LibraryManagementSystem.DTOs.Borrowing;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IBorrowingService
    {
        Task<int> GetAvailableBookCopiesAsync(int bookId);
        Task<BorrowingResponseDTO> BorrowBookAsync(int bookId, int userId, int bookCopyId = 0);
        Task<BorrowingResponseDTO> ReturnBookAsync(int bookId, int userId);
        Task<BorrowingResponseDTO> ReturnBookAsync(int borrowingId);
        Task<IList<BorrowingResponseDTO>> GetBorrowedBooksByUserIdAsync(int userId);
        Task<int> GetRemainingFineByUserIdAsync(int userId);
        Task<IList<BorrowingResponseDTO>> GetAllBorrowedBooksAsync();
        Task<IList<BorrowingResponseDTO>> GetAllOverDueBooksAsync();
        Task<IList<BorrowingResponseDTO>> GetBorrowingsDueTomorrowAsync();
        Task<IList<BorrowedBookEmailDTO>> GetBorrowedBooksDueTomorrowForEmailAsync();
        Task<bool> IsSameBookAlreadyBorrowedByUser(int bookId, int userId);
    }
}
