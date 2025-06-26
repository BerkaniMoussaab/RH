using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class Payroll
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime PayDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BaseSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Bonus { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Deductions { get; set; }

        public decimal NetPay => BaseSalary + Bonus - Deductions;
        public ICollection<PayrollAppliedRule> AppliedRules { get; set; } = new List<PayrollAppliedRule>();
        public int AbsenceDays { get; set; } // how many days the employee was absent
        public decimal AbsenceDeduction { get; set; } // the amount deducted due to absences

        public decimal DeductionPerAbsenceDay { get; set; }
        public int ManualAbsenceDays { get; set; }

    }

}
