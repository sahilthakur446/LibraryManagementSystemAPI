using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public int AuthorId { get; set; }

    public string Isbn { get; set; } = null!;

    public int CategoryId { get; set; }

    public int PublishedYear { get; set; }

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;
    public virtual Author Author { get; set; }
    public virtual Category Category { get; set; }
    public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
}
