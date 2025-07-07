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

        public async Task ConvertToRecovery(int id)
        {
            var worked = await _context.WorkedDaysOff.FindAsync(id);
            if (worked != null && worked.GrantsRecoveryDay && !worked.ConvertedToRecovery)
            {
                worked.ConvertedToRecovery = true;

                // Add RecoveryDay entry
                var recovery = new RecoveryDay
                {
                    EmployeeId = worked.EmployeeId,
                    Date = DateTime.Today,
                    Reason = $"Repos compensatoire pour travail du {worked.Date:dd/MM/yyyy}"
                };

                _context.RecoveryDays.Add(recovery);
                await _context.SaveChangesAsync();
            }
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
