using RH.Models;
using System.ComponentModel.DataAnnotations;

public class PayrollAdjustmentRule
{
    public int Id { get; set; }

    [Required]
    public int JobTitleId { get; set; }
    public JobTitle? JobTitle { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public AdjustmentType Type { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    public bool IsPercentage { get; set; }
}

public enum AdjustmentType
{
    Bonus,
    Deduction
}
