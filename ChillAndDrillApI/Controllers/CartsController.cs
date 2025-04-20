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
    public class CartsController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public CartsController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // GET: api/Carts?userId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartResponseDTO>>> GetCarts(int? userId)
        {
            var query = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.MenuItem)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value);
            }

            var carts = await query
                .Select(c => new CartResponseDTO
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CartItems = c.CartItems.Select(ci => new CartItemResponseDTO
                    {
                        Id = ci.Id,
                        MenuItemId = ci.MenuItemId,
                        MenuItemName = ci.MenuItem.Name,
                        MenuItemPrice = ci.MenuItem.Price,
                        Quantity = ci.Quantity,
                        CreatedAt = ci.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return carts;
        }

        // GET: api/Carts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CartResponseDTO>> GetCart(int id)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.MenuItem)
                .Select(c => new CartResponseDTO
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CartItems = c.CartItems.Select(ci => new CartItemResponseDTO
                    {
                        Id = ci.Id,
                        MenuItemId = ci.MenuItemId,
                        MenuItemName = ci.MenuItem.Name,
                        MenuItemPrice = ci.MenuItem.Price,
                        Quantity = ci.Quantity,
                        CreatedAt = ci.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        // PUT: api/Carts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCart(int id, Cart cart)
        {
            if (id != cart.Id)
            {
                return BadRequest(new { message = "ID в теле запроса не совпадает с ID в URL" });
            }

            var user = await _context.Users.FindAsync(cart.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "Пользователь с указанным UserId не существует" });
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
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

        // POST: api/Carts
        [HttpPost]
        public async Task<ActionResult<Cart>> PostCart([FromBody] CartCreateDTO cartDto)
        {
            Console.WriteLine($"Получен запрос POST /api/Carts с userId={cartDto.UserId}");

            // Проверяем, существует ли пользователь
            var user = await _context.Users.FindAsync(cartDto.UserId);
            if (user == null)
            {
                Console.WriteLine($"Пользователь с UserId={cartDto.UserId} не найден.");
                return BadRequest(new { message = "Пользователь с указанным UserId не существует" });
            }

            // Создаём объект Cart
            var cart = new Cart
            {
                UserId = cartDto.UserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Carts.Add(cart);
            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Корзина успешно создана с Id={cart.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении корзины: {ex.Message}");
                return StatusCode(500, new { message = "Ошибка сервера при создании корзины" });
            }

            return CreatedAtAction("GetCart", new { id = cart.Id }, cart);
        }

        // DELETE: api/Carts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }

    public class CartCreateDTO
    {
        public int UserId { get; set; }
    }
}