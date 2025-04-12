using System;
using System.Collections.Generic;

namespace ChillAndDrillApI.Model;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    public string Phone { get; set; } = null!;

    public string? Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? PasswordHash { get; set; } = null!;

    public int? RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<TableReservation> TableReservations { get; set; } = new List<TableReservation>();
}
