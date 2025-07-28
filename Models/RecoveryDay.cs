using RH.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class RecoveryDay
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime Date { get; set; }
    public string? Reason { get; set; }
    public float Quantity { get; set; } = 1f;

    public bool Used { get; set; } = false; // Keep this temporarily
    public float UsedQuantity { get; set; } = 0f;

    [NotMapped]
    public bool FullyUsed => UsedQuantity >= Quantity;
}
