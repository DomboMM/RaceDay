
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

        // LOAD ORGANISER EVENTS FOR DROPDOWN
        private async Task LoadEventsAsync(
            int userId, int selectedEventId = 0)
        {
            var events = await _apiService
                .GetAsync<List<EventViewModel>>("api/Events");

            var myEvents = (events ?? new List<EventViewModel>())
                .Where(e => e.OrganiserID == userId)
                .ToList();

            ViewBag.Events = new SelectList(
                myEvents,
                "EventID",
                "Name",
                selectedEventId
            );
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
                await LoadEventsAsync(userId.Value);
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
                await LoadEventsAsync(userId.Value, model.EventID);
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

        // OPEN EDIT CATEGORY FORM
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
                var category = await _apiService
                    .GetAsync<CategoryViewModel>(
                        $"api/Categories/{id}");

                if (category == null)
                    return NotFound();

                var events = await _apiService
                    .GetAsync<List<EventViewModel>>("api/Events");

                var ownsEvent = (events ?? new List<EventViewModel>())
                    .Any(e =>
                        e.EventID == category.EventID &&
                        e.OrganiserID == userId.Value);

                if (!ownsEvent)
                    return Forbid();

                await LoadEventsAsync(
                    userId.Value, category.EventID);

                return View(category);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to load category details.";

                return RedirectToAction("Index");
            }
        }

        // SAVE EDITED CATEGORY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, CategoryViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Home");

            if (id != model.CategoryID)
                return BadRequest();

            try
            {
                var existingCategory = await _apiService
                    .GetAsync<CategoryViewModel>(
                        $"api/Categories/{id}");

                if (existingCategory == null)
                    return NotFound();

                var events = await _apiService
                    .GetAsync<List<EventViewModel>>("api/Events");

                var myEvents = (events ?? new List<EventViewModel>())
                    .Where(e => e.OrganiserID == userId.Value)
                    .ToList();

                if (!myEvents.Any(e =>
                    e.EventID == existingCategory.EventID))
                {
                    return Forbid();
                }

                if (!myEvents.Any(e =>
                    e.EventID == model.EventID))
                {
                    ModelState.AddModelError(
                        "EventID",
                        "Please select one of your own events.");
                }

                if (string.IsNullOrWhiteSpace(model.CategoryName))
                {
                    ModelState.AddModelError(
                        "CategoryName",
                        "Category name is required.");
                }

                if (string.IsNullOrWhiteSpace(model.CategoryType))
                {
                    ModelState.AddModelError(
                        "CategoryType",
                        "Category type is required.");
                }

                if (ModelState.IsValid)
                {
                    var updatedCategory = new
                    {
                        CategoryID = existingCategory.CategoryID,
                        EventID = model.EventID,
                        CategoryName = model.CategoryName,
                        CategoryType = model.CategoryType
                    };

                    using var response = await _apiService
                        .PutAsync(
                            $"api/Categories/{id}",
                            updatedCategory);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] =
                            "Category updated successfully!";

                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError(
                        "",
                        "Unable to update category.");
                }

                ViewBag.Events = new SelectList(
                    myEvents,
                    "EventID",
                    "Name",
                    model.EventID
                );

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to connect to RaceDay API."
                );

                ViewBag.Events = new SelectList(
                    new List<EventViewModel>(),
                    "EventID",
                    "Name"
                );

                return View(model);
            }
        }
    }
}
