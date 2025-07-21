using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public class AttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MonthlyAttendanceSummary>> GetAllSummariesAsync()
        {
            return await _context.MonthlyAttendanceSummaries
                .Include(m => m.Employee)
                .ToListAsync();
        }

        public async Task<MonthlyAttendanceSummary?> GetSummaryAsync(int employeeId, int year, int month)
        {
            return await _context.MonthlyAttendanceSummaries
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Year == year && x.Month == month);
        }

        public async Task AddOrUpdateSummaryAsync(MonthlyAttendanceSummary summary)
        {
            // Ensure Employee is loaded to access MonthlySalary
            if (summary.Employee == null)
            {
                summary.Employee = await _context.Employees.FindAsync(summary.EmployeeId);
            }

            // Calculate smart deduction using salary, grace, and cap
            summary.CalculateDeductionSmart();

            var existing = await _context.MonthlyAttendanceSummaries
                .FirstOrDefaultAsync(x => x.EmployeeId == summary.EmployeeId && x.Year == summary.Year && x.Month == summary.Month);

            if (existing != null)
            {
                existing.DaysAbsent = summary.DaysAbsent;
                existing.LateArrivals = summary.LateArrivals;
                existing.TotalDeduction = summary.TotalDeduction;
            }
            else
            {
                _context.MonthlyAttendanceSummaries.Add(summary);
            }

            await _context.SaveChangesAsync();
        }

    }
}
