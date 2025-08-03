using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nom complet")]
        public string FullName { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        [Display(Name = "Salaire mensuel")]
        [Precision(18, 2)]
        public decimal MonthlySalary { get; set; }

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime? HireDate { get; set; }

        public int? DepartmentId { get; set; }

        public int? JobTitleId { get; set; }
        public JobTitle? JobTitle { get; set; }

        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();

        public float? InitialRemainingDays { get; set; }
        public DateTime? InscriptionDate { get; set; }

        public byte[]? ContractFile { get; set; }
        public string? ContractFileName { get; set; }
        public string? ContractContentType { get; set; }

        public DateTime? DateOfTermination { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal BaseSalary { get; set; } = 0;

        public bool Deleted { get; set; }

        public DateOnly? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
    }


    public enum EmployeeStatus
    {
        Active,
        Inactive,
        Terminated
    }
}
