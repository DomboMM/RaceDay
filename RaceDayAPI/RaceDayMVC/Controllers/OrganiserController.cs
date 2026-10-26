using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public OrganiserController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            // Check whether the user is logged in.
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Only organisers can access this dashboard.
            if (role != "Organiser")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Retrieve events from RaceDayAPI.
                var events = await _apiService
                    .GetAsync<List<EventViewModel>>("api/Events");

                // Display only events belonging to this organiser.
                var myEvents = (events ?? new List<EventViewModel>())
                    .Where(e => e.OrganiserID == userId.Value)
                    .ToList();

                return View(myEvents);
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