// ChillAndDrillApI/Controllers/MenuItemsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChillAndDrillApI.Model;

namespace ChillAndDrillApI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemsController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public MenuItemsController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // GET: api/MenuItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemResponseDTO>>> GetMenuItems()
        {
            return await _context.MenuItems
                .Include(m => m.Category)
                .Select(m => new MenuItemResponseDTO
                {
                    Id = m.Id,
                    CategoryId = m.CategoryId,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    ImageUrl = m.ImageUrl, // Теперь используем ImageUrl напрямую
                    CategoryName = m.Category.Name
                })
                .ToListAsync();
        }

        // GET: api/MenuItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItemResponseDTO>> GetMenuItem(int id)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .Select(m => new MenuItemResponseDTO
                {
                    Id = m.Id,
                    CategoryId = m.CategoryId,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    ImageUrl = m.ImageUrl, // Теперь используем ImageUrl напрямую
                    CategoryName = m.Category.Name
                })
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return menuItem;
        }

        // PUT: api/MenuItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenuItem(int id, [FromBody] MenuItemDTO menuItemDTO)
        {
            if (id != menuItemDTO.Id)
            {
                return BadRequest();
            }

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            menuItem.CategoryId = menuItemDTO.CategoryId;
            menuItem.Name = menuItemDTO.Name;
            menuItem.Description = menuItemDTO.Description;
            menuItem.Price = menuItemDTO.Price;
            menuItem.ImageUrl = menuItemDTO.ImageUrl;
            menuItem.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(id))
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

        // POST: api/MenuItems
        [HttpPost]
        public async Task<ActionResult<MenuItem>> PostMenuItem([FromBody] MenuItemDTO menuItemDTO)
        {
            var menuItem = new MenuItem
            {
                CategoryId = menuItemDTO.CategoryId,
                Name = menuItemDTO.Name,
                Description = menuItemDTO.Description,
                Price = menuItemDTO.Price,
                ImageUrl = menuItemDTO.ImageUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }

        // DELETE: api/MenuItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }

    public class MenuItemResponseDTO
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = null!;
    }

    public class MenuItemDTO
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}