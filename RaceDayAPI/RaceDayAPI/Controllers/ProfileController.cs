using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Handles profile operations for authenticated RaceDay users.
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the currently logged-in user's profile.
        /// </summary>
        /// <remarks>
        /// Both Organisers and Participants can use this endpoint.
        /// The user ID is obtained from the server-side session, so a user
        /// can only retrieve their own profile.
        /// </remarks>
        /// <returns>
        /// The authenticated user's profile information, including their
        /// name, email address, phone number and role.
        /// </returns>
        /// <response code="200">Profile retrieved successfully.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="404">The user's profile could not be found.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound("User profile not found.");
            }

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Role
            });
        }

        /// <summary>
        /// Updates the currently logged-in user's profile.
        /// </summary>
        /// <remarks>
        /// Both Organisers and Participants can update their own profile.
        /// The user's role and password cannot be changed through this
        /// endpoint.
        /// </remarks>
        /// <param name="request">
        /// The updated first name, last name, email address and phone number.
        /// </param>
        /// <returns>
        /// The updated profile information and a confirmation message.
        /// </returns>
        /// <response code="200">Profile updated successfully.</response>
        /// <response code="400">Required profile information is missing.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="404">The user's profile could not be found.</response>
        /// <response code="409">The supplied email is already used by another user.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateProfile(
            UpdateProfileDto request)
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound("User profile not found.");
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(
                    "First name, last name and email are required.");
            }

            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email == request.Email &&
                    u.UserID != userId);

            if (emailExists)
            {
                return Conflict(
                    "A user with this email already exists.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Role
            });
        }
    }
}