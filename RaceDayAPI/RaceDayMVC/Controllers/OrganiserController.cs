using Microsoft.AspNetCore.Mvc.Rendering;
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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Home");

            try
            {
                var eventTypes = await _apiService
                    .GetAsync<List<EventTypeViewModel>>("api/EventTypes");

                ViewBag.EventTypes = new SelectList(
                    eventTypes ?? new List<EventTypeViewModel>(),
                    "EventTypeID",
                    "TypeName"
                );
            }
            catch (HttpRequestException)
            {
                ViewBag.EventTypes = new SelectList(
                    new List<EventTypeViewModel>(),
                    "EventTypeID",
                    "TypeName"
                );

                ModelState.AddModelError(
                    "",
                    "Unable to load event types from RaceDay API."
                );
            }

            return View(new CreateEventViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                var newEvent = new
                {
                    OrganiserID = userId.Value,
                    EventTypeID = model.EventTypeID,
                    Name = model.Name,
                    Description = model.Description,
                    EventDate = model.EventDate,
                    Location = model.Location,
                    Distance = model.Distance
                };

                try
                {
                    using var response = await _apiService
                        .PostAsync("api/Events", newEvent);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] =
                            "Event created successfully!";

                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError(
                        "",
                        "Unable to create event. Please check your details."
                    );
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(
                        "",
                        "Cannot connect to RaceDay API."
                    );
                }
            }

            try
            {
                var eventTypes = await _apiService
                    .GetAsync<List<EventTypeViewModel>>("api/EventTypes");

                ViewBag.EventTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    eventTypes ?? new List<EventTypeViewModel>(),
                    "EventTypeID",
                    "TypeName",
                    model.EventTypeID
                );
            }
            catch (HttpRequestException)
            {
                ViewBag.EventTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    new List<EventTypeViewModel>(),
                    "EventTypeID",
                    "TypeName"
                );
            }

            return View(model);
        }

    }
}