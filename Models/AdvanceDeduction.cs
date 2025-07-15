namespace RH.Models
{
    public class AdvanceDeduction
    {
        public int Id { get; set; }
        public int AdvanceId { get; set; }
        public Advance Advance { get; set; }
        public int PayrollId { get; set; }
        public Payroll Payroll { get; set; }
        public decimal DeductedAmount { get; set; }
        public DateTime DeductionDate { get; set; }
    }
}
