using RH.Models;
using Microsoft.EntityFrameworkCore;
using RH.Data;

namespace RH.Services
{
    public class RecoveryDayService
    {
        private readonly ApplicationDbContext _context;

        public RecoveryDayService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<RecoveryDay>> GetAllAsync()
        {
            return await _context.RecoveryDays
                .Include(r => r.Employee)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }
        public async Task<List<RecoveryDay>> GetByEmployeeAsync(int employeeId)
        {
            return await _context.RecoveryDays
                .Include(r => r.Employee)
                .Where(r => r.EmployeeId == employeeId)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        public async Task<RecoveryDay?> GetByIdAsync(int id)
        {
            return await _context.RecoveryDays
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<RecoveryDay> AddAsync(RecoveryDay recoveryDay)
        {
            _context.RecoveryDays.Add(recoveryDay);
            await _context.SaveChangesAsync();
            return recoveryDay;
        }

        public async Task<RecoveryDay> UpdateAsync(RecoveryDay recoveryDay)
        {
            _context.RecoveryDays.Update(recoveryDay);
            await _context.SaveChangesAsync();
            return recoveryDay;
        }

        public async Task DeleteAsync(int id)
        {
            var recoveryDay = await _context.RecoveryDays.FindAsync(id);
            if (recoveryDay != null)
            {
                // Also delete any usage records
                var usageRecords = await _context.RecoveryDayUsages
                    .Where(u => u.RecoveryDayId == id)
                    .ToListAsync();

                _context.RecoveryDayUsages.RemoveRange(usageRecords);
                _context.RecoveryDays.Remove(recoveryDay);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<float> GetRemainingDaysAsync(int employeeId)
        {
            var recoveryDays = await _context.RecoveryDays
                .Where(r => r.EmployeeId == employeeId)
                .ToListAsync();

            return recoveryDays.Sum(r => r.Quantity - r.UsedQuantity);
        }

        public async Task<bool> UsePartialRecoveryDayAsync(int recoveryDayId, float quantityToUse, DateTime usageDate, string? reason = null)
        {
            var recoveryDay = await GetByIdAsync(recoveryDayId);
            if (recoveryDay == null)
                return false;

            var availableQuantity = recoveryDay.Quantity - recoveryDay.UsedQuantity;
            if (quantityToUse > availableQuantity)
                return false;

            // Create usage record
            var usage = new RecoveryDayUsage
            {
                RecoveryDayId = recoveryDayId,
                UsageDate = usageDate,
                QuantityUsed = quantityToUse,
                Reason = reason
            };

            _context.RecoveryDayUsages.Add(usage);

            // Update the used quantity
            recoveryDay.UsedQuantity += quantityToUse;
            _context.RecoveryDays.Update(recoveryDay);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelUsageAsync(int usageId)
        {
            var usage = await _context.RecoveryDayUsages
                .Include(u => u.RecoveryDay)
                .FirstOrDefaultAsync(u => u.Id == usageId);

            if (usage == null || usage.RecoveryDay == null)
                return false;

            // Restore the used quantity
            usage.RecoveryDay.UsedQuantity -= usage.QuantityUsed;
            _context.RecoveryDays.Update(usage.RecoveryDay);

            // Remove the usage record
            _context.RecoveryDayUsages.Remove(usage);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RecoveryDayUsage>> GetUsageHistoryAsync(int recoveryDayId)
        {
            return await _context.RecoveryDayUsages
                .Where(u => u.RecoveryDayId == recoveryDayId)
                .OrderByDescending(u => u.UsageDate)
                .ToListAsync();
        }

        public async Task<List<RecoveryDayUsage>> GetEmployeeUsageHistoryAsync(int employeeId)
        {
            return await _context.RecoveryDayUsages
                .Include(u => u.RecoveryDay)
                .Where(u => u.RecoveryDay!.EmployeeId == employeeId)
                .OrderByDescending(u => u.UsageDate)
                .ToListAsync();
        }

        // Legacy methods for backward compatibility
        [Obsolete("Use UsePartialRecoveryDayAsync instead")]
        public async Task MarkAsUsedAsync(int id)
        {
            var recoveryDay = await GetByIdAsync(id);
            if (recoveryDay != null && recoveryDay.UsedQuantity < recoveryDay.Quantity)
            {
                await UsePartialRecoveryDayAsync(id, recoveryDay.Quantity - recoveryDay.UsedQuantity, DateTime.Today, "Utilisation complète");
            }
        }

        [Obsolete("Use CancelUsageAsync instead")]
        public async Task MarkAsUnusedAsync(int id)
        {
            var recoveryDay = await GetByIdAsync(id);
            if (recoveryDay != null)
            {
                // Cancel all usage for this recovery day
                var usages = await GetUsageHistoryAsync(id);
                foreach (var usage in usages)
                {
                    await CancelUsageAsync(usage.Id);
                }
            }
        }
    }
}

