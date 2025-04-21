using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;

public class EmailService : IEmailService
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

    public async Task SendBookIssuedEmailAsync(string toEmail, string userName, DateTime issueDate, DateTime dueDate)
    {
        try
        {
            string filePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "BookIssuedTemplate.html");
            string emailBody = await File.ReadAllTextAsync(filePath);

            emailBody = emailBody
                .Replace("{{UserName}}", userName)
                .Replace("{{IssueDate}}", issueDate.ToString("dd MMM yyyy"))
                .Replace("{{ReturnDate}}", dueDate.ToString("dd MMM yyyy"))
                .Replace("{{FinePerDay}}", "5");

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail),
                Subject = "📚 Book Issued Notification",
                Body = emailBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send book issued email to {Email}", toEmail);
        }
    }

    public async Task SendBookReturnedEmailAsync(string toEmail, string userName, DateTime issueDate, DateTime dueDate, DateTime returnDate)
    {
        try
        {
            string filePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "BookReturnedTemplate.html");
            string emailBody = await File.ReadAllTextAsync(filePath);

            // Calculate fine if returnDate is later than dueDate
            int fineAmount = returnDate > dueDate ? (returnDate - dueDate).Days * 5 : 0;

            // Replace placeholders in the HTML template
            emailBody = emailBody
                .Replace("{{UserName}}", userName)
                .Replace("{{IssueDate}}", issueDate.ToString("dd MMM yyyy"))
                .Replace("{{DueDate}}", dueDate.ToString("dd MMM yyyy"))
                .Replace("{{ReturnDate}}", returnDate.ToString("dd MMM yyyy"))
                .Replace("{{FinePerDay}}", "5")
                .Replace("{{FineAmount}}", fineAmount.ToString());

            // Prepare email
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail),
                Subject = "📚 Book Returned Notification",
                Body = emailBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            // Setup SMTP client
            using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send book returned email to {Email}", toEmail);
        }
    }


    public async Task SendOverdueFineReminderEmailAsync(string toEmail, string userName, DateTime issueDate, DateTime dueDate)
    {
        try
        {
            string filePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "BookOverdueReminderTemplate.html");
            string emailBody = await File.ReadAllTextAsync(filePath);

            int finePerDay = 5;
            int overdueDays = (DateTime.Today - dueDate).Days;
            int totalFine = finePerDay * overdueDays;

            emailBody = emailBody
                .Replace("{{UserName}}", userName)
                .Replace("{{IssueDate}}", issueDate.ToString("dd MMM yyyy"))
                .Replace("{{ReturnDate}}", dueDate.ToString("dd MMM yyyy"))
                .Replace("{{OverdueDays}}", overdueDays.ToString())
                .Replace("{{FinePerDay}}", finePerDay.ToString())
                .Replace("{{TotalFine}}", totalFine.ToString());

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail),
                Subject = "⚠️ Overdue Book Reminder",
                Body = emailBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send overdue fine reminder email to {Email}", toEmail);
        }
    }

    public async Task SendReturnDueTomorrowEmailAsync(string toEmail, string userName, DateTime issueDate, DateTime dueDate)
    {
        try
        {
            string filePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "BookReturnReminderTemplate.html");
            string emailBody = await File.ReadAllTextAsync(filePath);

            emailBody = emailBody
                .Replace("{{UserName}}", userName)
                .Replace("{{IssueDate}}", issueDate.ToString("dd MMM yyyy"))
                .Replace("{{ReturnDate}}", dueDate.ToString("dd MMM yyyy"))
                .Replace("{{FinePerDay}}", "5");

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail),
                Subject = "⏰ Reminder: Return Book Tomorrow",
                Body = emailBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send book return reminder email to {Email}", toEmail);
        }
    }
}
