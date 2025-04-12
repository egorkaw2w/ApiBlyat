namespace ChillAndDrillApI.Model;

public class MenuItemDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = null!;
    public int CategoryId { get; set; }
}
public class MenuItemResponseDTO
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = null!;
}