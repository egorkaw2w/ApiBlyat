using System;
using System.Collections.Generic;

namespace ChillAndDrillApI.Model;

public class OrderResponseDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AddressId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemResponseDTO> OrderItems { get; set; } = new();
    public AddressDTO Address { get; set; } = null!;
    public UserDTO User { get; set; } = null!;
}

public class OrderItemResponseDTO
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
}