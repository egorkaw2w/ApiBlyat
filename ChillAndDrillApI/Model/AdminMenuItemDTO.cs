namespace ChillAndDrillApI.Model;

public class AdminMenuItemDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = null!;
    public int CategoryId { get; set; }
}