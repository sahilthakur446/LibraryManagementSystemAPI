namespace LibraryManagementSystem.Enums
{
    public enum RolesEnum
    {
        Admin = 1,
        Student = 2,
        Teacher = 3
    }

    public enum BorrowedBookStatusEnum
    {
        Borrowed = 1,
        Returned = 2,
        Overdue = 3
    }

    public enum FineStatusEnum
    {
        Paid = 1,
        Unpaid = 2
    }

    public enum ReservationStatusEnum
    {
        Pending = 1,
        Available = 2,
        Completed = 3,
        Cancelled = 4,
        Expired = 5,
        Rejected = 6
    }
}
