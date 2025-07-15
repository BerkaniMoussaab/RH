namespace RH.Models
{
    public class Advance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Reason { get; set; }
        public AdvanceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<AdvanceDeduction> Deductions { get; set; }
    }

    public enum AdvanceStatus
    {
        Active,
        Completed,
        Cancelled
    }
}
