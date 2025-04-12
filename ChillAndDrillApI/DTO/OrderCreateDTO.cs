// ChillAndDrillApI/Model/OrderCreateDTO.cs
namespace ChillAndDrillApI.Model;

public class OrderCreateDTO
{
    public int UserId { get; set; }
    public int AddressId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = null!;
    public List<OrderItemCreateDTO> OrderItems { get; set; } = new();
}

public class OrderItemCreateDTO
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
}