
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class EnrolmentsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public EnrolmentsController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // DISPLAY PARTICIPANT ENROLMENT FORM
        [HttpGet]
        public async Task<IActionResult> Create(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            try
            {
                var raceEvent = await _apiService
                    .GetAsync<EventViewModel>(
                        $"api/Events/{eventId}");

                if (raceEvent == null)
                    return NotFound();

                if (raceEvent.EventDate.Date < DateTime.Today)
                {
                    TempData["ErrorMessage"] =
                        "This event has already taken place.";

                    return RedirectToAction("Index", "Events");
                }

                var categories = await _apiService
                    .GetAsync<List<CategoryViewModel>>(
                        "api/Categories");

                var eventCategories =
                    (categories ?? new List<CategoryViewModel>())
                    .Where(c => c.EventID == eventId)
                    .ToList();

                ViewBag.Categories = new SelectList(
                    eventCategories,
                    "CategoryID",
                    "CategoryName");

                var model = new CreateEnrolmentViewModel
                {
                    EventID = raceEvent.EventID,
                    EventName = raceEvent.Name
                };

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to load event categories.";

                return RedirectToAction("Index", "Events");
            }
        }

        // SUBMIT PARTICIPANT ENROLMENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateEnrolmentViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            try
            {
                var raceEvent = await _apiService
                    .GetAsync<EventViewModel>(
                        $"api/Events/{model.EventID}");

                if (raceEvent == null)
                    return NotFound();

                model.EventName = raceEvent.Name;

                if (raceEvent.EventDate.Date < DateTime.Today)
                {
                    ModelState.AddModelError(
                        "",
                        "This event has already taken place.");
                }

                var categories = await _apiService
                    .GetAsync<List<CategoryViewModel>>(
                        "api/Categories");

                var eventCategories =
                    (categories ?? new List<CategoryViewModel>())
                    .Where(c => c.EventID == model.EventID)
                    .ToList();

                ViewBag.Categories = new SelectList(
                    eventCategories,
                    "CategoryID",
                    "CategoryName",
                    model.CategoryID);

                if (!eventCategories.Any(
                    c => c.CategoryID == model.CategoryID))
                {
                    ModelState.AddModelError(
                        "CategoryID",
                        "Please select a valid category.");
                }

                if (ModelState.IsValid)
                {
                    var enrolment = new
                    {
                        EventID = model.EventID,
                        CategoryID = model.CategoryID
                    };

                    using var response = await _apiService
                        .PostAsync("api/Enrolments", enrolment);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] =
                            "You have enrolled successfully!";

                        return RedirectToAction("Index", "Events");
                    }

                    if (response.StatusCode ==
                        System.Net.HttpStatusCode.Conflict)
                    {
                        ModelState.AddModelError(
                            "",
                            "You are already enrolled in this event.");
                    }
                    else
                    {
                        ModelState.AddModelError(
                            "",
                            "Unable to complete enrolment.");
                    }
                }

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to RaceDay API.";

                return RedirectToAction("Index", "Events");
            }
        }
    }
}
