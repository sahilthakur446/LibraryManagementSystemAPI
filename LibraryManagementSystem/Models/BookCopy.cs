using LibraryManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class BookCopy
{
    public int CopyId { get; set; }

    public int BookId { get; set; }

    public bool IsAvailable { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual ICollection<BorrowedBook> BorrowedBooks { get; set; } = new List<BorrowedBook>();
}
