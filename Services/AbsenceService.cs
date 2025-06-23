using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

public class AbsenceService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AbsenceService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    public async Task<int> GetTotalAbsenceCountAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.AbsenceRecords.CountAsync();
    }

    public async Task CreateAsync(AbsenceRecord absence)
    {
        using var context = _contextFactory.CreateDbContext();
        context.AbsenceRecords.Add(absence);
        await context.SaveChangesAsync();
    }
    public async Task UpdateAsync(AbsenceRecord absence)
    {
        using var context = _contextFactory.CreateDbContext();
        context.AbsenceRecords.Update(absence);
        await context.SaveChangesAsync();
    }
    public async Task<List<AbsenceRecord>> GetByEmployeeAndMonthAsync(int employeeId, DateTime month)
    {
        var start = new DateTime(month.Year, month.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        using var context = _contextFactory.CreateDbContext();

        return await context.AbsenceRecords
            .Where(a => a.EmployeeId == employeeId && a.Date >= start && a.Date <= end)
            .ToListAsync();
    }
    public async Task<List<AbsenceRecord>> GetAllAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.AbsenceRecords
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }
    public async Task<List<AbsenceRecord>> GetFilteredAsync(
       int? employeeId = null,
       DateTime? startDate = null,
       DateTime? endDate = null)
    {
        using var context = _contextFactory.CreateDbContext();
        var query = context.AbsenceRecords.AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Date <= endDate.Value);
        }

        return await query
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }
    public async Task UpdateCountedStatus(int absenceId, bool newValue)
    {
        using var context = _contextFactory.CreateDbContext();
        var record = await context.AbsenceRecords.FindAsync(absenceId);

        if (record != null)
        {
            record.Counted = newValue;
            await context.SaveChangesAsync();
        }
    }


}




