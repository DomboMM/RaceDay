using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Handles category viewing and management for RaceDay events.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all available event categories.
        /// </summary>
        /// <remarks>
        /// Both authenticated Organisers and Participants can view
        /// the available categories.
        /// </remarks>
        /// <returns>A list of all event categories.</returns>
        /// <response code="200">Categories retrieved successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Gets a specific event category.
        /// </summary>
        /// <remarks>
        /// Both authenticated Organisers and Participants can view
        /// the details of a specific category.
        /// </remarks>
        /// <param name="id">The ID of the category to retrieve.</param>
        /// <returns>The requested category and its related event.</returns>
        /// <response code="200">Category retrieved successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="404">The category could not be found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Creates a category for an event.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Organiser can create a category.
        /// The Organiser can only create categories for events that
        /// belong to their own account.
        /// </remarks>
        /// <param name="category">
        /// The category information, including the event ID,
        /// category name and category type.
        /// </param>
        /// <returns>The newly created category.</returns>
        /// <response code="201">Category created successfully.</response>
        /// <response code="400">The selected event does not exist.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">
        /// The user is not an Organiser or does not own the event.
        /// </response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(
                    e => e.EventID == category.EventID);

            if (raceEvent == null)
            {
                return BadRequest(
                    "The selected event does not exist.");
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

        /// <summary>
        /// Updates an existing event category.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Organiser can update a category.
        /// The category must belong to one of the Organiser's own events.
        /// A category cannot be moved to another Organiser's event.
        /// </remarks>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="updatedCategory">
        /// The updated event ID, category name and category type.
        /// </param>
        /// <response code="204">Category updated successfully.</response>
        /// <response code="400">The selected event does not exist.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">
        /// The user is not an Organiser or does not own the event.
        /// </response>
        /// <response code="404">The category could not be found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            var targetEvent = await _context.Events
                .FirstOrDefaultAsync(
                    e => e.EventID == updatedCategory.EventID);

            if (targetEvent == null)
            {
                return BadRequest(
                    "The selected event does not exist.");
            }

            if (targetEvent.OrganiserID != userId.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can only use your own events.");
            }

            existingCategory.EventID =
                updatedCategory.EventID;

            existingCategory.CategoryName =
                updatedCategory.CategoryName;

            existingCategory.CategoryType =
                updatedCategory.CategoryType;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deletes an event category.
        /// </summary>
        /// <remarks>
        /// Only an authenticated Organiser can delete a category,
        /// and the category must belong to one of their own events.
        /// </remarks>
        /// <param name="id">The ID of the category to delete.</param>
        /// <response code="204">Category deleted successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">
        /// The user is not an Organiser or does not own the event.
        /// </response>
        /// <response code="404">The category could not be found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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