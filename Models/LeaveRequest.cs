using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public string? Reason { get; set; }
    }
    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
