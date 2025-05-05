using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
            _logger.LogInformation("Email background service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Checking for due and overdue emails to send...");
                    await ProcessDueEmails(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing due or overdue emails.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Email background service is stopping.");
        }

        private async Task ProcessDueEmails(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var borrowingService = scope.ServiceProvider.GetRequiredService<IBorrowingService>();

            var dueTomorrowBooks = await borrowingService.GetBorrowedBooksDueTomorrowForEmailAsync();

            bool statusUpdateResult = await borrowingService.UpdateOverdueBooksStatusAsync();
            _logger.LogInformation("Overdue book status update result: {Status}", statusUpdateResult);

            var overdueBooks = await borrowingService.GetAllOverDueBooksForEmailAsync();

            foreach (var book in dueTomorrowBooks)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    await emailService.SendReturnDueTomorrowEmailAsync(book);
                    _logger.LogInformation("Sent 'due tomorrow' notification to {UserEmail} for book borrowed on {BorrowDate}",
                        book.UserEmail, book.BorrowDate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send 'due tomorrow' email to {UserEmail}", book.UserEmail);
                }
            }

            foreach (var book in overdueBooks)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    await emailService.SendOverdueFineReminderEmailAsync(book);
                    _logger.LogInformation("Sent 'overdue' fine reminder to {UserEmail} for book borrowed on {BorrowDate}",
                        book.UserEmail, book.BorrowDate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send 'overdue' fine reminder to {UserEmail}", book.UserEmail);
                }
            }

            _logger.LogInformation("Processed {DueCount} due tomorrow notifications and {OverdueCount} overdue notifications",
                dueTomorrowBooks.Count(), overdueBooks.Count());
        }
    }
}
