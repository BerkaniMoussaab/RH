using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace RH.Models
{
    public class PayrollAppliedRule
    {
        public int Id { get; set; }

        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int? PayrollId { get; set; }
        public Payroll Payroll { get; set; }

        public int RuleId { get; set; }
        public PayrollAdjustmentRule Rule { get; set; }

        // ✅ Quantity with explicit precision
        [Precision(18, 4)]
        public decimal? Quantity { get; set; }

        // ✅ Final calculated amount with precision
        [Precision(18, 4)]
        public decimal Amount { get; set; }

        public string? Notes { get; set; }
        public DateTime? Date { get; set; }
    }
}
