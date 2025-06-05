using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class MonthlyAttendanceSummary
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int Month { get; set; }

        public int DaysAbsent { get; set; }
        public int LateArrivals { get; set; }

        // Optional: auto-calculated total deduction
        public decimal TotalDeduction { get; set; }
        public void CalculateDeductionSmart()
        {
            if (Employee == null || Employee.MonthlySalary <= 0)
            {
                TotalDeduction = 0;
                return;
            }

            var dailySalary = Employee.MonthlySalary / 30m;

            var absenceDeduction = DaysAbsent * dailySalary;

            int chargeableLateArrivals = Math.Max(0, LateArrivals - 3); // Grace period
            var lateDeduction = chargeableLateArrivals * (0.1m * dailySalary); // 10% penalty

            var rawTotal = absenceDeduction + lateDeduction;

            var maxDeduction = Employee.MonthlySalary * 0.3m; // Cap at 30%
            TotalDeduction = Math.Min(rawTotal, maxDeduction);
        }


    }


}
