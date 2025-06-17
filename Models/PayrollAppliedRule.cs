namespace RH.Models
{
    public class PayrollAppliedRule
    {
        public int Id { get; set; }

        public int PayrollId { get; set; }
        public Payroll Payroll { get; set; }

        public int RuleId { get; set; }
        public PayrollAdjustmentRule Rule { get; set; }
    }

}
