using RH.Models;
using Microsoft.EntityFrameworkCore;
using RH.Data;
using static RH.Components.Pages.Employees.AttestationGenerator;

namespace RH.Services
{
    public class AttestationGeneratorService
    {
        private readonly ApplicationDbContext _context;

        public AttestationGeneratorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AttestationData> GenerateWorkAttestationAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Where(e => !e.Deleted)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(e => e.Id == employeeId)
                ?? throw new ArgumentException("Employé non trouvé.");

            var companyInfo = await _context.CompanyInfos.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Les informations de l'entreprise ne sont pas configurées.");

            return new AttestationData
            {
                Type = AttestationType.Work,
                Employee = employee,
                CompanyInfo = companyInfo,
                GeneratedDate = DateTime.Now
            };
        }

        public async Task<AttestationData> GenerateInternshipAttestationAsync(
            int employeeId,
            DateTime internshipStartDate,
            DateTime internshipEndDate,
            string internshipDepartment,
            string supervisorName,
            string internshipObjectives)
        {
            var employee = await _context.Employees
                .Where(p => !p.Deleted)
                .FirstOrDefaultAsync(e => e.Id == employeeId)
                ?? throw new ArgumentException("Employé non trouvé.");

            var companyInfo = await _context.CompanyInfos.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Les informations de l'entreprise ne sont pas configurées.");

            return new AttestationData
            {
                Type = AttestationType.Internship,
                Employee = employee,
                CompanyInfo = companyInfo,
                GeneratedDate = DateTime.Now,
                InternshipStartDate = internshipStartDate,
                InternshipEndDate = internshipEndDate,
                InternshipDepartment = internshipDepartment,
                SupervisorName = supervisorName,
                InternshipObjectives = internshipObjectives
            };
        }

        public async Task<string> GetAttestationFileNameAsync(int employeeId, AttestationType type)
        {
            var employee = await _context.Employees.Where(p => !p.Deleted).FirstOrDefaultAsync(e => e.Id == employeeId)
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

            return $"{baseName}_{safeName}_{DateTime.Now:yyyyMMdd}";
        }
        public async Task<LeaveRequestData> GenerateLeaveRequestAttestationAsync(int leaveRequestId)
        {
            var leave = await _context.LeaveRequests
                            .Include(l => l.Employee)
                            .ThenInclude(e => e.JobTitle)
                            .FirstOrDefaultAsync(l => l.Id == leaveRequestId && l.Status == LeaveStatus.Approved)
                        ?? throw new ArgumentException("Demande de congé approuvée non trouvée.");

            var companyInfo = await _context.CompanyInfos.FirstOrDefaultAsync()
                              ?? throw new InvalidOperationException("Les informations de l'entreprise ne sont pas configurées.");

            return new LeaveRequestData
            {
                Leave = leave,
                CompanyInfo = companyInfo
            };
        }

    }
}

