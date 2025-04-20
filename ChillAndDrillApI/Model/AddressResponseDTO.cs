namespace ChillAndDrillApI.Model;

public class AddressDTO
{
    public int Id { get; set; }
    public string AddressText { get; set; } = null!;
}

public class AddressResponseDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string AddressText { get; set; } = null!;
    public bool? IsDefault { get; set; }
}
