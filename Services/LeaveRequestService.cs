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

        public async Task<int> GetRemainingDaysAsync(int employeeId)
        {
            using var context = _contextFactory.CreateDbContext();

            var employee = await context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null || employee.InscriptionDate == null || employee.InitialRemainingDays == null)
                return 0;

            var fromDate = employee.InscriptionDate.Value;
            var initialBalance = employee.InitialRemainingDays.Value;

            var usedDays = await context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId
                             && lr.IsPaid
                             && lr.Status == LeaveStatus.Approved
                             && lr.StartDate >= fromDate)
                .SumAsync(lr => EF.Functions.DateDiffDay(lr.StartDate, lr.EndDate) + 1);

            return Math.Max((int)Math.Floor(initialBalance - usedDays), 0);
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
