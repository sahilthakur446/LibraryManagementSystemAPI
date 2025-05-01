using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Modelss;

public partial class BookCopy
{
    public int CopyId { get; set; }

    public int BookId { get; set; }

    public bool IsAvailable { get; set; }
}
