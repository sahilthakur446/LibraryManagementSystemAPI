using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class BorrowedBookStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<BorrowedBook> BorrowedBooks { get; set; } = new List<BorrowedBook>();
}
