using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class JobTitle
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }


}
