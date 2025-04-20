// ChillAndDrillApI/Controllers/UsersController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChillAndDrillApI.Model;
using BCrypt.Net; // Для хеширования пароля

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
                    Login = u.Login,
                    FullName = u.FullName,
                    BirthDate = u.BirthDate,
                    Phone = u.Phone,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : "Без роли",
                    CreatedAt = u.CreatedAt
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
                    Login = u.Login,
                    FullName = u.FullName,
                    BirthDate = u.BirthDate,
                    Phone = u.Phone,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : "Без роли",
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(UserCreateDTO userDTO)
        {
            // Проверяем, существует ли роль (если указана)
            if (userDTO.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(userDTO.RoleId.Value);
                if (role == null)
                {
                    return BadRequest(new { message = "Указанная роль не существует." });
                }
            }

            // Проверяем, не занят ли логин
            if (await _context.Users.AnyAsync(u => u.Login == userDTO.Login))
            {
                return BadRequest(new { message = "Логин уже занят." });
            }

            // Проверяем, не занят ли email (если указан)
            if (userDTO.Email != null && await _context.Users.AnyAsync(u => u.Email == userDTO.Email))
            {
                return BadRequest(new { message = "Email уже занят." });
            }

            // Хешируем пароль
            if (string.IsNullOrEmpty(userDTO.PasswordHash))
            {
                return BadRequest(new { message = "Пароль обязателен." });
            }
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.PasswordHash);

            // Создаём нового пользователя
            var user = new User
            {
                Login = userDTO.Login,
                FullName = userDTO.FullName,
                BirthDate = userDTO.BirthDate,
                Phone = userDTO.Phone,
                Email = userDTO.Email,
                PasswordHash = passwordHash,
                RoleId = userDTO.RoleId,
                AvatarUrl = userDTO.AvatarUrl,
                CreatedAt = DateTime.Now // Устанавливаем дату создания
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Формируем DTO для ответа
            var result = new UserDTO
            {
                Id = user.Id,
                Login = user.Login,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                Phone = user.Phone,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                RoleId = user.RoleId,
                RoleName = user.Role != null ? user.Role.Name : "Без роли",
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, result);
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

            // Проверяем, не занят ли логин другим пользователем
            if (userDTO.Login != user.Login && await _context.Users.AnyAsync(u => u.Login == userDTO.Login && u.Id != id))
            {
                return BadRequest(new { message = "Логин уже занят." });
            }

            // Проверяем, не занят ли email другим пользователем (если email указан)
            if (userDTO.Email != null && userDTO.Email != user.Email && await _context.Users.AnyAsync(u => u.Email == userDTO.Email && u.Id != id))
            {
                return BadRequest(new { message = "Email уже занят." });
            }

            // Проверяем, существует ли роль (если указана)
            if (userDTO.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(userDTO.RoleId.Value);
                if (role == null)
                {
                    return BadRequest(new { message = "Указанная роль не существует." });
                }
            }

            // Обновляем данные пользователя
            user.Login = userDTO.Login;
            user.FullName = userDTO.FullName;
            user.BirthDate = userDTO.BirthDate;
            user.Phone = userDTO.Phone;
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

    public class UserCreateDTO
    {
        public string Login { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateOnly? BirthDate { get; set; }
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string PasswordHash { get; set; } = null!;
        public int? RoleId { get; set; }
    }
}