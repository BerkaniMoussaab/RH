using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public class LeaveRequestService : ILeaveRequestService
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

            // 1. Calculate full months since inscription
            int monthsElapsed = ((today.Year - inscriptionDate.Year) * 12) + today.Month - inscriptionDate.Month;
            if (today.Day < inscriptionDate.Day)
                monthsElapsed--; // Current month not completed

            monthsElapsed = Math.Max(monthsElapsed, 0);

            // 2. Calculate earned leave days (2.5 days/month)
            float earnedDays = monthsElapsed * 2.5f;

            // 3. Calculate used paid leave days (fixed translation issue)
            var approvedLeaves = await context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId
                             && lr.IsPaid
                             && lr.Status == LeaveStatus.Approved
                             && lr.StartDate >= inscriptionDate)
                .Select(lr => new { lr.StartDate, lr.EndDate })
                .ToListAsync();

            var usedDays = approvedLeaves.Sum(lr =>
            {
                var days = (lr.EndDate - lr.StartDate).Days + 1;
                return days > 0 ? days : 0;
            });

            // 4. Calculate total remaining days = initial + earned - used
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
