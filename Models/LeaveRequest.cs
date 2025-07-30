using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RH.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "L'employé est requis.")]
        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Required(ErrorMessage = "La date de début est requise.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La date de fin est requise.")]
        [DataType(DataType.Date)]
        [DateGreaterThan("StartDate", ErrorMessage = "La date de fin doit être postérieure à la date de début.")]
        public DateTime EndDate { get; set; }
        [NotMapped]
        public int TotalDays => (EndDate - StartDate).Days + 1;
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        [MaxLength(500, ErrorMessage = "La raison ne peut pas dépasser 500 caractères.")]
        public string? Reason { get; set; }
        public bool IsPaid { get; set; } = true;

        
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected
    }

    // Custom validation attribute
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = (DateTime?)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
                throw new ArgumentException("Propriété de comparaison non trouvée.");

            var comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);

            if (currentValue.HasValue && comparisonValue.HasValue && currentValue <= comparisonValue)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
