namespace ChillAndDrillApI.Model;

public class UserDTO
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateOnly? BirthDate { get; set; }
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public int? RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
}