using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "La raison de l'avance est obligatoire.")]
        [MaxLength(500, ErrorMessage = "La raison ne peut pas dépasser 500 caractères.")]
        public string Reason { get; set; } = string.Empty;

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
