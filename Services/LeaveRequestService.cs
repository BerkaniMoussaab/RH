using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public class LeaveRequestService 
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public LeaveRequestService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<LeaveRequest>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.LeaveRequests
                .Include(lr => lr.Employee)
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

        public async Task<float> GetRemainingDaysAsync(int employeeId)
        {
            using var context = _contextFactory.CreateDbContext();

            var employee = await context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null || employee.InscriptionDate == null || employee.InitialRemainingDays == null)
                return 0;

            var inscriptionDate = employee.InscriptionDate.Value;
            var today = DateTime.Today;

            if (today < inscriptionDate)
                return 0;

            // 1. Mois complets depuis la date d’inscription
            int monthsElapsed = ((today.Year - inscriptionDate.Year) * 12) + today.Month - inscriptionDate.Month;
            if (today.Day < inscriptionDate.Day)
                monthsElapsed--; // Mois en cours non terminé

            monthsElapsed = Math.Max(monthsElapsed, 0);

            // 2. Jours de congé acquis (2,5 jours/mois)
            float earnedDays = monthsElapsed * 2.5f;

            // 3. Congés payés déjà pris
            var usedDays = await context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId
                             && lr.IsPaid
                             && lr.Status == LeaveStatus.Approved
                             && lr.StartDate >= inscriptionDate)
                .SumAsync(lr => Math.Max(0, EF.Functions.DateDiffDay(lr.StartDate, lr.EndDate) + 1));

            // 4. Solde total = initial + acquis - utilisés
            float total = employee.InitialRemainingDays.Value + earnedDays - usedDays;

            return Math.Max(total, 0);
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
    }
}
