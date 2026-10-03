using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Handles user registration, login and logout for the RaceDay system.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        /// <summary>
        /// Registers a new RaceDay user.
        /// </summary>
        /// <remarks>
        /// Creates a new account using the supplied personal details.
        /// The role must be either Organiser or Participant.
        /// The password is hashed before it is stored in the database.
        /// </remarks>
        /// <param name="request">
        /// The user's first name, last name, email, phone number,
        /// password and selected role.
        /// </param>
        /// <returns>
        /// The newly registered user's basic account information.
        /// </returns>
        /// <response code="201">User registered successfully.</response>
        /// <response code="400">Required information or role is invalid.</response>
        /// <response code="409">The email address is already registered.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest("Required fields are missing.");
            }

            if (request.Role != "Organiser" &&
                request.Role != "Participant")
            {
                return BadRequest(
                    "Role must be Organiser or Participant.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return Conflict(
                    "A user with this email already exists.");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Created("", new
            {
                message = "Registration successful.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
        }

        /// <summary>
        /// Logs a registered user into RaceDay.
        /// </summary>
        /// <remarks>
        /// Verifies the supplied email and password. When successful,
        /// the user's ID and role are stored in the server-side session.
        /// </remarks>
        /// <param name="request">
        /// The registered user's email address and password.
        /// </param>
        /// <returns>
        /// Basic information about the authenticated user.
        /// </returns>
        /// <response code="200">Login successful.</response>
        /// <response code="401">Email or password is incorrect.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            HttpContext.Session.SetInt32(
                "UserID",
                user.UserID);

            HttpContext.Session.SetString(
                "Role",
                user.Role);

            return Ok(new
            {
                message = "Login successful.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
        }

        /// <summary>
        /// Logs the current user out of RaceDay.
        /// </summary>
        /// <remarks>
        /// Clears the current server-side session, including the stored
        /// user ID and role.
        /// </remarks>
        /// <returns>A confirmation that logout was successful.</returns>
        /// <response code="200">Logout successful.</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Ok(new
            {
                message = "Logout successful."
            });
        }
    }
}