using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        // 👇 ADD THIS
        [Range(0, double.MaxValue)]
        public decimal MonthlySalary { get; set; }

        
        public string Email { get; set; } = string.Empty;

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
        public DateTime? InscriptionDate { get; set; }   // date employee was added to system

    }
    public enum EmployeeStatus
    {
        Active,
        Inactive,
        Terminated
    }



}
