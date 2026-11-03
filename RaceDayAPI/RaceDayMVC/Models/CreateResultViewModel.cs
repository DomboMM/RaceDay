
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CreateResultViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select an enrolment.")]
        public int EnrolmentID { get; set; }

        [Required(ErrorMessage = "Finishing time is required.")]
        public TimeSpan? FinishTime { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Finishing position must be greater than zero.")]
        public int FinishingPosition { get; set; }
    }
}
