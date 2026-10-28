
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public CategoriesController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // DISPLAY ORGANISER CATEGORIES
        [HttpGet]
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

                var myEventIds = (events ?? new List<EventViewModel>())
                    .Where(e => e.OrganiserID == userId.Value)
                    .Select(e => e.EventID)
                    .ToHashSet();

                var categories = await _apiService
                    .GetAsync<List<CategoryViewModel>>("api/Categories");

                var myCategories =
                    (categories ?? new List<CategoryViewModel>())
                    .Where(c => myEventIds.Contains(c.EventID))
                    .ToList();

                return View(myCategories);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to load categories from RaceDay API.";

                return View(new List<CategoryViewModel>());
            }
        }

        // OPEN CREATE CATEGORY FORM
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
                var events = await _apiService
                    .GetAsync<List<EventViewModel>>("api/Events");

                var myEvents = (events ?? new List<EventViewModel>())
                    .Where(e => e.OrganiserID == userId.Value)
                    .ToList();

                ViewBag.Events = new SelectList(
                    myEvents,
                    "EventID",
                    "Name"
                );
            }
            catch (HttpRequestException)
            {
                ViewBag.Events = new SelectList(
                    new List<EventViewModel>(),
                    "EventID",
                    "Name"
                );

                ModelState.AddModelError(
                    "",
                    "Unable to load your events."
                );
            }

            return View(new CreateCategoryViewModel());
        }

        // SAVE NEW CATEGORY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateCategoryViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                var newCategory = new
                {
                    EventID = model.EventID,
                    CategoryName = model.CategoryName,
                    CategoryType = model.CategoryType
                };

                try
                {
                    using var response = await _apiService
                        .PostAsync("api/Categories", newCategory);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] =
                            "Category created successfully!";

                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError(
                        "",
                        "Unable to create category. Please check your details."
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
                var events = await _apiService
                    .GetAsync<List<EventViewModel>>("api/Events");

                var myEvents = (events ?? new List<EventViewModel>())
                    .Where(e => e.OrganiserID == userId.Value)
                    .ToList();

                ViewBag.Events = new SelectList(
                    myEvents,
                    "EventID",
                    "Name",
                    model.EventID
                );
            }
            catch (HttpRequestException)
            {
                ViewBag.Events = new SelectList(
                    new List<EventViewModel>(),
                    "EventID",
                    "Name"
                );
            }

            return View(model);
        }
    }
}
