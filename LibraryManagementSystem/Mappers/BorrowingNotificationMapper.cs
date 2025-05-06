using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.Repositories.ProjectionModels;
using LibraryManagementSystem.Settings;

namespace LibraryManagementSystem.Mappers
{
    public class BorrowingNotificationMapper
    {
        // For borrowings due tomorrow (no overdue/fine)
        public static BorrowedBookNotificationDTO FromProjectionToDTO(BorrowedBookNotificationProjection bb)
        {
            return new BorrowedBookNotificationDTO
            {
                BookTitle = bb.BookTitle,
                UserName = $"{bb.UserFirstName} {bb.UserLastName}",
                UserEmail = bb.UserEmail,
                UserPhone = bb.UserPhone,
                BorrowDate = bb.BorrowDate.ToString("yyyy-MMM-dd"),
                DueDate = bb.DueDate.ToString("yyyy-MMM-dd"),
                FinePerDay = LibrarySettings.FinePerDay.ToString(),
                OverdueDays = "0",
                TotalFine = "0"
            };
        }

        // For overdue borrowings (includes fine)
        public static BorrowedBookNotificationDTO FromProjectionToDTO(BorrowedBookNotificationProjection bb, int overdueDays, int totalFine)
        {
            return new BorrowedBookNotificationDTO
            {
                BookTitle = bb.BookTitle,
                UserName = $"{bb.UserFirstName} {bb.UserLastName}",
                UserEmail = bb.UserEmail,
                UserPhone = bb.UserPhone,
                BorrowDate = bb.BorrowDate.ToString("yyyy-MMM-dd"),
                DueDate = bb.DueDate.ToString("yyyy-MMM-dd"),
                FinePerDay = LibrarySettings.FinePerDay.ToString(),
                OverdueDays = overdueDays.ToString(),
                TotalFine = totalFine.ToString()
            };
        }
    }
}
