
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public ParticipantController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // DISPLAY EVENTS ENROLLED IN BY PARTICIPANT
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            try
            {
                var enrolments = await _apiService
                    .GetAsync<List<EnrolmentViewModel>>(
                        "api/Enrolments");

                var myEnrolments =
                    (enrolments ?? new List<EnrolmentViewModel>())
                    .Where(e => e.ParticipantID == userId.Value)
                    .OrderBy(e => e.Event?.EventDate)
                    .ToList();

                return View(myEnrolments);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to load your enrolled events from RaceDay API.";

                return View(new List<EnrolmentViewModel>());
            }
        }

        // DISPLAY PARTICIPANT RACE RESULTS
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            try
            {
                var results = await _apiService
                    .GetAsync<List<ResultViewModel>>(
                        "api/Results");

                var myResults =
                    (results ?? new List<ResultViewModel>())
                    .Where(r =>
                        r.Enrolment != null &&
                        r.Enrolment.ParticipantID == userId.Value)
                    .OrderBy(r => r.FinishingPosition)
                    .ToList();

                return View(myResults);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to load your race results from RaceDay API.";

                return View(new List<ResultViewModel>());
            }
        }
    }
}
