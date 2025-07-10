namespace RH.Models
{
    public class WorkedDayOff
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime Date { get; set; }

        public string? Reason { get; set; }

        public bool GrantsRecoveryDay { get; set; }

        public bool GrantsBonus { get; set; }

        public bool ConvertedToRecovery { get; set; } = false;

        public bool BonusPaid { get; set; } = false;
        public float Quantity { get; set; } = 1f;

    }

}
