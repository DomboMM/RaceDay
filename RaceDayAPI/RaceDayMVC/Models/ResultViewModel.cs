
namespace RaceDayMVC.Models
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }

        public int EnrolmentID { get; set; }

        public TimeSpan FinishTime { get; set; }

        public int FinishingPosition { get; set; }

        public EnrolmentViewModel? Enrolment { get; set; }
    }
}
