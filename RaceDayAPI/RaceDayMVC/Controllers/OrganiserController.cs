
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // ORGANISER DASHBOARD
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Home");

            try
            {
                var events = await _apiService
                    .GetAsync<List<EventViewModel>>("api/Events");

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

        // OPEN CREATE EVENT FORM
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

        // SAVE NEW EVENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateEventViewModel model)
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

                ViewBag.EventTypes = new SelectList(
                    eventTypes ?? new List<EventTypeViewModel>(),
                    "EventTypeID",
                    "TypeName",
                    model.EventTypeID
                );
            }
            catch (HttpRequestException)
            {
                ViewBag.EventTypes = new SelectList(
                    new List<EventTypeViewModel>(),
                    "EventTypeID",
                    "TypeName"
                );
            }

            return View(model);
        }

        // OPEN EDIT EVENT FORM
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Home");

            try
            {
                var raceEvent = await _apiService
                    .GetAsync<EventViewModel>(
                        $"api/Events/{id}");

                if (raceEvent == null)
                    return NotFound();

                if (raceEvent.OrganiserID != userId.Value)
                    return Forbid();

                var eventTypes = await _apiService
                    .GetAsync<List<EventTypeViewModel>>(
                        "api/EventTypes");

                ViewBag.EventTypes = new SelectList(
                    eventTypes ?? new List<EventTypeViewModel>(),
                    "EventTypeID",
                    "TypeName",
                    raceEvent.EventTypeID
                );

                return View(raceEvent);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to load event details from RaceDay API.";

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Home");

            if (id != model.EventID)
                return BadRequest();

            try
            {
                // Retrieve the original event from the API.
                var existingEvent = await _apiService
                    .GetAsync<EventViewModel>($"api/Events/{id}");

                if (existingEvent == null)
                    return NotFound();

                // Only the owner can edit this event.
                if (existingEvent.OrganiserID != userId.Value)
                    return Forbid();

                if (ModelState.IsValid)
                {
                    var updatedEvent = new
                    {
                        EventID = existingEvent.EventID,
                        OrganiserID = existingEvent.OrganiserID,
                        EventTypeID = model.EventTypeID,
                        Name = model.Name,
                        Description = model.Description,
                        EventDate = model.EventDate,
                        Location = model.Location,
                        Distance = model.Distance
                    };

                    using var response = await _apiService
                        .PutAsync($"api/Events/{id}", updatedEvent);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] =
                            "Event updated successfully!";

                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError(
                        "",
                        "Unable to update the event. Please check your details."
                    );
                }

                // Reload event types if validation fails.
                var eventTypes = await _apiService
                    .GetAsync<List<EventTypeViewModel>>("api/EventTypes");

                ViewBag.EventTypes = new SelectList(
                    eventTypes ?? new List<EventTypeViewModel>(),
                    "EventTypeID",
                    "TypeName",
                    model.EventTypeID
                );

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to connect to RaceDay API."
                );

                ViewBag.EventTypes = new SelectList(
                    new List<EventTypeViewModel>(),
                    "EventTypeID",
                    "TypeName"
                );

                return View(model);
            }
        }

    }
}
