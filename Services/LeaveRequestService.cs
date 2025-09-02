using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<LeaveRequestService>? _logger;

        public LeaveRequestService(
     IDbContextFactory<ApplicationDbContext> contextFactory,
     ILogger<LeaveRequestService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }


        public async Task<List<LeaveRequest>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.LeaveRequests
                .Include(lr => lr.Employee).ThenInclude(j=>j.JobTitle)
                .ToListAsync();
        }

        public async Task<LeaveRequest?> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.LeaveRequests
                .Include(lr => lr.Employee)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<LeaveRequest> CreateAsync(LeaveRequest request)
        {
            using var context = _contextFactory.CreateDbContext();
            context.LeaveRequests.Add(request);
            await context.SaveChangesAsync();
            return request;
        }

        public async Task<LeaveRequest> UpdateAsync(LeaveRequest request)
        {
            using var context = _contextFactory.CreateDbContext();
            context.LeaveRequests.Update(request);
            await context.SaveChangesAsync();
            return request;
        }
        public async Task DeleteAsync(int requestId)
        {
            using var context = _contextFactory.CreateDbContext();

            var request = await context.LeaveRequests.FindAsync(requestId);
            if (request == null)
                throw new InvalidOperationException($"Leave request with ID {requestId} not found.");

            context.LeaveRequests.Remove(request);
            await context.SaveChangesAsync();
        }

        public async Task<float> GetRemainingDaysAsync(int employeeId)
        {
            using var context = _contextFactory.CreateDbContext();

            var employee = await context.Employees.Include(J => J.JobTitle).ThenInclude(LeavePolicy => LeavePolicy.LeavePolicy)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null || employee.InscriptionDate == null || employee.InitialRemainingDays == null)
                return 0;

            var inscriptionDate = employee.InscriptionDate.Value.Date;
            var today = DateTime.Today;

            if (today < inscriptionDate)
                return 0;

            // 1. Calculate full completed months since inscription date
            int monthsElapsed = ((today.Year - inscriptionDate.Year) * 12) + (today.Month - inscriptionDate.Month);
            if (today < inscriptionDate.AddMonths(monthsElapsed))
                monthsElapsed--;

            monthsElapsed = Math.Max(monthsElapsed, 0);

            // 2. Calculate earned leave days (2.5 days per completed month)
            float earnedDays = monthsElapsed * ((float)employee.JobTitle.LeavePolicy.AnnualLeaveDays / 12f);


            // 3. Calculate used paid leave days
            var approvedLeaves = await context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId
                             && lr.IsPaid
                             && lr.Status == LeaveStatus.Approved
                             && lr.EndDate.Date >= inscriptionDate) // include overlapping leaves
                .Select(lr => new { lr.StartDate, lr.EndDate })
                .ToListAsync();

            var usedDays = approvedLeaves.Sum(lr =>
            {
                var start = lr.StartDate.Date;
                var end = lr.EndDate.Date;
                var days = (end - start).TotalDays + 1;
                return days > 0 ? (float)days : 0f;
            });

            // 4. Calculate total remaining days
            float total = employee.InitialRemainingDays.Value + earnedDays - usedDays;

            return total;
        }






        public async Task UpdateStatusAsync(int requestId, LeaveStatus newStatus)
        {
            using var context = _contextFactory.CreateDbContext();
            var request = await context.LeaveRequests.FindAsync(requestId);
            if (request != null)
            {
                request.Status = newStatus;
                await context.SaveChangesAsync();
            }
        }
      
        /// <summary>
        /// Génère le HTML pour l'impression d'une demande de congé
        /// </summary>
        //public async Task<string> GenerateLeaveRequestHtmlAsync(LeaveRequestPrintModel model)
        //{
        //    try
        //    {
        //        _logger?.LogInformation("Génération du HTML pour la demande de congé {LeaveRequestId}", model.LeaveRequest.Id);

        //        // Template HTML de base
        //        var htmlTemplate = await GetHtmlTemplateAsync();

        //        // Remplacer les placeholders avec les données réelles
        //        var html = htmlTemplate
        //            .Replace("{{CompanyName}}", model.CompanyInfo.Name)
        //            .Replace("{{CompanyAddress}}", model.CompanyInfo.Address)
        //            .Replace("{{CompanyCity}}", model.CompanyInfo.City)
        //            .Replace("{{CompanyRC}}", model.CompanyInfo.RC)
        //            .Replace("{{CompanyNIF}}", model.CompanyInfo.NIF)
        //            .Replace("{{CompanyPhone}}", model.CompanyInfo.Phone)
        //            .Replace("{{CompanyEmail}}", model.CompanyInfo.Email)
        //            .Replace("{{EmployeeName}}", model.Employee.FullName)
        //            .Replace("{{EmployeePosition}}", model.Employee.JobTitle.Title)
                 
                 
        //            .Replace("{{StartDate}}", model.LeaveRequest.StartDate.ToString("dd/MM/yyyy"))
        //            .Replace("{{EndDate}}", model.LeaveRequest.EndDate.ToString("dd/MM/yyyy"))
        //            .Replace("{{TotalDays}}", model.LeaveRequest.TotalDays.ToString())
        //            .Replace("{{Reason}}", model.LeaveRequest.Reason ?? "")
        //            .Replace("{{Status}}", GetStatusText(model.LeaveRequest.Status))
        //            .Replace("{{IsPaid}}", model.LeaveRequest.IsPaid ? "checked" : "")
        //            .Replace("{{IsUnpaid}}", !model.LeaveRequest.IsPaid ? "checked" : "")
        //            .Replace("{{SupervisorName}}", model.SupervisorName ?? "")
        //            .Replace("{{SupervisorTitle}}", model.SupervisorTitle ?? "")
        //            .Replace("{{PrintDate}}", DateTime.Now.ToString("dd/MM/yyyy"));

        //        // Ajouter le logo si disponible
        //        if (model.CompanyInfo.LogoBytes != null && !string.IsNullOrEmpty(model.CompanyInfo.LogoMimeType))
        //        {
        //            var logoBase64 = Convert.ToBase64String(model.CompanyInfo.LogoBytes);
        //            var logoSrc = $"data:{model.CompanyInfo.LogoMimeType};base64,{logoBase64}";
        //            html = html.Replace("{{LogoSrc}}", logoSrc).Replace("{{LogoDisplay}}", "block");
        //        }
        //        else
        //        {
        //            html = html.Replace("{{LogoSrc}}", "").Replace("{{LogoDisplay}}", "none");
        //        }

        //        return html;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(ex, "Erreur lors de la génération du HTML pour la demande de congé");
        //        throw;
        //    }
        //}

        /// <summary>
        /// Déclenche l'impression via JavaScript
        /// </summary>
        public async Task PrintLeaveRequestAsync(IJSRuntime jsRuntime)
        {
            try
            {
                _logger?.LogInformation("Déclenchement de l'impression");
                await jsRuntime.InvokeVoidAsync("window.print");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erreur lors du déclenchement de l'impression");
                throw;
            }
        }

        /// <summary>
        /// Génère un PDF de la demande de congé (nécessite une bibliothèque PDF)
        /// </summary>
       
        /// <summary>
        /// Récupère le template HTML de base
        /// </summary>
     

    /// <summary>
    /// Extensions pour faciliter l'utilisation du service
    /// </summary>
   
}
}
