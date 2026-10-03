using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL CATEGORIES
        // Organiser and Participant
        // GET: api/Categories
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var categories = await _context.Categories
                .Include(c => c.Event)
                .ToListAsync();

            return Ok(categories);
        }

        // ==========================================
        // GET ONE CATEGORY
        // Organiser and Participant
        // GET: api/Categories/5
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            return Ok(category);
        }

        // ==========================================
        // CREATE CATEGORY
        // Organiser only
        // POST: api/Categories
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(
            Category category)
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
                    "Only Organisers can create categories.");
            }

            // Check that the event exists and belongs
            // to the logged-in organiser.
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(
                    e => e.EventID == category.EventID);

            if (raceEvent == null)
            {
                return BadRequest("The selected event does not exist.");
            }

            if (raceEvent.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only create categories for your own events.");
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                category);
        }

        // ==========================================
        // UPDATE CATEGORY
        // Organiser only - own events
        // PUT: api/Categories/5
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            Category updatedCategory)
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
                    "Only Organisers can update categories.");
            }

            var existingCategory = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (existingCategory == null)
            {
                return NotFound("Category not found.");
            }

            if (existingCategory.Event == null ||
                existingCategory.Event.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only update categories for your own events.");
            }

            // Prevent moving a category to another
            // organiser's event.
            var targetEvent = await _context.Events
                .FirstOrDefaultAsync(
                    e => e.EventID == updatedCategory.EventID);

            if (targetEvent == null)
            {
                return BadRequest("The selected event does not exist.");
            }

            if (targetEvent.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only use your own events.");
            }

            existingCategory.EventID = updatedCategory.EventID;
            existingCategory.CategoryName = updatedCategory.CategoryName;
            existingCategory.CategoryType = updatedCategory.CategoryType;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // DELETE CATEGORY
        // Organiser only - own events
        // DELETE: api/Categories/5
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
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
                    "Only Organisers can delete categories.");
            }

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            if (category.Event == null ||
                category.Event.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only delete categories from your own events.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}