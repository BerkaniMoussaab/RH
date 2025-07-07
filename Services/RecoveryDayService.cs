using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RecoveryDayService
{
    private readonly ApplicationDbContext _context;

    public RecoveryDayService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ✅ Get all recovery days
    public async Task<List<RecoveryDay>> GetAllAsync()
    {
        return await _context.RecoveryDays
            .Include(r => r.Employee)
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    // ✅ Get recovery days for a specific employee
    public async Task<List<RecoveryDay>> GetByEmployeeAsync(int employeeId)
    {
        return await _context.RecoveryDays
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    // ✅ Add a new recovery day
    public async Task AddAsync(RecoveryDay day)
    {
        _context.RecoveryDays.Add(day);
        await _context.SaveChangesAsync();
    }

    // ✅ Delete a recovery day
    public async Task DeleteAsync(int id)
    {
        var existing = await _context.RecoveryDays.FindAsync(id);
        if (existing != null)
        {
            _context.RecoveryDays.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }

    // ✅ Mark as used
    public async Task MarkAsUsedAsync(int id)
    {
        var day = await _context.RecoveryDays.FindAsync(id);
        if (day != null && !day.Used)
        {
            day.Used = true;
            await _context.SaveChangesAsync();
        }
    }

    // ✅ Mark as unused
    public async Task MarkAsUnusedAsync(int id)
    {
        var day = await _context.RecoveryDays.FindAsync(id);
        if (day != null && day.Used)
        {
            day.Used = false;
            await _context.SaveChangesAsync();
        }
    }

    // ✅ Get count of remaining (unused) recovery days for one employee
    public async Task<int> GetRemainingDaysAsync(int employeeId)
    {
        return await _context.RecoveryDays
            .Where(r => r.EmployeeId == employeeId && !r.Used)
            .CountAsync();
    }

    // ✅ Get dictionary of employeeId → remaining days (for dashboard)
    public async Task<Dictionary<int, int>> GetRemainingDaysForAllAsync()
    {
        return await _context.RecoveryDays
            .Where(r => !r.Used)
            .GroupBy(r => r.EmployeeId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }
}
