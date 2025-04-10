using System;
using System.Collections.Generic;

namespace ChillAndDrillApI.Model;

public partial class Table
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<TableReservation> TableReservations { get; set; } = new List<TableReservation>();
}
