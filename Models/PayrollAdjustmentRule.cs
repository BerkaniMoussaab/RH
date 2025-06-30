using RH.Models;
using System.ComponentModel.DataAnnotations;

public class PayrollAdjustmentRule
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public AdjustmentType Type { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    public bool IsPercentage { get; set; }
    public bool IsCountable { get; set; }
    public bool IsEditable { get; set; } // ✅ NEW in DB
    public ICollection<JobTitle> JobTitles { get; set; } = new List<JobTitle>();
}


public enum AdjustmentType
{
    Bonus,
    Deduction
}
