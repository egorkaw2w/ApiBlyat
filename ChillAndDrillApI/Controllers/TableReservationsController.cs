// ChillAndDrillApI/Controllers/TableReservationsController.cs
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
    public class TableReservationsController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;

        public TableReservationsController(ChillAndDrillContext context)
        {
            _context = context;
        }

        // GET: api/TableReservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TableReservationDTO>>> GetTableReservations()
        {
            return await _context.TableReservations
                .Include(tr => tr.Table)
                .Include(tr => tr.User)
                .Select(tr => new TableReservationDTO
                {
                    Id = tr.Id,
                    TableId = tr.TableId,
                    TableName = tr.Table.Name,
                    UserId = tr.UserId,
                    // Предполагаем, что время в базе хранится как UTC
                    ReservationTime = DateTime.SpecifyKind(tr.ReservationTime, DateTimeKind.Utc),
                    DurationMinutes = tr.DurationMinutes,
                    Comment = tr.Comment,
                    CreatedAt = tr.CreatedAt.HasValue ? DateTime.SpecifyKind(tr.CreatedAt.Value, DateTimeKind.Utc) : null
                })
                .ToListAsync();
        }

        // GET: api/TableReservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TableReservationDTO>> GetTableReservation(int id)
        {
            var tableReservation = await _context.TableReservations
                .Include(tr => tr.Table)
                .Include(tr => tr.User)
                .Select(tr => new TableReservationDTO
                {
                    Id = tr.Id,
                    TableId = tr.TableId,
                    TableName = tr.Table.Name,
                    UserId = tr.UserId,
                    ReservationTime = DateTime.SpecifyKind(tr.ReservationTime, DateTimeKind.Utc),
                    DurationMinutes = tr.DurationMinutes,
                    Comment = tr.Comment,
                    CreatedAt = tr.CreatedAt.HasValue ? DateTime.SpecifyKind(tr.CreatedAt.Value, DateTimeKind.Utc) : null
                })
                .FirstOrDefaultAsync(tr => tr.Id == id);

            if (tableReservation == null)
            {
                return NotFound();
            }

            return tableReservation;
        }

        // POST: api/TableReservations
        [HttpPost]
        public async Task<ActionResult<TableReservationDTO>> PostTableReservation(TableReservationCreate reservationDTO)
        {


            Console.WriteLine($"Received reservationTime: {reservationDTO.ReservationTime} (Kind: {reservationDTO.ReservationTime.Kind})");

            TableReservationCreate.reservationTime = DateTime.Now;

            var table = await _context.Tables.FindAsync(reservationDTO.TableId);
            
            if (table == null)
            {
                return BadRequest(new { errors = new { TableId = new[] { "Table with the specified TableId does not exist." } } });
            }

            // Проверяем, существует ли пользователь (если UserId указан)
            if (reservationDTO.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(reservationDTO.UserId.Value);
                if (user == null)
                {
                    return BadRequest(new { errors = new { UserId = new[] { "User with the specified UserId does not exist." } } });
                }
            }

            // Проверяем, что длительность бронирования разумная
            if (reservationDTO.DurationMinutes <= 0 || reservationDTO.DurationMinutes > 240)
            {
                return BadRequest(new { errors = new { DurationMinutes = new[] { "DurationMinutes must be between 30 and 240 minutes." } } });
            }

            // Приводим дату к UTC, если она не в UTC
            var reservationTimeUtc = reservationDTO.ReservationTime.Kind == DateTimeKind.Utc
                ? reservationDTO.ReservationTime
                : reservationDTO.ReservationTime.ToUniversalTime();
            Console.WriteLine($"Converted to UTC: {reservationTimeUtc}");

            // Для хранения в базе данных преобразуем в Kind=Unspecified
            var reservationTimeForDb = DateTime.SpecifyKind(reservationTimeUtc, DateTimeKind.Unspecified);
            var currentTimeForDb = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            Console.WriteLine($"Current time for DB: {currentTimeForDb}");

            // Проверяем корректность времени бронирования
            if (reservationTimeForDb < currentTimeForDb)
            {
                return BadRequest(new { errors = new { ReservationTime = new[] { "Reservation time cannot be in the past." } } });
            }

            // Проверяем, нет ли пересечений с другими бронированиями
            var reservationEndTime = reservationTimeForDb.AddMinutes(reservationDTO.DurationMinutes);
            try
            {
                var conflictingReservations = await _context.TableReservations
                    .Where(tr => tr.TableId == reservationDTO.TableId &&
                                 tr.ReservationTime < reservationEndTime &&
                                 reservationTimeForDb < tr.ReservationTime.AddMinutes(tr.DurationMinutes))
                    .ToListAsync();

                Console.WriteLine($"Found {conflictingReservations.Count} conflicting reservations:");
                foreach (var res in conflictingReservations)
                {
                    Console.WriteLine($"  Reservation: Start={res.ReservationTime}, Duration={res.DurationMinutes}");
                }

                if (conflictingReservations.Any())
                {
                    return BadRequest(new { errors = new { General = new[] { "The table is already booked for the specified time range." } } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new { General = new[] { $"Error checking conflicts: {ex.Message}" } } });
            }

            // Создаём бронирование
            var reservation = new TableReservation
            {
                TableId = reservationDTO.TableId,
                UserId = reservationDTO.UserId,
                ReservationTime = reservationTimeForDb, // Сохраняем как Unspecified
                DurationMinutes = reservationDTO.DurationMinutes,
                Comment = reservationDTO.Comment,
                CreatedAt = currentTimeForDb // Сохраняем как Unspecified
            };

            _context.TableReservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Формируем DTO для ответа
            var result = new TableReservationDTO
            {
                Id = reservation.Id,
                TableId = reservation.TableId,
                TableName = table.Name,
                UserId = reservation.UserId,
                ReservationTime = DateTime.SpecifyKind(reservation.ReservationTime, DateTimeKind.Utc), // Возвращаем как UTC
                DurationMinutes = reservation.DurationMinutes,
                Comment = reservation.Comment,
                CreatedAt = reservation.CreatedAt.HasValue ? DateTime.SpecifyKind(reservation.CreatedAt.Value, DateTimeKind.Utc) : null
            };

            return CreatedAtAction(nameof(GetTableReservation), new { id = reservation.Id }, result);
        }

        // PUT: api/TableReservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTableReservation(int id, TableReservationUpdateDTO reservationDTO)
        {
            var reservation = await _context.TableReservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // Проверяем, существует ли стол
            var table = await _context.Tables.FindAsync(reservationDTO.TableId);
            if (table == null)
            {
                return BadRequest(new { errors = new { TableId = new[] { "Table with the specified TableId does not exist." } } });
            }

            // Проверяем, существует ли пользователь (если UserId указан)
            if (reservationDTO.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(reservationDTO.UserId.Value);
                if (user == null)
                {
                    return BadRequest(new { errors = new { UserId = new[] { "User with the specified UserId does not exist." } } });
                }
            }

            // Проверяем, что длительность бронирования разумная
            if (reservationDTO.DurationMinutes <= 0 || reservationDTO.DurationMinutes > 240)
            {
                return BadRequest(new { errors = new { DurationMinutes = new[] { "DurationMinutes must be between 30 and 240 minutes." } } });
            }

            // Приводим дату к UTC
            var reservationTimeUtc = reservationDTO.ReservationTime.Kind == DateTimeKind.Utc
                ? reservationDTO.ReservationTime
                : reservationDTO.ReservationTime.ToUniversalTime();

            // Для хранения в базе данных преобразуем в Kind=Unspecified
            var reservationTimeForDb = DateTime.SpecifyKind(reservationTimeUtc, DateTimeKind.Unspecified);

            // Проверяем, нет ли пересечений с другими бронированиями (кроме текущей)
            var reservationEndTime = reservationTimeForDb.AddMinutes(reservationDTO.DurationMinutes);
            try
            {
                var conflictingReservations = await _context.TableReservations
                    .Where(tr => tr.Id != id && tr.TableId == reservationDTO.TableId &&
                                 tr.ReservationTime < reservationEndTime &&
                                 reservationTimeForDb < tr.ReservationTime.AddMinutes(tr.DurationMinutes))
                    .ToListAsync();

                Console.WriteLine($"Found {conflictingReservations.Count} conflicting reservations during update:");
                foreach (var res in conflictingReservations)
                {
                    Console.WriteLine($"  Reservation: Start={res.ReservationTime}, Duration={res.DurationMinutes}");
                }

                if (conflictingReservations.Any())
                {
                    return BadRequest(new { errors = new { General = new[] { "The table is already booked for the specified time range." } } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new { General = new[] { $"Error checking conflicts: {ex.Message}" } } });
            }

            // Обновляем бронирование
            reservation.TableId = reservationDTO.TableId;
            reservation.UserId = reservationDTO.UserId;
            reservation.ReservationTime = reservationTimeForDb;
            reservation.DurationMinutes = reservationDTO.DurationMinutes;
            reservation.Comment = reservationDTO.Comment;
            // CreatedAt оставляем без изменений

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TableReservationExists(id))
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

        // DELETE: api/TableReservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTableReservation(int id)
        {
            var tableReservation = await _context.TableReservations.FindAsync(id);
            if (tableReservation == null)
            {
                return NotFound();
            }

            _context.TableReservations.Remove(tableReservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TableReservationExists(int id)
        {
            return _context.TableReservations.Any(e => e.Id == id);
        }
    }

    public class TableReservationDTO
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public string TableName { get; set; } = null!;
        public int? UserId { get; set; }
        public DateTime ReservationTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class TableReservationCreate
    {
        internal static DateTime reservationTime;

        public int TableId { get; set; }
        public int? UserId { get; set; }
        public DateTime ReservationTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Comment { get; set; }
    }

    public class TableReservationUpdateDTO
    {
        public int TableId { get; set; }
        public int? UserId { get; set; }
        public DateTime ReservationTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Comment { get; set; }
    }
}