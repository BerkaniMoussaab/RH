using RH.Models;

public class LeaveRequestData
{
    public LeaveRequest Leave { get; set; } = default!;
    public CompanyInfo CompanyInfo { get; set; } = default!;
    public string? CompanyLogoDataUrl { get; set; }
    public string? CompanyCity { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.Today;

    public string FormattedStartDate => Leave.StartDate.ToString("dd/MM/yyyy");
    public string FormattedEndDate => Leave.EndDate.ToString("dd/MM/yyyy");
    public string FormattedRequestDate => RequestDate.ToString("dd/MM/yyyy");
    public int TotalDays => Leave.TotalDays;
}
