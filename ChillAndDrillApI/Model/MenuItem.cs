// ChillAndDrillApI/Model/MenuItem.cs
namespace ChillAndDrillApI.Model;

public partial class MenuItem
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ImageUrl { get; set; } // Заменяем byte[]? ImageData на string? ImageUrl

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual MenuCategory Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}