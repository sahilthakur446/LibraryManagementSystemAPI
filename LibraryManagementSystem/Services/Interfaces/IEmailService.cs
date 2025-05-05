using LibraryManagementSystem.DTOs.Borrowing;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendBookIssuedEmailAsync(string toEmail, string userName, DateTime issueDate, DateTime dueDate);
        Task SendBookReturnedEmailAsync(string toEmail, string userName, DateTime issueDate, DateTime dueDate, DateTime returnDate);
        Task SendReturnDueTomorrowEmailAsync(BorrowedBookEmailDTO dueTomorrowBook);
        Task SendOverdueFineReminderEmailAsync(BorrowedBookEmailDTO dueTomorrowBook);

    }

}
