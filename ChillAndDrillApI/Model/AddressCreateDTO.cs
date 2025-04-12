namespace ChillAndDrillApI.Model;

public class AddressCreateDTO
{
    public int UserId { get; set; }
    public string AddressText { get; set; } = null!;
    public bool IsDefault { get; set; }
}