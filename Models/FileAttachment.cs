using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RH.Models
{
    public class FileAttachment
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public byte[] Content { get; set; } = Array.Empty<byte>();

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public string? Description { get; set; }

        // Navigation property
        public Employee Employee { get; set; } = null!;
    }
}
