using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChillAndDrillApI.Model;
using BCrypt.Net;
using Npgsql;
using Microsoft.Extensions.Logging;

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

        private IActionResult ApiResponse(object data = null, string error = null, int statusCode = 200)
        {
            var response = new { Success = error == null, Data = data, Error = error };
            return StatusCode(statusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Fetching all users");
            var users = await _context.Users.ToListAsync();
            return ApiResponse(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            _logger.LogInformation("Fetching user with ID: {Id}", id);
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID: {Id} not found", id);
                return ApiResponse(error: "Пользователь не найден", statusCode: 404);
            }

            var userData = new
            {
                id = user.Id,
                fullName = user.FullName,
                birthDate = user.BirthDate?.ToString("yyyy-MM-dd"), // Формат для HTML input type="date"
                email = user.Email,
                avatarUrl = user.AvatarUrl
                // Gender отсутствует в модели, добавить в User.cs, если нужно
            };
            return ApiResponse(userData);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserUpdateRequest request)
        {
            _logger.LogInformation("Updating user with ID: {Id}", id);

            if (request == null || id != request.Id)
            {
                _logger.LogWarning("Invalid request: ID mismatch or null request");
                return ApiResponse(error: "Неверный запрос", statusCode: 400);
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                _logger.LogWarning("User with ID: {Id} not found", id);
                return ApiResponse(error: "Пользователь не найден", statusCode: 404);
            }

            existingUser.FullName = request.FullName ?? existingUser.FullName;
            existingUser.BirthDate = request.BirthDate ?? existingUser.BirthDate;
            existingUser.Email = request.Email ?? existingUser.Email;
            existingUser.AvatarUrl = request.AvatarUrl ?? existingUser.AvatarUrl;
            // Gender отсутствует в модели, добавить в User.cs, если нужно

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID: {Id} updated successfully", id);
                return ApiResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {Id}", id);
                return ApiResponse(error: "Ошибка при обновлении пользователя", statusCode: 500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] UserCreateRequest request)
        {
            _logger.LogInformation("Creating new user");

            if (request == null || !IsValidUserCreateRequest(request))
            {
                _logger.LogWarning("Invalid user creation request");
                return ApiResponse(error: "Все обязательные поля должны быть заполнены", statusCode: 400);
            }

            var user = new User
            {
                Login = request.Login,
                FullName = request.FullName,
                Phone = request.Phone,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                BirthDate = request.BirthDate,
                AvatarUrl = request.AvatarUrl
                // Gender отсутствует в модели, добавить в User.cs, если нужно
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User created with ID: {Id}", user.Id);
                return ApiResponse(new { id = user.Id }, statusCode: 201);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                string detail = pgEx.ConstraintName switch
                {
                    "users_login_key" => "Логин уже занят",
                    "users_email_key" => "Email уже занят",
                    _ => "Нарушение уникальности"
                };
                _logger.LogWarning("Unique constraint violation: {Detail}", detail);
                return ApiResponse(error: detail, statusCode: 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return ApiResponse(error: "Ошибка при создании пользователя", statusCode: 500);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for user: {Login}", request?.Login);

            if (request == null || string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password))
            {
                _logger.LogWarning("Login request invalid: missing login or password");
                return ApiResponse(error: "Логин и пароль обязательны", statusCode: 400);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid login attempt for user: {Login}", request.Login);
                return ApiResponse(error: "Неверный логин или пароль", statusCode: 401);
            }

            _logger.LogInformation("Login successful for user: {Login}", request.Login);
            return ApiResponse(new { id = user.Id, fullName = user.FullName });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation("Deleting user with ID: {Id}", id);
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID: {Id} not found", id);
                return ApiResponse(error: "Пользователь не найден", statusCode: 404);
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID: {Id} deleted successfully", id);
                return ApiResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {Id}", id);
                return ApiResponse(error: "Ошибка при удалении пользователя", statusCode: 500);
            }
        }

        private bool IsValidUserCreateRequest(UserCreateRequest request) =>
            !string.IsNullOrEmpty(request.Login) &&
            !string.IsNullOrEmpty(request.FullName) &&
            !string.IsNullOrEmpty(request.Phone) &&
            !string.IsNullOrEmpty(request.Email) &&
            !string.IsNullOrEmpty(request.Password);
    }

    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class UserCreateRequest
    {
        public string Login { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class UserUpdateRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }
}