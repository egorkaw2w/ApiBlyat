// ChillAndDrillApI/Controllers/EventsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChillAndDrillApI.Model;
using Microsoft.Extensions.Logging;

namespace ChillAndDrillApI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ChillAndDrillContext _context;
        private readonly ILogger<EventsController> _logger;

        public EventsController(ChillAndDrillContext context, ILogger<EventsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEvents()
        {
            _logger.LogInformation("Fetching events");
            try
            {
                var events = await _context.Events
                    .Select(e => new EventDTO
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        ImageUrl = e.ImageUrl
                    })
                    .ToListAsync();
                _logger.LogInformation("Fetched {Count} events", events.Count);
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching events");
                throw;
            }
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDTO>> GetEvent(int id)
        {
            _logger.LogInformation("Fetching event with id {Id}", id);
            try
            {
                var eventItem = await _context.Events
                    .Select(e => new EventDTO
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        ImageUrl = e.ImageUrl,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    })
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (eventItem == null)
                {
                    _logger.LogWarning("Event with id {Id} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Fetched event with id {Id}", id);
                return eventItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching event with id {Id}", id);
                throw;
            }
        }

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<EventDTO>> PostEvent([FromBody] EventCreateDTO eventDTO)
        {
            _logger.LogInformation("Creating new event with title {Title}", eventDTO.Title);
            try
            {
                var eventItem = new Event
                {
                    Title = eventDTO.Title,
                    Description = eventDTO.Description,
                    ImageUrl = eventDTO.ImageUrl,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Events.Add(eventItem);
                await _context.SaveChangesAsync();

                var resultDTO = new EventDTO
                {
                    Id = eventItem.Id,
                    Title = eventItem.Title,
                    Description = eventItem.Description,
                    ImageUrl = eventItem.ImageUrl,
                    CreatedAt = eventItem.CreatedAt,
                    UpdatedAt = eventItem.UpdatedAt
                };

                _logger.LogInformation("Created event with id {Id}", eventItem.Id);
                return CreatedAtAction(nameof(GetEvent), new { id = eventItem.Id }, resultDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                throw;
            }
        }

        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromBody] EventCreateDTO eventDTO)
        {
            _logger.LogInformation("Updating event with id {Id}", id);
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    _logger.LogWarning("Event with id {Id} not found", id);
                    return NotFound();
                }

                eventItem.Title = eventDTO.Title;
                eventItem.Description = eventDTO.Description;
                eventItem.ImageUrl = eventDTO.ImageUrl;
                eventItem.UpdatedAt = DateTime.Now;

                _context.Entry(eventItem).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Updated event with id {Id}", id);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!_context.Events.Any(e => e.Id == id))
                    {
                        _logger.LogWarning("Event with id {Id} no longer exists", id);
                        return NotFound();
                    }
                    _logger.LogError(ex, "Concurrency error updating event with id {Id}", id);
                    throw;
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event with id {Id}", id);
                throw;
            }
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            _logger.LogInformation("Deleting event with id {Id}", id);
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    _logger.LogWarning("Event with id {Id} not found", id);
                    return NotFound();
                }

                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted event with id {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event with id {Id}", id);
                throw;
            }
        }
    }

    public class EventDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class EventCreateDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }
}