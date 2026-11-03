
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class ProfileViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}
