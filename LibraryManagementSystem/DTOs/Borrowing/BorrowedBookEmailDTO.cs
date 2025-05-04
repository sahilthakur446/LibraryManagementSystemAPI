

namespace LibraryManagementSystem.DTOs.Borrowing
{
    public class BorrowedBookEmailDTO
    {
        public string BookTitle { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string BorrowDate { get; set; }
        public string DueDate { get; set; }
    }

}
