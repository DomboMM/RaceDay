using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public AccountController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // Display registration page
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }
        // Process registration form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var registrationData = new
            {
                model.FirstName,
                model.LastName,
                model.Email,
                model.PhoneNumber,
                model.Password,
                model.Role
            };

            try
            {
                using var response = await _apiService.PostAsync(
                    "api/auth/register", registrationData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Registration successful! Please log in.";

                    return RedirectToAction("Login");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(
                        "Email",
                        "This email address is already registered.");
                }
                else
                {
                    ModelState.AddModelError(
                        "",
                        "Registration failed. Please check your details.");
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Cannot connect to RaceDay API. Please try again.");
            }

            return View(model);
        }
        // Display login page
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }
        // Process login form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using var response = await _apiService.PostAsync(
                    "api/auth/login",
                    new
                    {
                        model.Email,
                        model.Password
                    });

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email or password.");

                    return View(model);
                }

                // Obtain the session cookie returned by the API.
                if (!response.Headers.TryGetValues(
                        "Set-Cookie", out var cookies))
                {
                    ModelState.AddModelError(
                        "",
                        "Login session could not be established.");

                    return View(model);
                }

                var sessionCookie = cookies
                    .Select(cookie => cookie.Split(';')[0].Trim())
                    .FirstOrDefault(cookie =>
                        cookie.StartsWith(
                            ".AspNetCore.Session=",
                            StringComparison.Ordinal));

                if (string.IsNullOrEmpty(sessionCookie))
                {
                    ModelState.AddModelError(
                        "",
                        "Login session could not be established.");

                    return View(model);
                }

                // Read the authenticated user details.
                using var json = System.Text.Json.JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync());

                var user = json.RootElement;

                int userId = user.GetProperty("userID").GetInt32();
                string role = user.GetProperty("role").GetString() ?? "";

                if (role != "Organiser" && role != "Participant")
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid account role.");

                    return View(model);
                }

                // Clear any previous MVC session.
                HttpContext.Session.Clear();

                // Store the authenticated session details.
                HttpContext.Session.SetInt32("UserID", userId);
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString(
                    "ApiSessionCookie", sessionCookie);

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Cannot connect to RaceDay API. Please try again.");
            }
            catch (System.Text.Json.JsonException)
            {
                ModelState.AddModelError(
                    "",
                    "The API returned an unexpected response.");
            }

            return View(model);
        }

        // Log out the current user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                using var response = await _apiService.PostAsync(
                    "api/auth/logout",
                    new { });
            }
            catch (HttpRequestException)
            {
                // Clear the MVC session even if the API is unavailable.
            }
            finally
            {
                HttpContext.Session.Clear();
            }

            return RedirectToAction("Login", "Account");
        }
    }
}