using System;
using System.Collections.Generic;

namespace ChillAndDrillApI.Model;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AddressId { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;
}
