
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CreateEnrolmentViewModel
    {
        [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid event.")]
        public int EventID { get; set; }

        [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a category.")]
        public int CategoryID { get; set; }

        public string EventName { get; set; } = string.Empty;
    }
}
