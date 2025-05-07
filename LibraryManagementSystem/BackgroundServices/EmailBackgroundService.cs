using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.DTOs.Notification;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.BackgroundServices
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

        public EmailBackgroundService(ILogger<EmailBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Processing overdue status updates and notifications...");

                    await UpdateBookOverdueStatusIfNeededAsync(stoppingToken);
                    await ProcessAndSendNotificationsAsync(stoppingToken);

                    _logger.LogInformation("Completed daily email notification processing.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during background email processing.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Email background service stopped.");
        }

        private async Task UpdateBookOverdueStatusIfNeededAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            using var scope = _serviceProvider.CreateScope();
            var borrowingService = scope.ServiceProvider.GetRequiredService<IBorrowingService>();

            try
            {
                bool isUpdated = await borrowingService.UpdateOverdueBooksStatusAsync();
                _logger.LogInformation("Overdue books status update result: {Result}", isUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update overdue books status.");
            }
        }

        private async Task ProcessAndSendNotificationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var borrowingService = scope.ServiceProvider.GetRequiredService<IBorrowingService>();

            var dueTomorrow = await borrowingService.GetBorrowedBooksDueTomorrowForNotificationAsync();
            var overdue = await borrowingService.GetAllOverDueBooksForNotificationAsync();

            var dueNotifications = BuildNotificationMessages(dueTomorrow, NotificationType.ReturnDueTomorrow);
            var overdueNotifications = BuildNotificationMessages(overdue, NotificationType.OverdueFineReminder);

            await SendNotificationsAsync(notificationService, dueNotifications, cancellationToken);
            await SendNotificationsAsync(notificationService, overdueNotifications, cancellationToken);

            _logger.LogInformation("Sent {DueCount} due and {OverdueCount} overdue notifications.",
                dueTomorrow.Count, overdue.Count);
        }

        private List<NotificationMessage> BuildNotificationMessages(IList<BorrowedBookNotificationDTO> books, NotificationType type)
        {
            return books.Select(book => new NotificationMessage
            {
                NotificationType = type,
                borrowedBookDetails = book,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            }).ToList();
        }

        private async Task SendNotificationsAsync(INotificationService notificationService, IEnumerable<NotificationMessage> notifications, CancellationToken cancellationToken)
        {
            foreach (var notification in notifications)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await notificationService.SendAsync(notification);
                    _logger.LogInformation("Sent '{Type}' notification to {Email} (Borrowed on {Date})",
                        notification.NotificationType,
                        notification.borrowedBookDetails.UserEmail,
                        notification.borrowedBookDetails.BorrowDate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send '{Type}' notification to {Email}",
                        notification.NotificationType,
                        notification.borrowedBookDetails.UserEmail);
                }
            }
        }
    }
}
