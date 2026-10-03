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
        // GET ALL RESULTS
        // GET: api/Results
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                .ToListAsync();

            return Ok(results);
        }

        // ==========================================
        // GET ONE RESULT
        // GET: api/Results/5
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetResult(int id)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // ==========================================
        // CREATE RESULT
        // POST: api/Results
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Result>> CreateResult(Result result)
        {
            _context.Results.Add(result);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultID },
                result);
        }

        // ==========================================
        // UPDATE RESULT
        // PUT: api/Results/5
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(
            int id,
            Result result)
        {
            if (id != result.ResultID)
            {
                return BadRequest();
            }

            _context.Entry(result).State =
                EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool resultExists =
                    await _context.Results
                    .AnyAsync(r => r.ResultID == id);

                if (!resultExists)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // ==========================================
        // DELETE RESULT
        // DELETE: api/Results/5
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result =
                await _context.Results.FindAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            _context.Results.Remove(result);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}