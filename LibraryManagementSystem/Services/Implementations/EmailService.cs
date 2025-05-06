using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.DTOs.Notification;
using LibraryManagementSystem.Models;

public class EmailService : INotificationService
{
    private readonly EmailSettings _emailSettings;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, IWebHostEnvironment env, ILogger<EmailService> logger)
    {
        _emailSettings = options.Value;
        _env = env;
        _logger = logger;
    }

    public async Task SendAsync(NotificationMessage notificationMessage)
    {
        var borrowedBook = notificationMessage.borrowedBookDetails;

        switch (notificationMessage.NotificationType)
        {
            case LibraryManagementSystem.Enums.NotificationType.BookIssued:
                await SendEmailWithTemplateAsync(borrowedBook, "BookIssuedTemplate.html", "📚 Book Issued Notification");
                break;
            case LibraryManagementSystem.Enums.NotificationType.BookReturned:
                await SendEmailWithTemplateAsync(borrowedBook, "BookReturnedTemplate.html", "📚 Book Returned Notification");
                break;
            case LibraryManagementSystem.Enums.NotificationType.ReturnDueTomorrow:
                await SendEmailWithTemplateAsync(borrowedBook, "BookReturnReminderTemplate.html", "⏰ Reminder: Return Book Tomorrow");
                break;
            case LibraryManagementSystem.Enums.NotificationType.OverdueFineReminder:
                await SendEmailWithTemplateAsync(borrowedBook, "BookOverdueReminderTemplate.html", "⚠️ Overdue Book Reminder");
                break;
        }
    }

    private async Task SendEmailWithTemplateAsync(BorrowedBookNotificationDTO book, string templateFileName, string subject)
    {
        try
        {
            string filePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", templateFileName);
            await GenerateAndSendEmailAsync(book, filePath, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send '{Subject}' email to {Email}", subject, book.UserEmail);
        }
    }

    private async Task GenerateAndSendEmailAsync(BorrowedBookNotificationDTO book, string filePath, string subject)
    {
        try
        {
            string emailBody = await File.ReadAllTextAsync(filePath);

            emailBody = emailBody
                .Replace("{{BookTitle}}", book.BookTitle)
                .Replace("{{UserName}}", book.UserName)
                .Replace("{{IssueDate}}", book.BorrowDate)
                .Replace("{{DueDate}}", book.DueDate)
                .Replace("{{OverdueDays}}", book.OverdueDays.ToString())
                .Replace("{{FinePerDay}}", book.FinePerDay.ToString())
                .Replace("{{TotalFine}}", book.TotalFine.ToString());

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail),
                Subject = subject,
                Body = emailBody,
                IsBodyHtml = true
            };
            mail.To.Add(book.UserEmail);

            using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate or send email to {Email}", book.UserEmail);
            throw;
        }
    }
}
