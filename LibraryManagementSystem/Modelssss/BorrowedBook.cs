using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Modelssss;

public partial class BorrowedBook
{
    public int BorrowId { get; set; }

    public int UserId { get; set; }

    public int BookCopyId { get; set; }

    public DateTime? BorrowDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public int? FineAmount { get; set; }

    public int StatusId { get; set; }

    public virtual BookCopy BookCopy { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
