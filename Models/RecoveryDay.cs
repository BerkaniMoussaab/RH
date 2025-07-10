using System;

namespace RH.Models
{
    public class RecoveryDay
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime Date { get; set; }

        public string? Reason { get; set; }

        public bool Used { get; set; } = false;
        public float Quantity { get; set; } = 1f;

    }
}
