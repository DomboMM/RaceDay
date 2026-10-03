using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET RESULTS
        //
        // Participant:
        // Sees only their own results.
        //
        // Organiser:
        // Sees results for their own events.
        //
        // GET: api/Results
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            if (role == "Participant")
            {
                var participantResults = await _context.Results
                    .Include(r => r.Enrolment)
                    .Where(r =>
                        r.Enrolment != null &&
                        r.Enrolment.ParticipantID == userId.Value)
                    .ToListAsync();

                return Ok(participantResults);
            }

            if (role == "Organiser")
            {
                var organiserResults = await _context.Results
                    .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                    .Where(r =>
                        r.Enrolment != null &&
                        r.Enrolment.Event != null &&
                        r.Enrolment.Event.OrganiserID == userId.Value)
                    .ToListAsync();

                return Ok(organiserResults);
            }

            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Invalid user role.");
        }

        // ==========================================
        // GET ONE RESULT
        //
        // Participant:
        // Can view only their own result.
        //
        // Organiser:
        // Can view result for their own event.
        //
        // GET: api/Results/5
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetResult(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var result = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound("Result not found.");
            }

            if (result.Enrolment == null)
            {
                return NotFound("Enrolment information not found.");
            }

            if (role == "Participant")
            {
                if (result.Enrolment.ParticipantID != userId.Value)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        "You can only view your own results.");
                }

                return Ok(result);
            }

            if (role == "Organiser")
            {
                if (result.Enrolment.Event == null ||
                    result.Enrolment.Event.OrganiserID != userId.Value)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        "You can only view results for your own events.");
                }

                return Ok(result);
            }

            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Invalid user role.");
        }

        // ==========================================
        // CREATE RESULT
        // Organiser only
        //
        // POST: api/Results
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Result>> CreateResult(Result result)
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
                    "Only Organisers can capture results.");
            }

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(
                    e => e.EnrolmentID == result.EnrolmentID);

            if (enrolment == null)
            {
                return BadRequest(
                    "The selected enrolment does not exist.");
            }

            if (enrolment.Event == null ||
                enrolment.Event.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only capture results for your own events.");
            }

            bool resultAlreadyExists = await _context.Results
                .AnyAsync(
                    r => r.EnrolmentID == result.EnrolmentID);

            if (resultAlreadyExists)
            {
                return Conflict(
                    "A result already exists for this enrolment.");
            }

            if (result.FinishingPosition <= 0)
            {
                return BadRequest(
                    "Finishing position must be greater than zero.");
            }

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultID },
                result);
        }

        // ==========================================
        // UPDATE RESULT
        // Organiser only - own events
        //
        // PUT: api/Results/5
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(
            int id,
            Result updatedResult)
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
                    "Only Organisers can update results.");
            }

            var existingResult = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (existingResult == null)
            {
                return NotFound("Result not found.");
            }

            if (existingResult.Enrolment == null ||
                existingResult.Enrolment.Event == null ||
                existingResult.Enrolment.Event.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only update results for your own events.");
            }

            if (updatedResult.FinishingPosition <= 0)
            {
                return BadRequest(
                    "Finishing position must be greater than zero.");
            }

            existingResult.FinishTime =
                updatedResult.FinishTime;

            existingResult.FinishingPosition =
                updatedResult.FinishingPosition;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // DELETE RESULT
        // Organiser only - own events
        //
        // DELETE: api/Results/5
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
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
                    "Only Organisers can delete results.");
            }

            var result = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound("Result not found.");
            }

            if (result.Enrolment == null ||
                result.Enrolment.Event == null ||
                result.Enrolment.Event.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only delete results for your own events.");
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}