using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Settings;

namespace LibraryManagementSystem.DTOs.Notification
{
    public class NotificationMessage
    {
        public NotificationType NotificationType { get; set; }
        public BorrowedBookNotificationDTO borrowedBookDetails { get; set; } = new BorrowedBookNotificationDTO();
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
