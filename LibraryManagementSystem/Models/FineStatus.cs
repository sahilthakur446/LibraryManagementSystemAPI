using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class FineStatus
{
    public int FineStatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();
}
