namespace ChillAndDrillApI.Model;

// DTO для ответа Cart
public class CartResponseDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemResponseDTO> CartItems { get; set; } = new List<CartItemResponseDTO>();
}

// DTO для ответа CartItem
public class CartItemResponseDTO
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = null!;
    public decimal MenuItemPrice { get; set; }
    public int Quantity { get; set; }
    public DateTime? CreatedAt { get; set; }
}