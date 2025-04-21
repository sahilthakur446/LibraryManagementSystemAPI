using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class Fine
{
    public int FineId { get; set; }

    public int UserId { get; set; }

    public int Amount { get; set; }

    public int FineStatusId { get; set; }

    public DateTime? CreatedAt { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual FineStatus FineStatus { get; set; } = null!;
}
