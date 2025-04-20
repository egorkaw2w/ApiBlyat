// ChillAndDrillApI/Controllers/RolesController.cs
using System;
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
    public class RolesController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public RolesController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
        {
            return await _context.Roles
                .Where(r => r.Id != 1) // Исключаем роль клиента
                .Select(r => new RoleDTO
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDTO>> GetRole(int id)
        {
            // Исключаем роль клиента
            if (id == 1)
            {
                return BadRequest(new { message = "Роль клиента недоступна для просмотра." });
            }

            var role = await _context.Roles
                .Select(r => new RoleDTO
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<RoleDTO>> PostRole(RoleCreateDTO roleDTO)
        {
            // Проверяем, не занято ли имя роли
            if (await _context.Roles.AnyAsync(r => r.Name == roleDTO.Name))
            {
                return BadRequest(new { message = "Роль с таким именем уже существует." });
            }

            // Создаём новую роль
            var role = new Role
            {
                Name = roleDTO.Name
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Формируем DTO для ответа
            var result = new RoleDTO
            {
                Id = role.Id,
                Name = role.Name
            };

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, result);
        }

        // PUT: api/Roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, RoleDTO roleDTO)
        {
            // Исключаем роль клиента
            if (id == 1)
            {
                return BadRequest(new { message = "Роль клиента недоступна для редактирования." });
            }

            if (id != roleDTO.Id)
            {
                return BadRequest();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Проверяем, не занято ли имя роли другой ролью
            if (roleDTO.Name != role.Name && await _context.Roles.AnyAsync(r => r.Name == roleDTO.Name && r.Id != id))
            {
                return BadRequest(new { message = "Роль с таким именем уже существует." });
            }

            // Обновляем данные роли
            role.Name = roleDTO.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
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

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            // Исключаем роль клиента
            if (id == 1)
            {
                return BadRequest(new { message = "Роль клиента недоступна для удаления." });
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Проверяем, не привязаны ли пользователи к этой роли
            if (await _context.Users.AnyAsync(u => u.RoleId == id))
            {
                return BadRequest(new { message = "Нельзя удалить роль, к которой привязаны пользователи." });
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }

    public class RoleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class RoleCreateDTO
    {
        public string Name { get; set; } = null!;
    }
}