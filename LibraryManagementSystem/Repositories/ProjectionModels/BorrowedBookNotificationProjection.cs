using LibraryManagementSystem.Settings;

namespace LibraryManagementSystem.Repositories.ProjectionModels
{
    public class BorrowedBookNotificationProjection
    {
        public int UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; } = null;
        public DateTime DueDate { get; set; }
    }
}
