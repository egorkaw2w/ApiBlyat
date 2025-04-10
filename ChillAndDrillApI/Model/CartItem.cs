﻿using System;
using System.Collections.Generic;

namespace ChillAndDrillApI.Model;

public partial class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int MenuItemId { get; set; }

    public int Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual MenuItem MenuItem { get; set; } = null!;
}
