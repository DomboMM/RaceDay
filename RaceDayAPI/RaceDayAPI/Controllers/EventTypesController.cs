
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTypesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventTypesController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEventTypes()
        {
            var eventTypes = await _context.EventTypes
                .AsNoTracking()
                .Select(e => new
                {
                    e.EventTypeID,
                    e.TypeName,
                    e.Description
                })
                .ToListAsync();

            return Ok(eventTypes);
        }
    }
}
