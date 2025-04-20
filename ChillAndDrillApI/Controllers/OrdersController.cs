using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ChillAndDrillApI.Model;

namespace ChillAndDrillApI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ChillAndDrillContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDTO>>> GetOrders()
        {
            _logger.LogInformation("Fetching all orders");
            try
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
                        User = new ChillAndDrillApI.Model.UserDTO
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all orders");
                throw;
            }
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetOrder(int id)
        {
            _logger.LogInformation("Fetching order with ID {Id}", id);
            try
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
                        User = new ChillAndDrillApI.Model.UserDTO
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
                    _logger.LogWarning("Order with ID {Id} not found", id);
                    return NotFound(new { message = $"Заказ с ID {id} не найден" });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order with ID {Id}", id);
                throw;
            }
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<OrderResponseDTO>> PostOrder(OrderCreateDTO orderDto)
        {
            _logger.LogInformation("Creating new order for user {UserId}", orderDto.UserId);
            try
            {
                var user = await _context.Users.FindAsync(orderDto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", orderDto.UserId);
                    return BadRequest("Пользователь не найден");
                }

                var address = await _context.Addresses.FindAsync(orderDto.AddressId);
                if (address == null)
                {
                    _logger.LogWarning("Address with ID {AddressId} not found", orderDto.AddressId);
                    return BadRequest("Адрес не найден");
                }

                if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
                {
                    _logger.LogWarning("Order items are empty for user {UserId}", orderDto.UserId);
                    return BadRequest("Заказ должен содержать хотя бы один товар");
                }

                foreach (var item in orderDto.OrderItems)
                {
                    var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                    if (menuItem == null)
                    {
                        _logger.LogWarning("Menu item with ID {MenuItemId} not found", item.MenuItemId);
                        return BadRequest($"Товар с ID {item.MenuItemId} не найден");
                    }
                }

                var order = new Order
                {
                    UserId = orderDto.UserId,
                    AddressId = orderDto.AddressId,
                    TotalPrice = orderDto.OrderItems.Sum(item => item.Quantity * item.PriceAtOrder),
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
                        User = new ChillAndDrillApI.Model.UserDTO
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", orderDto.UserId);
                throw;
            }
        }

        // PATCH: api/Orders/5/status
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<OrderResponseDTO>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDTO statusDto)
        {
            _logger.LogInformation("Updating status for order {Id}", id);
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .Include(o => o.Address)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {Id} not found", id);
                    return NotFound(new { message = $"Заказ с ID {id} не найден" });
                }

                if (string.IsNullOrEmpty(statusDto.Status))
                {
                    _logger.LogWarning("Status is empty for order {Id}", id);
                    return BadRequest(new { message = "Статус не может быть пустым" });
                }

                order.Status = statusDto.Status;
                order.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

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
                    User = new ChillAndDrillApI.Model.UserDTO
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {Id}", id);
                throw;
            }
        }
    }

    // DTO для обновления статуса
    public class UpdateOrderStatusDTO
    {
        public string Status { get; set; } = null!;
    }
}