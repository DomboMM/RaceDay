
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public ProfileController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // DISPLAY THE LOGGED-IN USER'S PROFILE
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var profile = await _apiService
                    .GetAsync<ProfileViewModel>("api/profile");

                if (profile == null)
                {
                    ViewBag.ErrorMessage =
                        "Your profile could not be found.";

                    return View(new ProfileViewModel());
                }

                return View(profile);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to load your profile from the RaceDay API.";

                return View(new ProfileViewModel());
            }
        }

        // DISPLAY EDIT PROFILE FORM
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var profile = await _apiService
                    .GetAsync<ProfileViewModel>("api/profile");

                if (profile == null)
                {
                    TempData["ErrorMessage"] =
                        "Your profile could not be found.";

                    return RedirectToAction(nameof(Index));
                }

                return View(profile);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to load your profile.";

                return RedirectToAction(nameof(Index));
            }
        }

        // SAVE UPDATED PROFILE INFORMATION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Role is not editable, so do not validate
            // the role supplied by the form.
            ModelState.Remove(nameof(ProfileViewModel.Role));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var updateData = new
                {
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.PhoneNumber
                };

                using var response = await _apiService.PutAsync(
                    "api/profile", updateData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Profile updated successfully.";

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(
                    "",
                    response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Conflict =>
                            "This email address is already registered.",

                        System.Net.HttpStatusCode.BadRequest =>
                            "Please check your profile information.",

                        _ => "Unable to update your profile."
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
