

using LibraryManagementSystem.Settings;

namespace LibraryManagementSystem.DTOs.Borrowing
{
    public class BorrowedBookNotificationDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public string BookTitle { get; set; }
        public string BorrowDate { get; set; }
        public string ReturnDate { get; set; } = string.Empty;
        public string DueDate { get; set; }
        public string FinePerDay { get; set; } = LibrarySettings.FinePerDay.ToString();
        public string TotalFine { get; set; } = string.Empty;
        public string OverdueDays { get; set; } = string.Empty;
    }

}
