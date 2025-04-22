// ChillAndDrillApI/Controllers/TablesController.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChillAndDrillApI.Model;

namespace ChillAndDrillApI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TablesController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public TablesController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // GET: api/Tables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TableDTO>>> GetTables()
        {
            return await _context.Tables
                .Select(t => new TableDTO
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToListAsync();
        }

        // GET: api/Tables/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TableDTO>> GetTable(int id)
        {
            var table = await _context.Tables
                .Select(t => new TableDTO
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound();
            }

            return table;
        }

        // POST: api/Tables
        [HttpPost]
        public async Task<ActionResult<TableDTO>> CreateTable(TableDTO tableDTO)
        {
            // Валидация входных данных
            if (tableDTO == null || string.IsNullOrWhiteSpace(tableDTO.Name))
            {
                return BadRequest("Имя таблицы не может быть пустым.");
            }

            // Проверяем, существует ли таблица с таким именем
            var existingTable = await _context.Tables
                .FirstOrDefaultAsync(t => t.Name == tableDTO.Name);
            if (existingTable != null)
            {
                return Conflict("Таблица с таким именем уже существует.");
            }

            // Создаём новую таблицу
            var table = new Table
            {
                Name = tableDTO.Name
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            // Обновляем DTO с новым Id
            tableDTO.Id = table.Id;

            return CreatedAtAction(nameof(GetTable), new { id = table.Id }, tableDTO);
        }

        // PUT: api/Tables/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, TableDTO tableDTO)
        {
            // Проверяем, совпадает ли id из URL с id в DTO
            if (id != tableDTO.Id)
            {
                return BadRequest("Идентификатор таблицы в URL не совпадает с идентификатором в теле запроса.");
            }

            // Валидация входных данных
            if (string.IsNullOrWhiteSpace(tableDTO.Name))
            {
                return BadRequest("Имя таблицы не может быть пустым.");
            }

            // Находим таблицу в базе данных
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            // Проверяем, не занято ли новое имя другой таблицей
            var existingTable = await _context.Tables
                .FirstOrDefaultAsync(t => t.Name == tableDTO.Name && t.Id != id);
            if (existingTable != null)
            {
                return Conflict("Таблица с таким именем уже существует.");
            }

            // Обновляем данные
            table.Name = tableDTO.Name;
            _context.Entry(table).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TableExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Tables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Вспомогательный метод для проверки существования таблицы
        private bool TableExists(int id)
        {
            return _context.Tables.Any(e => e.Id == id);
        }
    }

    public class TableDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}