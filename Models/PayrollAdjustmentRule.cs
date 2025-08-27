using Microsoft.EntityFrameworkCore;
using RH.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PayrollAdjustmentRule
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public AdjustmentType Type { get; set; }

    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,6)")]
    public decimal Amount { get; set; }

    public bool IsPercentage { get; set; }

    public bool IsCountable { get; set; }

    public bool IsEditable { get; set; }

   
    public bool AutoApplied { get; set; }

    public ICollection<JobTitle> JobTitles { get; set; } = new List<JobTitle>();
}

public enum AdjustmentType
{
    Bonus,
    Deduction
}
