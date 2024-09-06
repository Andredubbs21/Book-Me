using Microsoft.AspNetCore.Mvc;
using EventService.API.Data;
using EventService.API.Dto;
using EventService.API.Entities;
using EventService.API.Mapping;
using Microsoft.EntityFrameworkCore;

namespace EventService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventDbContext _dbContext;

        public EventController(EventDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get All Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventSummaryDto>>> GetAllEvents()
        {
            var events = await _dbContext.Events
                .Select(evt => evt.ToEventSummaryDto())
                .AsNoTracking()
                .ToListAsync();
            return Ok(events);
        }

        // Get Event by Id
        [HttpGet("{id}", Name = "GetEventById")]
        public async Task<ActionResult<EventDetailsDto>> GetEventById(int id)
        {
            var evt = await _dbContext.Events.FindAsync(id);
            if (evt is null)
                return NotFound();

            return Ok(evt.ToEventDetailsDto());
        }

        // Get Event by Name
        [HttpGet("event_name/{name}")]
        public async Task<ActionResult<EventDetailsDto>> GetEventByName(string name)
        {
            var evt = await _dbContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == name);

            if (evt is null)
                return NotFound();

            return Ok(evt.ToEventDetailsDto());
        }

        // Create Event
        [HttpPost]
        public async Task<ActionResult<EventDetailsDto>> CreateEvent(CreateEventDto newEvent)
        {
            var evt = newEvent.ToEntity();
            _dbContext.Events.Add(evt);
            await _dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetEventById", new { id = evt.Id }, evt.ToEventDetailsDto());
        }

        // Update Event by Id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, UpdateEventDto updatedEvent)
        {
            var existingEvent = await _dbContext.Events.FindAsync(id);
            if (existingEvent is null)
                return NotFound();

            _dbContext.Entry(existingEvent).CurrentValues.SetValues(updatedEvent.ToEntity(id));
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        // Delete Event by Id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventById(int id)
        {
            var existingEvent = await _dbContext.Events.FindAsync(id);
            if (existingEvent is null)
                return NotFound();

            _dbContext.Events.Remove(existingEvent);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        // Delete Event by Name
        [HttpDelete("event_name/{name}")]
        public async Task<IActionResult> DeleteEventByName(string name)
        {
            var evt = await _dbContext.Events.FirstOrDefaultAsync(e => e.Name == name);
            if (evt == null)
                return NotFound();

            _dbContext.Events.Remove(evt);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
