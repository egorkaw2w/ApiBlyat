// ChillAndDrillApI/Controllers/CartItemsController.cs
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
    public class CartItemsController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public CartItemsController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // GET: api/CartItems?cartId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemResponseDTO>>> GetCartItems(int? cartId)
        {
            var query = _context.CartItems
                .Include(ci => ci.MenuItem)
                .AsQueryable();

            if (cartId.HasValue)
            {
                query = query.Where(ci => ci.CartId == cartId.Value);
            }

            var cartItems = await query
                .Select(ci => new CartItemResponseDTO
                {
                    Id = ci.Id,
                    MenuItemId = ci.MenuItemId,
                    MenuItemName = ci.MenuItem.Name,
                    MenuItemPrice = ci.MenuItem.Price,
                    Quantity = ci.Quantity,
                    CreatedAt = ci.CreatedAt
                })
                .ToListAsync();

            return cartItems;
        }

        // GET: api/CartItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CartItemResponseDTO>> GetCartItem(int id)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.MenuItem)
                .Select(ci => new CartItemResponseDTO
                {
                    Id = ci.Id,
                    MenuItemId = ci.MenuItemId,
                    MenuItemName = ci.MenuItem.Name,
                    MenuItemPrice = ci.MenuItem.Price,
                    Quantity = ci.Quantity,
                    CreatedAt = ci.CreatedAt
                })
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (cartItem == null)
            {
                return NotFound();
            }

            return cartItem;
        }

        // POST: api/CartItems
        [HttpPost]
        public async Task<ActionResult<CartItemResponseDTO>> PostCartItem(CartItemCreateDTO cartItemDto)
        {
            cartItemDto.CreatedAt = DateTime.Now;
            // Проверяем, существует ли корзина
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartItemDto.CartId);
            if (cart == null)
            {
                return BadRequest("Корзина не найдена");
            }

            // Проверяем, существует ли MenuItem
            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == cartItemDto.MenuItemId);
            if (menuItem == null)
            {
                return BadRequest("Товар не найден");
            }

            // Проверяем, существует ли уже такой товар в корзине
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartItemDto.CartId && ci.MenuItemId == cartItemDto.MenuItemId);

            CartItem cartItem;
            if (existingItem != null)
            {
                // Если товар уже есть, увеличиваем количество
                existingItem.Quantity += cartItemDto.Quantity;
                cartItem = existingItem;
                _context.Entry(existingItem).State = EntityState.Modified;
            }
            else
            {
                // Иначе создаём новый
                cartItem = new CartItem
                {
                    CartId = cartItemDto.CartId,
                    MenuItemId = cartItemDto.MenuItemId,
                    Quantity = cartItemDto.Quantity,
                    CreatedAt = cartItemDto.CreatedAt ?? DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // Формируем ответ
            var createdItem = await _context.CartItems
                .Include(ci => ci.MenuItem)
                .Where(ci => ci.Id == cartItem.Id)
                .Select(ci => new CartItemResponseDTO
                {
                    Id = ci.Id,
                    MenuItemId = ci.MenuItemId,
                    MenuItemName = ci.MenuItem.Name,
                    MenuItemPrice = ci.MenuItem.Price,
                    Quantity = ci.Quantity,
                    CreatedAt = ci.CreatedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction("GetCartItem", new { id = createdItem!.Id }, createdItem);
        }

        // PUT: api/CartItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCartItem(int id, CartItemUpdateDTO cartItemDto)
        {
            if (id != cartItemDto.Id)
            {
                return BadRequest("ID в теле запроса не совпадает с ID в URL");
            }

            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            // Обновляем количество
            cartItem.Quantity = cartItemDto.Quantity;

            // Если количество <= 0, удаляем элемент
            if (cartItem.Quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                _context.Entry(cartItem).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartItemExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/CartItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartItemExists(int id)
        {
            return _context.CartItems.Any(e => e.Id == id);
        }
    }
}