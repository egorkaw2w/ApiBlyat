using System;
using System.Collections.Generic;

namespace ChillAndDrillApI.Model;

public partial class MenuCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
