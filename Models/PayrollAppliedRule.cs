namespace RH.Models
{
    public class PayrollAppliedRule
    {
        public int Id { get; set; }

        public int PayrollId { get; set; }
        public Payroll Payroll { get; set; }

        public int RuleId { get; set; }
        public PayrollAdjustmentRule Rule { get; set; }

        // ✅ Optional: For countable rules (e.g., 5 lates)
        public decimal? Quantity { get; set; }

        // ✅ Final calculated amount (based on quantity and rule)
        public decimal Amount { get; set; }

        // ✅ Optional: Comment (e.g., reason or explanation)
        public string? Notes { get; set; }

    }
}
