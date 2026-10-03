using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ENROLMENTS
        //
        // Organiser:
        // Sees enrolments for their own events.
        //
        // Participant:
        // Sees their own enrolments.
        //
        // GET: api/Enrolments
        // ==========================================
        [HttpGet]
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

        // ==========================================
        // GET ONE ENROLMENT
        //
        // Participant can view their own enrolment.
        // Organiser can view enrolments for their
        // own events.
        //
        // GET: api/Enrolments/5
        // ==========================================
        [HttpGet("{id}")]
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

        // ==========================================
        // CREATE ENROLMENT
        // Participant only
        //
        // POST: api/Enrolments
        // ==========================================
        [HttpPost]
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

            // Make sure the event exists.
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(
                    e => e.EventID == enrolment.EventID);

            if (raceEvent == null)
            {
                return BadRequest("The selected event does not exist.");
            }

            // Make sure the category exists and belongs
            // to the selected event.
            var category = await _context.Categories
                .FirstOrDefaultAsync(
                    c => c.CategoryID == enrolment.CategoryID &&
                         c.EventID == enrolment.EventID);

            if (category == null)
            {
                return BadRequest(
                    "The selected category does not belong to this event.");
            }

            // Prevent duplicate enrolment in the same event.
            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == userId.Value &&
                    e.EventID == enrolment.EventID);

            if (alreadyEnrolled)
            {
                return Conflict(
                    "You are already enrolled in this event.");
            }

            // Never trust ParticipantID from the request.
            // Use the logged-in Participant instead.
            enrolment.ParticipantID = userId.Value;
            enrolment.EnrolmentDate = DateTime.UtcNow;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentID },
                enrolment);
        }

        // ==========================================
        // DELETE ENROLMENT
        //
        // Participant can cancel their own enrolment.
        //
        // DELETE: api/Enrolments/5
        // ==========================================
        [HttpDelete("{id}")]
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