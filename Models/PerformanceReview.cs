using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class PerformanceReview
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime ReviewDate { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comments { get; set; }

        public string? Reviewer { get; set; }
    }

}
