using System;
using System.Collections.Generic;

namespace ChillAndDrillApI.Model;

public partial class Address
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string AddressText { get; set; } = null!;

    public bool? IsDefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
