
namespace ChillAndDrillApI.Model;

public class UserDTO
{
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public int? RoleId { get; set; }

    public static implicit operator UserDTO(Controllers.UserDTO v)
    {
        throw new NotImplementedException();
    }
}