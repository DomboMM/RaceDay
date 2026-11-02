
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public EventsController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // DISPLAY AVAILABLE EVENTS FOR PARTICIPANTS
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Participant")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var events = await _apiService
                    .GetAsync<List<EventViewModel>>("api/Events");

                var availableEvents = (events ?? new List<EventViewModel>())
                    .Where(e => e.EventDate >= DateTime.Today)
                    .OrderBy(e => e.EventDate)
                    .ToList();

                return View(availableEvents);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to load events from RaceDay API.";

                return View(new List<EventViewModel>());
            }
        }
    }
}
