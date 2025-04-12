// ChillAndDrillApI/Model/CartItemCreateDTO.cs
namespace ChillAndDrillApI.Model;

public class CartItemCreateDTO
{
    public int CartId { get; set; }
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public DateTime? CreatedAt { get; set; }
}