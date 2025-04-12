// ChillAndDrillApI/Controllers/OrdersController.cs
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
    public class OrdersController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public OrdersController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<OrderResponseDTO>> PostOrder(OrderCreateDTO orderDto)
        {
            // Проверяем существование пользователя
            var user = await _context.Users.FindAsync(orderDto.UserId);
            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            // Проверяем существование адреса
            var address = await _context.Addresses.FindAsync(orderDto.AddressId);
            if (address == null)
            {
                return BadRequest("Адрес не найден");
            }

            // Проверяем, что есть элементы заказа
            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
            {
                return BadRequest("Заказ должен содержать хотя бы один товар");
            }

            // Проверяем существование всех MenuItems
            foreach (var item in orderDto.OrderItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                if (menuItem == null)
                {
                    return BadRequest($"Товар с ID {item.MenuItemId} не найден");
                }
            }

            // Создаём заказ
            var order = new Order
            {
                UserId = orderDto.UserId,
                AddressId = orderDto.AddressId,
                TotalPrice = orderDto.TotalPrice,
                Status = orderDto.Status,
                CreatedAt = DateTime.Now,
                OrderItems = orderDto.OrderItems.Select(item => new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    PriceAtOrder = item.PriceAtOrder
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Формируем ответ
            var orderResponse = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.Id == order.Id)
                .Select(o => new OrderResponseDTO
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    AddressId = o.AddressId,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDTO
                    {
                        Id = oi.Id,
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItem.Name,
                        Quantity = oi.Quantity,
                        PriceAtOrder = oi.PriceAtOrder
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction("GetOrder", new { id = orderResponse.Id }, orderResponse);
        }

        // Для полноты добавим GET, если его нет
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Select(o => new OrderResponseDTO
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    AddressId = o.AddressId,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDTO
                    {
                        Id = oi.Id,
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItem.Name,
                        Quantity = oi.Quantity,
                        PriceAtOrder = oi.PriceAtOrder
                    }).ToList()
                })
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }
    }
}