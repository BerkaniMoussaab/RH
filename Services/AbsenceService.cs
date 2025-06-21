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
}
