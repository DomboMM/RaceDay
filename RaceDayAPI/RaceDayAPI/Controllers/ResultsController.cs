using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Handles race results for RaceDay events.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets results available to the logged-in user.
        /// </summary>
        /// <remarks>
        /// Participants can only view their own results.
        /// Organisers can view results belonging to their own events.
        /// </remarks>
        /// <returns>A list of results accessible to the current user.</returns>
        /// <response code="200">Results retrieved successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The user has an invalid role.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Gets a specific race result.
        /// </summary>
        /// <remarks>
        /// A Participant can only view their own result.
        /// An Organiser can only view a result belonging to
        /// one of their own events.
        /// </remarks>
        /// <param name="id">The ID of the result to retrieve.</param>
        /// <returns>The requested race result.</returns>
        /// <response code="200">Result retrieved successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The user cannot access this result.</response>
        /// <response code="404">The result could not be found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Captures a Participant's race result.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Organiser can capture results.
        /// The enrolment must belong to one of the Organiser's own
        /// events. Only one result can exist for each enrolment.
        /// </remarks>
        /// <param name="result">
        /// The enrolment ID, finish time and finishing position.
        /// </param>
        /// <returns>The newly created result.</returns>
        /// <response code="201">Result captured successfully.</response>
        /// <response code="400">The enrolment or result data is invalid.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The Organiser cannot manage this event.</response>
        /// <response code="409">A result already exists for the enrolment.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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

        /// <summary>
        /// Updates an existing race result.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Organiser can update results,
        /// and the result must belong to one of their own events.
        /// </remarks>
        /// <param name="id">The ID of the result to update.</param>
        /// <param name="updatedResult">
        /// The updated finish time and finishing position.
        /// </param>
        /// <response code="204">Result updated successfully.</response>
        /// <response code="400">The finishing position is invalid.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The user cannot update this result.</response>
        /// <response code="404">The result could not be found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Deletes an existing race result.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Organiser can delete results,
        /// and the result must belong to one of their own events.
        /// </remarks>
        /// <param name="id">The ID of the result to delete.</param>
        /// <response code="204">Result deleted successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The user cannot delete this result.</response>
        /// <response code="404">The result could not be found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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