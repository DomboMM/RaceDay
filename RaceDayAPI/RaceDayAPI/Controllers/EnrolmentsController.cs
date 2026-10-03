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
        // GET ALL ENROLMENTS
        // GET: api/Enrolments
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolments()
        {
            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .ToListAsync();

            return Ok(enrolments);
        }

        // ==========================================
        // GET ONE ENROLMENT
        // GET: api/Enrolments/5
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Enrolment>> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null)
            {
                return NotFound();
            }

            return Ok(enrolment);
        }

        // ==========================================
        // CREATE ENROLMENT
        // POST: api/Enrolments
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Enrolment>> CreateEnrolment(
            Enrolment enrolment)
        {
            _context.Enrolments.Add(enrolment);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentID },
                enrolment);
        }

        // ==========================================
        // UPDATE ENROLMENT
        // PUT: api/Enrolments/5
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrolment(
            int id,
            Enrolment enrolment)
        {
            if (id != enrolment.EnrolmentID)
            {
                return BadRequest();
            }

            _context.Entry(enrolment).State =
                EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool enrolmentExists =
                    await _context.Enrolments
                    .AnyAsync(e => e.EnrolmentID == id);

                if (!enrolmentExists)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // ==========================================
        // DELETE ENROLMENT
        // DELETE: api/Enrolments/5
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var enrolment =
                await _context.Enrolments.FindAsync(id);

            if (enrolment == null)
            {
                return NotFound();
            }

            _context.Enrolments.Remove(enrolment);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}