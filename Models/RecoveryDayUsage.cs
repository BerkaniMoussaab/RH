using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class RecoveryDayUsage
    {
        public int Id { get; set; }
        
        public int RecoveryDayId { get; set; }
        public RecoveryDay? RecoveryDay { get; set; }
        
        public DateTime UsageDate { get; set; }
        
        [Range(0.1, float.MaxValue, ErrorMessage = "La quantité utilisée doit être supérieure à 0")]
        public float QuantityUsed { get; set; }
        
        public string? Reason { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

