using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ChillAndDrillApI.Model;
using BCrypt.Net;

namespace ChillAndDrillApI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ChillAndDrillContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChillAndDrillApI.Model.UserDTO>>> GetUsers()
        {
            _logger.LogInformation("Получение списка всех пользователей");
            try
            {
                return await _context.Users
                    .Include(u => u.Role)
                    .Select(u => new ChillAndDrillApI.Model.UserDTO
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей");
                throw;
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChillAndDrillApI.Model.UserDTO>> GetUser(int id)
        {
            _logger.LogInformation("Получение пользователя с ID {Id}", id);
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Select(u => new ChillAndDrillApI.Model.UserDTO
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
                    _logger.LogWarning("Пользователь с ID {Id} не найден", id);
                    return NotFound(new { message = $"Пользователь с ID {id} не найден" });
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя с ID {Id}", id);
                throw;
            }
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<ChillAndDrillApI.Model.UserDTO>> PostUser(UserCreateDTO userDTO)
        {
            _logger.LogInformation("Создание нового пользователя с логином {Login}", userDTO.Login);
            try
            {
                // Проверяем, существует ли роль (если указана)
                if (userDTO.RoleId.HasValue)
                {
                    var role = await _context.Roles.FindAsync(userDTO.RoleId.Value);
                    if (role == null)
                    {
                        _logger.LogWarning("Роль с ID {RoleId} не найдена", userDTO.RoleId);
                        return BadRequest(new { message = "Указанная роль не существует." });
                    }
                }

                // Проверяем, не занят ли логин
                if (await _context.Users.AnyAsync(u => u.Login == userDTO.Login))
                {
                    _logger.LogWarning("Логин {Login} уже занят", userDTO.Login);
                    return BadRequest(new { message = "Логин уже занят." });
                }

                // Проверяем, не занят ли email (если указан)
                if (userDTO.Email != null && await _context.Users.AnyAsync(u => u.Email == userDTO.Email))
                {
                    _logger.LogWarning("Email {Email} уже занят", userDTO.Email);
                    return BadRequest(new { message = "Email уже занят." });
                }

                // Проверяем пароль
                if (string.IsNullOrEmpty(userDTO.PasswordHash))
                {
                    _logger.LogWarning("Пароль не указан для пользователя {Login}", userDTO.Login);
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
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Формируем DTO для ответа
                var result = new ChillAndDrillApI.Model.UserDTO
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

                _logger.LogInformation("Пользователь {Login} успешно создан", userDTO.Login);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя {Login}", userDTO.Login);
                throw;
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, ChillAndDrillApI.Model.UserDTO userDTO)
        {
            _logger.LogInformation("Обновление пользователя с ID {Id}", id);
            try
            {
                if (id != userDTO.Id)
                {
                    _logger.LogWarning("ID в запросе не совпадает с ID в теле");
                    return BadRequest(new { message = "ID пользователя не совпадает." });
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с ID {Id} не найден", id);
                    return NotFound(new { message = $"Пользователь с ID {id} не найден" });
                }

                // Проверяем, не занят ли логин другим пользователем
                if (userDTO.Login != user.Login && await _context.Users.AnyAsync(u => u.Login == userDTO.Login && u.Id != id))
                {
                    _logger.LogWarning("Логин {Login} уже занят другим пользователем", userDTO.Login);
                    return BadRequest(new { message = "Логин уже занят." });
                }

                // Проверяем, не занят ли email другим пользователем (если email указан)
                if (userDTO.Email != null && userDTO.Email != user.Email && await _context.Users.AnyAsync(u => u.Email == userDTO.Email && u.Id != id))
                {
                    _logger.LogWarning("Email {Email} уже занят другим пользователем", userDTO.Email);
                    return BadRequest(new { message = "Email уже занят." });
                }

                // Проверяем, существует ли роль (если указана)
                if (userDTO.RoleId.HasValue)
                {
                    var role = await _context.Roles.FindAsync(userDTO.RoleId.Value);
                    if (role == null)
                    {
                        _logger.LogWarning("Роль с ID {RoleId} не найдена", userDTO.RoleId);
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
                        _logger.LogWarning("Пользователь с ID {Id} не найден при обновлении", id);
                        return NotFound(new { message = $"Пользователь с ID {id} не найден" });
                    }
                    else
                    {
                        _logger.LogError("Конфликт при обновлении пользователя с ID {Id}", id);
                        throw;
                    }
                }

                _logger.LogInformation("Пользователь с ID {Id} успешно обновлен", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пользователя с ID {Id}", id);
                throw;
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation("Удаление пользователя с ID {Id}", id);
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с ID {Id} не найден", id);
                    return NotFound(new { message = $"Пользователь с ID {id} не найден" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Пользователь с ID {Id} успешно удален", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя с ID {Id}", id);
                throw;
            }
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            _logger.LogInformation("Попытка входа для пользователя {Login}", loginDto.Login);
            try
            {
                if (string.IsNullOrEmpty(loginDto.Login) || string.IsNullOrEmpty(loginDto.Password))
                {
                    _logger.LogWarning("Логин или пароль пустой");
                    return BadRequest(new { error = "Логин и пароль обязательны" });
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Login == loginDto.Login);

                if (user == null)
                {
                    _logger.LogWarning("Пользователь с логином {Login} не найден", loginDto.Login);
                    return Unauthorized(new { error = "Неверный логин или пароль" });
                }

                // Проверяем пароль с использованием BCrypt
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Неверный пароль для пользователя {Login}", loginDto.Login);
                    return Unauthorized(new { error = "Неверный логин или пароль" });
                }

                // Формируем ответ с данными пользователя
                var userResponse = new ChillAndDrillApI.Model.UserDTO
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

                _logger.LogInformation("Пользователь {Login} успешно вошел", loginDto.Login);
                return Ok(new { data = userResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе пользователя {Login}", loginDto.Login);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
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

    public class LoginDTO
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}