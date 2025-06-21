namespace RH.Models
{
    public class LeavePolicy
    {
        public int Id { get; set; }
        public int AnnualLeaveDays { get; set; } = 30;
        public ICollection<JobTitle> JobTitles { get; set; } = new List<JobTitle>();
    }
}
