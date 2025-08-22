using RH.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using RH.Data;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;

namespace RH.Services
{
    public class AttestationGeneratorService
    {
        private readonly ApplicationDbContext _context;

        public AttestationGeneratorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateWorkAttestationPdfAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(e => e.Id == employeeId)
                ?? throw new ArgumentException("Employé non trouvé.");

            var companyInfo = await _context.CompanyInfos.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Les informations de l'entreprise ne sont pas configurées.");

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            document.SetFont(normalFont);

            AddCompanyHeader(document, companyInfo, boldFont, normalFont);

            if (employee.Status == EmployeeStatus.Terminated)
            {
                document.Add(new Paragraph("Certificat de Travail")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetFont(boldFont)
                    .SetMarginBottom(20));
            }
            else
            {
                document.Add(new Paragraph("Attestation de Travail")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetFont(boldFont)
                    .SetMarginBottom(20));
            }

            var content = new Paragraph()
                .SetFontSize(12)
                .Add($"Nous soussignés, {companyInfo.Name}, certifions que M./Mme ")
                .Add(new Text(employee.FullName).SetFont(boldFont))
                .Add($" a été employé(e) dans notre entreprise en qualité de ")
                .Add(new Text(employee.JobTitle?.Title ?? "N/A").SetFont(boldFont))
                .Add($", à partir du ")
                .Add(new Text(employee.HireDate?.ToString("dd/MM/yyyy") ?? "Date inconnue").SetFont(boldFont))
                .Add(".");

            if (employee.Status == EmployeeStatus.Terminated)
            {
                content.Add($" Son contrat a pris fin le {DateTime.Now:dd/MM/yyyy}.");
            }

            document.Add(content.SetTextAlignment(TextAlignment.JUSTIFIED).SetMarginBottom(20));

            document.Add(new Paragraph("Cette attestation est délivrée à la demande de l'intéressé(e) pour servir et valoir ce que de droit.")
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFontSize(12)
                .SetMarginBottom(40));

            document.Add(new Paragraph($"Fait à {companyInfo.Address ?? "Alger"}, le {DateTime.Now:dd/MM/yyyy}")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(11));

            document.Add(new Paragraph("Signature et cachet de l'entreprise")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(11)
                .SetMarginTop(40));

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GenerateInternshipAttestationPdfAsync(int employeeId, DateTime internshipStartDate, DateTime internshipEndDate, string internshipDepartment, string supervisorName, string internshipObjectives)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId)
                ?? throw new ArgumentException("Employé non trouvé.");

            var companyInfo = await _context.CompanyInfos.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Les informations de l'entreprise ne sont pas configurées.");

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            document.SetFont(normalFont);

            AddCompanyHeader(document, companyInfo, boldFont, normalFont);

            document.Add(new Paragraph("Attestation de Stage")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetFont(boldFont)
                .SetMarginBottom(20));

            var content = new Paragraph()
                .SetFontSize(12)
                .Add($"Nous soussignés, {companyInfo.Name}, certifions que M./Mme ")
                .Add(new Text(employee.FullName).SetFont(boldFont))
                .Add(" a effectué un stage au sein de notre entreprise. ")
                .Add($"Ce stage s’est déroulé du {internshipStartDate:dd/MM/yyyy} au {internshipEndDate:dd/MM/yyyy}, ")
                .Add($"au sein du département ")
                .Add(new Text(internshipDepartment).SetFont(boldFont))
                .Add(", sous la supervision de ")
                .Add(new Text(supervisorName).SetFont(boldFont))
                .Add(". ")
                .Add("Les objectifs du stage étaient : ")
                .Add(new Text(internshipObjectives).SetFont(boldFont))
                .Add(".");

            document.Add(content.SetTextAlignment(TextAlignment.JUSTIFIED).SetMarginBottom(20));

            document.Add(new Paragraph("La présente attestation est délivrée à l'intéressé(e) pour servir et valoir ce que de droit.")
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFontSize(12)
                .SetMarginBottom(40));

            document.Add(new Paragraph($"Fait à {companyInfo.Address ?? "Alger"}, le {DateTime.Now:dd/MM/yyyy}")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(11));

            document.Add(new Paragraph("Signature et cachet de l'entreprise")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(11)
                .SetMarginTop(40));

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<string> GetAttestationFileNameAsync(int employeeId, AttestationType type)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId)
                ?? throw new ArgumentException("Employé non trouvé.");

            string baseName = type switch
            {
                AttestationType.Work when employee.Status == EmployeeStatus.Terminated => "Certificat_Travail",
                AttestationType.Work => "Attestation_Travail",
                AttestationType.Internship => "Attestation_Stage",
                _ => "Attestation"
            };

            string safeName = string.Join("_", employee.FullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Replace("/", "-")));

            return $"{baseName}_{safeName}_{DateTime.Now:yyyyMMdd}.pdf";
        }

        private void AddCompanyHeader(Document document, CompanyInfo companyInfo, PdfFont boldFont, PdfFont normalFont)
        {
            if (companyInfo.LogoBytes != null && !string.IsNullOrEmpty(companyInfo.LogoMimeType))
            {
                var imageData = ImageDataFactory.Create(companyInfo.LogoBytes);
                var image = new Image(imageData)
                    .SetWidth(80)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(image);
            }

            document.Add(new Paragraph(companyInfo.Name)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16)
                .SetFont(boldFont));

            document.Add(new Paragraph(companyInfo.Address)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetFont(normalFont));

            document.Add(new Paragraph($"RC : {companyInfo.RC}     |     NIF : {companyInfo.NIF}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetFont(normalFont)
                .SetMarginBottom(10));

            document.Add(new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine()));
            document.Add(new Paragraph("\n"));
        }
    }

    public enum AttestationType
    {
        Work,
        Internship
    }
}
