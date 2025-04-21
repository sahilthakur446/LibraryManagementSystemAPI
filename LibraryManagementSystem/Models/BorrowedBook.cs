using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class BorrowedBook
{
    public int BorrowId { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public DateTime? BorrowDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public int? FineAmount { get; set; }

    public int StatusId { get; set; }

    public virtual BorrowedBookStatus Status { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Book Book { get; set; } = null!;
}
