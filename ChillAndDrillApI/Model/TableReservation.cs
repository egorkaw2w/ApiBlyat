using System;
using System.Collections.Generic;

namespace ChillAndDrillApI.Model;

public partial class TableReservation
{
    public int Id { get; set; }

    public int TableId { get; set; }

    public int? UserId { get; set; }

    public DateTime ReservationTime { get; set; }

    public int DurationMinutes { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Table Table { get; set; } = null!;

    public virtual User? User { get; set; }
}
