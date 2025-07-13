namespace RH.Models
{
    public class CompanyInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string RC { get; set; }
        public string NIF { get; set; }

        public byte[]? LogoBytes { get; set; }
        public string? LogoMimeType { get; set; } // e.g. "image/png"
        public float RecoveryDaysPerWorkedDayOff { get; set; } = 1f;
        public int WorKdaysInMonth { get; set; }
    }
}
