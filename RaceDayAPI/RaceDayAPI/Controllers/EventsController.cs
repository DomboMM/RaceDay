using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL EVENTS
        // Both Organisers and Participants
        // GET: api/Events
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var events = await _context.Events
                .Include(e => e.EventType)
                .ToListAsync();

            return Ok(events);
        }

        // ==========================================
        // GET ONE EVENT
        // Both Organisers and Participants
        // GET: api/Events/5
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var raceEvent = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (raceEvent == null)
            {
                return NotFound("Event not found.");
            }

            return Ok(raceEvent);
        }

        // ==========================================
        // CREATE EVENT
        // Organiser only
        // POST: api/Events
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event raceEvent)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            if (role != "Organiser")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Only Organisers can create events.");
            }

            // The organiser is taken from the logged-in session.
            raceEvent.OrganiserID = userId.Value;

            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventID },
                raceEvent);
        }

        // ==========================================
        // UPDATE EVENT
        // Organiser only - own events
        // PUT: api/Events/5
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            Event updatedEvent)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            if (role != "Organiser")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Only Organisers can update events.");
            }

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            if (existingEvent.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only update your own events.");
            }

            existingEvent.EventTypeID = updatedEvent.EventTypeID;
            existingEvent.Name = updatedEvent.Name;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Location = updatedEvent.Location;
            existingEvent.Distance = updatedEvent.Distance;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // DELETE EVENT
        // Organiser only - own events
        // DELETE: api/Events/5
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            if (role != "Organiser")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Only Organisers can delete events.");
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (raceEvent == null)
            {
                return NotFound("Event not found.");
            }

            if (raceEvent.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only delete your own events.");
            }

            _context.Events.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}