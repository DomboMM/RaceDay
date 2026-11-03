
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class ResultsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public ResultsController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }


     

        // DISPLAY ORGANISER ENROLMENTS AND RESULTS
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
                // GET ORGANISER ENROLMENTS
                var enrolments = await _apiService
                    .GetAsync<List<EnrolmentViewModel>>(
                        "api/Enrolments");

                // GET RECORDED RESULTS
                var results = await _apiService
                    .GetAsync<List<ResultViewModel>>(
                        "api/Results");

                // PASS RESULTS TO THE VIEW
                ViewBag.RecordedResults =
                    results ?? new List<ResultViewModel>();

                return View(
                    enrolments ?? new List<EnrolmentViewModel>());
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to load enrolments or results from the RaceDay API.";

                ViewBag.RecordedResults =
                    new List<ResultViewModel>();

                return View(new List<EnrolmentViewModel>());
            }
        }


        // DISPLAY RECORD RESULT FORM
        [HttpGet]
        public async Task<IActionResult> Create(int enrolmentId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Home");

            try
            {
                var enrolments = await _apiService
                    .GetAsync<List<EnrolmentViewModel>>(
                        "api/Enrolments");

                var selectedEnrolment = enrolments?
                    .FirstOrDefault(e =>
                        e.EnrolmentID == enrolmentId);

                if (selectedEnrolment == null)
                    return NotFound();

                var model = new CreateResultViewModel
                {
                    EnrolmentID = selectedEnrolment.EnrolmentID
                };

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to load the selected enrolment.";

                return RedirectToAction(nameof(Index));
            }
        }

        // SAVE A PARTICIPANT'S RACE RESULT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateResultViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Home");

            if (model.FinishTime == null ||
                model.FinishTime.Value <= TimeSpan.Zero)
            {
                ModelState.AddModelError(
                    nameof(model.FinishTime),
                    "Enter a valid finishing time greater than zero.");
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var resultData = new
                {
                    model.EnrolmentID,
                    FinishTime = model.FinishTime.Value,
                    model.FinishingPosition
                };

                using var response = await _apiService.PostAsync(
                    "api/Results", resultData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Race result recorded successfully.";

                    return RedirectToAction(nameof(Index));
                }

                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Conflict =>
                            "A result already exists for this enrolment.",

                        System.Net.HttpStatusCode.Forbidden =>
                            "You cannot record results for this enrolment.",

                        System.Net.HttpStatusCode.BadRequest =>
                            "Invalid result details. Please check your entries.",

                        _ => "Unable to save the result. " + errorMessage
                    });
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Cannot connect to the RaceDay API.");
            }

            return View(model);
        }



    }
}
