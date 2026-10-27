using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = string.Empty;

        [Range(0.1, 1000, ErrorMessage = "Enter a valid distance.")]
        public decimal Distance { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select an event type.")]
        public int EventTypeID { get; set; }
    }
}