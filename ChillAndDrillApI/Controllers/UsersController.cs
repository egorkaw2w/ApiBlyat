// ChillAndDrillApI/Controllers/UsersController.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChillAndDrillApI.Model;

namespace ChillAndDrillApI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public UsersController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    RoleId = u.RoleId ?? 0,
                    RoleName = u.Role != null ? u.Role.Name : "Без роли"
                })
                .ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    RoleId = u.RoleId ?? 0,
                    RoleName = u.Role != null ? u.Role.Name : "Без роли"
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserDTO userDTO)
        {
            if (id != userDTO.Id)
            {
                return BadRequest();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = userDTO.FullName;
            user.Email = userDTO.Email;
            user.AvatarUrl = userDTO.AvatarUrl;
            user.RoleId = userDTO.RoleId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }
}