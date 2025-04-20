using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChillAndDrillApI.Model;

public partial class Cart
{
    public int Id { get; set; }

    [Required(ErrorMessage = "UserId is required")]
    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User User { get; set; } = null!;
}