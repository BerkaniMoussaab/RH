using RH.Models;

namespace RH.Models
{
    public class AttestationData
    {
        public AttestationType Type { get; set; }
        public Employee Employee { get; set; } = null!;
        public CompanyInfo CompanyInfo { get; set; } = null!;
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        
        // For internship attestations
        public DateTime? InternshipStartDate { get; set; }
        public DateTime? InternshipEndDate { get; set; }
        public string? InternshipDepartment { get; set; }
        public string? SupervisorName { get; set; }
        public string? InternshipObjectives { get; set; }
        
        // Computed properties for display
        public string FormattedGeneratedDate => GeneratedDate.ToString("dd/MM/yyyy");
        public string FormattedBirthDate => Employee.BirthDate?.ToString("dd/MM/yyyy") ?? "Date inconnue";
        public string FormattedHireDate => Employee.HireDate?.ToString("dd/MM/yyyy") ?? "Date inconnue";
        public string FormattedInternshipStartDate => InternshipStartDate?.ToString("dd/MM/yyyy") ?? "";
        public string FormattedInternshipEndDate => InternshipEndDate?.ToString("dd/MM/yyyy") ?? "";
        public string BirthPlace => Employee.BirthPlace ?? CompanyInfo.City ?? "Ville inconnue";
        public string JobTitle => Employee.JobTitle?.Title ?? "Fonction inconnue";
        public string City => CompanyInfo.City ?? Employee.BirthPlace ?? "AKBOU";
        
        // Company logo as base64 data URL
        public string? CompanyLogoDataUrl 
        { 
            get 
            {
                if (CompanyInfo.LogoBytes != null && !string.IsNullOrEmpty(CompanyInfo.LogoMimeType))
                {
                    var base64 = Convert.ToBase64String(CompanyInfo.LogoBytes);
                    return $"data:{CompanyInfo.LogoMimeType};base64,{base64}";
                }
                return null;
            }
        }
    }

    public enum AttestationType
    {
        Work,
        Internship
    }
}

