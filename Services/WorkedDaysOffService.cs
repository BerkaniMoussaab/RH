using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public class WorkedDaysOffService
    {
        private readonly ApplicationDbContext _context;

        public WorkedDaysOffService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<WorkedDayOff>> GetAllAsync()
        {
            return await _context.WorkedDaysOff.Include(w => w.Employee).ToListAsync();
        }
        public async Task<int> GetWorkedDayOffCountAsync(int employeeId, DateTime start, DateTime end)
        {
            return await _context.WorkedDaysOff
                .Where(w => w.EmployeeId == employeeId && w.Date >= start && w.Date <= end && w.GrantsBonus)
                .CountAsync();
        }

        public async Task<List<WorkedDayOff>> GetByEmployeeAsync(int employeeId)
        {
            return await _context.WorkedDaysOff
                .Where(w => w.EmployeeId == employeeId)
                .OrderByDescending(w => w.Date)
                .ToListAsync();
        }

        public async Task AddAsync(WorkedDayOff entry)
        {
            _context.WorkedDaysOff.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task ConvertToRecovery(int workedDayId)
        {
            var worked = await _context.WorkedDaysOff
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Id == workedDayId);

            if (worked == null || !worked.GrantsRecoveryDay || worked.ConvertedToRecovery)
                return;

            var companyInfo = await _context.CompanyInfos.FirstOrDefaultAsync();
            var ratio = companyInfo?.RecoveryDaysPerWorkedDayOff ?? 1f;

            // Total recovery time earned = quantity of day worked × company-configured ratio
            float totalRecovery = worked.Quantity * ratio;

            // Add as one RecoveryDay with quantity (preferred)
            var recoveryDay = new RecoveryDay
            {
                EmployeeId = worked.EmployeeId,
                Date = DateTime.Today,
                Reason = $"Repos compensatoire pour travail du {worked.Date:dd/MM/yyyy}",
                Quantity = totalRecovery
            };

            _context.RecoveryDays.Add(recoveryDay);
            worked.ConvertedToRecovery = true;

            await _context.SaveChangesAsync();
        }



        public async Task MarkBonusPaid(int id)
        {
            var worked = await _context.WorkedDaysOff.FindAsync(id);
            if (worked != null && worked.GrantsBonus)
            {
                worked.BonusPaid = true;
                await _context.SaveChangesAsync();
            }
        }
    }

}
