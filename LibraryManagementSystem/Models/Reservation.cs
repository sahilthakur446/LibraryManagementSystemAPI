using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public DateTime? ReservedDate { get; set; }

    public int ReservationStatusId { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual Book Book { get; set; } = null!;
    public virtual ReservationStatus ReservationStatus { get; set; } = null!;
}
