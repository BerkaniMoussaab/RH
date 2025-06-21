using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class JobTitle
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int BaseSalary { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<PayrollAdjustmentRule> PayrollAdjustmentRules { get; set; } = new List<PayrollAdjustmentRule>();
        public ICollection<LeavePolicy> LeavePolicies { get; set; } = new List<LeavePolicy>();

    }


}
