// ChillAndDrillApI/Controllers/AddressesController.cs
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
    public class AddressesController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public AddressesController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // GET: api/Addresses?userId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddressResponseDTO>>> GetAddresses(int? userId)
        {
            var query = _context.Addresses.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value);
            }

            var addresses = await query
                .Select(a => new AddressResponseDTO
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    AddressText = a.AddressText,
                    IsDefault = a.IsDefault
                })
                .ToListAsync();

            return addresses;
        }

        // GET: api/Addresses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AddressResponseDTO>> GetAddress(int id)
        {
            var address = await _context.Addresses
                .Select(a => new AddressResponseDTO
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    AddressText = a.AddressText,
                    IsDefault = a.IsDefault
                })
                .FirstOrDefaultAsync(a => a.Id == id);

            if (address == null)
            {
                return NotFound();
            }

            return address;
        }

        // POST: api/Addresses
        [HttpPost]
        public async Task<ActionResult<AddressResponseDTO>> PostAddress(AddressCreateDTO addressDto)
        {
            // Проверяем существование пользователя
            var user = await _context.Users.FindAsync(addressDto.UserId);
            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            // Если адрес помечен как IsDefault, сбрасываем IsDefault у других адресов
            if (addressDto.IsDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == addressDto.UserId && (a.IsDefault ?? true))
                    .ToListAsync();
                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                }
            }

            // Создаём адрес
            var address = new Address
            {
                UserId = addressDto.UserId,
                AddressText = addressDto.AddressText,
                IsDefault = addressDto.IsDefault
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // Формируем ответ
            var addressResponse = new AddressResponseDTO
            {
                Id = address.Id,
                UserId = address.UserId,
                AddressText = address.AddressText,
                IsDefault = address.IsDefault
            };

            return CreatedAtAction("GetAddress", new { id = addressResponse.Id }, addressResponse);
        }

        // PUT: api/Addresses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAddress(int id, AddressCreateDTO addressDto)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            // Проверяем существование пользователя
            var user = await _context.Users.FindAsync(addressDto.UserId);
            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            address.UserId = addressDto.UserId;
            address.AddressText = addressDto.AddressText;
            address.IsDefault = addressDto.IsDefault;

            // Если адрес становится IsDefault, сбрасываем у других
            if (addressDto.IsDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == addressDto.UserId && (a.IsDefault ?? true) && a.Id != id)
                    .ToListAsync();
                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                }
            }

            _context.Entry(address).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AddressExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Addresses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AddressExists(int id)
        {
            return _context.Addresses.Any(e => e.Id == id);
        }
    }
}