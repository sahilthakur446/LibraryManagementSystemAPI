using LibraryManagementSystem.DTOs.Notification;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface INotificationService
    {
        public Task SendAsync(NotificationMessage notificationMessage);
    }
}
