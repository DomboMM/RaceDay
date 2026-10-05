using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Handles RaceDay event viewing and management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all RaceDay events.
        /// </summary>
        /// <remarks>
        /// Both authenticated Organisers and Participants can view
        /// the available events.
        /// </remarks>
        /// <returns>A list of all available events.</returns>
        /// <response code="200">Events retrieved successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Gets a specific RaceDay event.
        /// </summary>
        /// <remarks>
        /// Both authenticated Organisers and Participants can view
        /// the details of a specific event.
        /// </remarks>
        /// <param name="id">The ID of the event to retrieve.</param>
        /// <returns>The requested event and its event type.</returns>
        /// <response code="200">Event retrieved successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="404">The event could not be found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Creates a new RaceDay event.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Organiser can create an event.
        /// The Organiser ID is taken from the current session rather
        /// than trusted from the request body.
        /// </remarks>
        /// <param name="raceEvent">
        /// The event information, including event type, name,
        /// description, date, location and distance.
        /// </param>
        /// <returns>The newly created event.</returns>
        /// <response code="201">Event created successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The logged-in user is not an Organiser.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

            raceEvent.OrganiserID = userId.Value;

            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventID },
                raceEvent);
        }

        /// <summary>
        /// Updates an existing RaceDay event.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Organiser can update an event,
        /// and the Organiser can only update an event that belongs
        /// to their own account.
        /// </remarks>
        /// <param name="id">The ID of the event to update.</param>
        /// <param name="updatedEvent">
        /// The updated event type, name, description, date,
        /// location and distance.
        /// </param>
        /// <response code="204">Event updated successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">
        /// The user is not an Organiser or does not own the event.
        /// </response>
        /// <response code="404">The event could not be found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Deletes a RaceDay event.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Organiser can delete an event,
        /// and the Organiser can only delete their own event.
        /// </remarks>
        /// <param name="id">The ID of the event to delete.</param>
        /// <response code="204">Event deleted successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">
        /// The user is not an Organiser or does not own the event.
        /// </response>
        /// <response code="404">The event could not be found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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