using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class AbsenceRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500, ErrorMessage = "La raison ne peut pas dépasser 500 caractères.")]
        public string? Reason { get; set; }
        public bool Counted { get; set; }
        
    }
}
