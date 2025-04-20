using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChillAndDrillApI.Model;
using ChillAndDrillApI.Controllers;

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

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDTO>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Address)
                .Include(o => o.User)
                .Select(o => new OrderResponseDTO
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    AddressId = o.AddressId,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDTO
                    {
                        Id = oi.Id,
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItem.Name,
                        Quantity = oi.Quantity,
                        PriceAtOrder = oi.PriceAtOrder
                    }).ToList(),
                    Address = new ChillAndDrillApI.Model.AddressDTO
                    {
                        Id = o.Address.Id,
                        AddressText = o.Address.AddressText
                    },
                    User = new UserDTO
                    {
                        Id = o.User.Id,
                        Login = o.User.Login,
                        FullName = o.User.FullName,
                        BirthDate = o.User.BirthDate,
                        Phone = o.User.Phone,
                        Email = o.User.Email,
                        AvatarUrl = o.User.AvatarUrl,
                        RoleId = o.User.RoleId,
                        RoleName = o.User.Role != null ? o.User.Role.Name : "Без роли",
                        CreatedAt = o.User.CreatedAt
                    }
                })
                .ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Address)
                .Include(o => o.User)
                .Select(o => new OrderResponseDTO
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    AddressId = o.AddressId,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDTO
                    {
                        Id = oi.Id,
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItem.Name,
                        Quantity = oi.Quantity,
                        PriceAtOrder = oi.PriceAtOrder
                    }).ToList(),
                    Address = new ChillAndDrillApI.Model.AddressDTO
                    {
                        Id = o.Address.Id,
                        AddressText = o.Address.AddressText
                    },
                    User = new UserDTO
                    {
                        Id = o.User.Id,
                        Login = o.User.Login,
                        FullName = o.User.FullName,
                        BirthDate = o.User.BirthDate,
                        Phone = o.User.Phone,
                        Email = o.User.Email,
                        AvatarUrl = o.User.AvatarUrl,
                        RoleId = o.User.RoleId,
                        RoleName = o.User.Role != null ? o.User.Role.Name : "Без роли",
                        CreatedAt = o.User.CreatedAt
                    }
                })
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = $"Заказ с ID {id} не найден" });
            }

            return Ok(order);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<OrderResponseDTO>> PostOrder(OrderCreateDTO orderDto)
        {
            var user = await _context.Users.FindAsync(orderDto.UserId);
            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            var address = await _context.Addresses.FindAsync(orderDto.AddressId);
            if (address == null)
            {
                return BadRequest("Адрес не найден");
            }

            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
            {
                return BadRequest("Заказ должен содержать хотя бы один товар");
            }

            foreach (var item in orderDto.OrderItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                if (menuItem == null)
                {
                    return BadRequest($"Товар с ID {item.MenuItemId} не найден");
                }
            }

            var order = new Order
            {
                UserId = orderDto.UserId,
                AddressId = orderDto.AddressId,
                TotalPrice = orderDto.TotalPrice,
                Status = orderDto.Status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                OrderItems = orderDto.OrderItems.Select(item => new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    PriceAtOrder = item.PriceAtOrder
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderResponse = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Address)
                .Include(o => o.User)
                .Select(o => new OrderResponseDTO
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    AddressId = o.AddressId,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDTO
                    {
                        Id = oi.Id,
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItem.Name,
                        Quantity = oi.Quantity,
                        PriceAtOrder = oi.PriceAtOrder
                    }).ToList(),
                    Address = new ChillAndDrillApI.Model.AddressDTO
                    {
                        Id = o.Address.Id,
                        AddressText = o.Address.AddressText
                    },
                    User = new UserDTO
                    {
                        Id = o.User.Id,
                        Login = o.User.Login,
                        FullName = o.User.FullName,
                        BirthDate = o.User.BirthDate,
                        Phone = o.User.Phone,
                        Email = o.User.Email,
                        AvatarUrl = o.User.AvatarUrl,
                        RoleId = o.User.RoleId,
                        RoleName = o.User.Role != null ? o.User.Role.Name : "Без роли",
                        CreatedAt = o.User.CreatedAt
                    }
                })
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            return CreatedAtAction("GetOrder", new { id = orderResponse.Id }, orderResponse);
        }

        // PATCH: api/Orders/5/status
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<OrderResponseDTO>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDTO statusDto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = $"Заказ с ID {id} не найден" });
            }

            if (string.IsNullOrEmpty(statusDto.Status))
            {
                return BadRequest(new { message = "Статус не может быть пустым" });
            }

            order.Status = statusDto.Status;
            order.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Ошибка при обновлении статуса заказа", error = ex.Message });
            }

            var orderResponse = new OrderResponseDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    MenuItemId = oi.MenuItemId,
                    MenuItemName = oi.MenuItem.Name,
                    Quantity = oi.Quantity,
                    PriceAtOrder = oi.PriceAtOrder
                }).ToList(),
                Address = new ChillAndDrillApI.Model.AddressDTO
                {
                    Id = order.Address.Id,
                    AddressText = order.Address.AddressText
                },
                User = new UserDTO
                {
                    Id = order.User.Id,
                    Login = order.User.Login,
                    FullName = order.User.FullName,
                    BirthDate = order.User.BirthDate,
                    Phone = order.User.Phone,
                    Email = order.User.Email,
                    AvatarUrl = order.User.AvatarUrl,
                    RoleId = order.User.RoleId,
                    RoleName = order.User.Role != null ? order.User.Role.Name : "Без роли",
                    CreatedAt = order.User.CreatedAt
                }
            };

            return Ok(orderResponse);
        }
    }

    // DTO для обновления статуса
    public class UpdateOrderStatusDTO
    {
        public string Status { get; set; } = null!;
    }
}