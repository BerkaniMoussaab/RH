using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class CompanyInfo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse est requise.")]
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro RC est requis.")]
        [MaxLength(50)]
        public string RC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le NIF est requis.")]
        [MaxLength(50)]
        public string NIF { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string DirectorName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string DirectorTitle { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Numéro de mobile invalide.")]
        [MaxLength(20)]
        public string Mobile { get; set; } = string.Empty;

        [MaxLength(50)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Numéro de téléphone invalide.")]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public byte[]? LogoBytes { get; set; }

        public string? LogoMimeType { get; set; } // e.g. "image/png"

        [Range(0, 10, ErrorMessage = "Valeur invalide pour jours de récupération.")]
        public float RecoveryDaysPerWorkedDayOff { get; set; } = 1f;

        [Range(0, 31, ErrorMessage = "Jours travaillés par mois invalide.")]
        public int WorKdaysInMonth { get; set; } = 26; // corrected name and default value
    }
}
