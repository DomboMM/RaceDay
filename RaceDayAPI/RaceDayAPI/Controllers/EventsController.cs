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
        // GET: api/Events
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.EventType)
                .ToListAsync();

            return Ok(events);
        }


        // ==========================================
        // GET ONE EVENT
        // GET: api/Events/5
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var raceEvent = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (raceEvent == null)
            {
                return NotFound();
            }

            return Ok(raceEvent);
        }


        // ==========================================
        // CREATE EVENT
        // POST: api/Events
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event raceEvent)
        {
            _context.Events.Add(raceEvent);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventID },
                raceEvent);
        }


        // ==========================================
        // UPDATE EVENT
        // PUT: api/Events/5
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            Event raceEvent)
        {
            if (id != raceEvent.EventID)
            {
                return BadRequest();
            }

            _context.Entry(raceEvent).State =
                EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool eventExists =
                    await _context.Events
                    .AnyAsync(e => e.EventID == id);

                if (!eventExists)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }


        // ==========================================
        // DELETE EVENT
        // DELETE: api/Events/5
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var raceEvent =
                await _context.Events.FindAsync(id);

            if (raceEvent == null)
            {
                return NotFound();
            }

            _context.Events.Remove(raceEvent);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}