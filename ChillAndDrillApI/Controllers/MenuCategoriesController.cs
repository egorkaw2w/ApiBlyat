// ChillAndDrillApI/Controllers/MenuCategoriesController.cs
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
    public class MenuCategoriesController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public MenuCategoriesController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // GET: api/MenuCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuCategory>>> GetMenuCategories()
        {
            var categories = await _context.MenuCategories
                .Select(c => new MenuCategory
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/MenuCategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuCategory>> GetMenuCategory(int id)
        {
            var category = await _context.MenuCategories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }
    }
}