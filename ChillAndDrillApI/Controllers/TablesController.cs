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
    }

    public class TableDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}