using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Handles event enrolments for RaceDay Participants and Organisers.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets event enrolments available to the logged-in user.
        /// </summary>
        /// <remarks>
        /// Participants receive only their own enrolments.
        /// Organisers receive enrolments belonging to their own events.
        /// </remarks>
        /// <returns>A list of enrolments accessible to the current user.</returns>
        /// <response code="200">Enrolments retrieved successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The user has an invalid role.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolments()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            if (role == "Organiser")
            {
                var organiserEnrolments = await _context.Enrolments
                    .Include(e => e.Participant)
                    .Include(e => e.Event)
                    .Include(e => e.Category)
                    .Where(e => e.Event != null &&
                                e.Event.OrganiserID == userId.Value)
                    .ToListAsync();

                return Ok(organiserEnrolments);
            }

            if (role == "Participant")
            {
                var participantEnrolments = await _context.Enrolments
                    .Include(e => e.Event)
                    .Include(e => e.Category)
                    .Where(e => e.ParticipantID == userId.Value)
                    .ToListAsync();

                return Ok(participantEnrolments);
            }

            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Invalid user role.");
        }

        /// <summary>
        /// Gets a specific event enrolment.
        /// </summary>
        /// <remarks>
        /// A Participant can only view their own enrolment.
        /// An Organiser can only view an enrolment belonging to
        /// one of their own events.
        /// </remarks>
        /// <param name="id">The ID of the enrolment to retrieve.</param>
        /// <returns>The requested enrolment.</returns>
        /// <response code="200">Enrolment retrieved successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The user is not allowed to view the enrolment.</response>
        /// <response code="404">The enrolment could not be found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Enrolment>> GetEnrolment(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null)
            {
                return NotFound("Enrolment not found.");
            }

            if (role == "Participant")
            {
                if (enrolment.ParticipantID != userId.Value)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        "You can only view your own enrolments.");
                }

                return Ok(enrolment);
            }

            if (role == "Organiser")
            {
                if (enrolment.Event == null ||
                    enrolment.Event.OrganiserID != userId.Value)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        "You can only view enrolments for your own events.");
                }

                return Ok(enrolment);
            }

            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Invalid user role.");
        }

        /// <summary>
        /// Enrols a Participant in an event.
        /// </summary>
        /// <remarks>
        /// Only authenticated Participants can enrol.
        /// The Participant ID is obtained from the server-side session.
        /// The selected category must belong to the selected event,
        /// and duplicate enrolment in the same event is prevented.
        /// </remarks>
        /// <param name="enrolment">
        /// The selected event and category information.
        /// </param>
        /// <returns>The newly created enrolment.</returns>
        /// <response code="201">Participant enrolled successfully.</response>
        /// <response code="400">The event or category is invalid.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">Only Participants can enrol in events.</response>
        /// <response code="409">The Participant is already enrolled in the event.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Enrolment>> CreateEnrolment(
            Enrolment enrolment)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            if (role != "Participant")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Only Participants can enrol in events.");
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(
                    e => e.EventID == enrolment.EventID);

            if (raceEvent == null)
            {
                return BadRequest(
                    "The selected event does not exist.");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(
                    c => c.CategoryID == enrolment.CategoryID &&
                         c.EventID == enrolment.EventID);

            if (category == null)
            {
                return BadRequest(
                    "The selected category does not belong to this event.");
            }

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == userId.Value &&
                    e.EventID == enrolment.EventID);

            if (alreadyEnrolled)
            {
                return Conflict(
                    "You are already enrolled in this event.");
            }

            enrolment.ParticipantID = userId.Value;
            enrolment.EnrolmentDate = DateTime.UtcNow;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentID },
                enrolment);
        }

        /// <summary>
        /// Cancels a Participant's event enrolment.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Participant can cancel an enrolment,
        /// and the Participant can only cancel their own enrolment.
        /// </remarks>
        /// <param name="id">The ID of the enrolment to cancel.</param>
        /// <response code="204">Enrolment cancelled successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The user cannot cancel this enrolment.</response>
        /// <response code="404">The enrolment could not be found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            if (role != "Participant")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Only Participants can cancel enrolments.");
            }

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null)
            {
                return NotFound("Enrolment not found.");
            }

            if (enrolment.ParticipantID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only cancel your own enrolment.");
            }

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}