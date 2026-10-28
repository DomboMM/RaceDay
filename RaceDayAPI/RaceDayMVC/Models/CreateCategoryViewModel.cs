
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CreateCategoryViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select an event.")]
        public int EventID { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category type is required.")]
        [StringLength(100)]
        public string CategoryType { get; set; } = string.Empty;
    }
}
