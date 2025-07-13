using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using System;

public class PayrollAdjustmentRuleService : IPayrollAdjustmentRuleService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public PayrollAdjustmentRuleService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<PayrollAdjustmentRule>> GetAllAsync()
    {
        using var context = _dbContextFactory.CreateDbContext();

        var rules = await context.PayrollAdjustmentRules
            .Include(r => r.JobTitles)
            .ToListAsync();

        // Detach to avoid accessing disposed context later
        foreach (var rule in rules)
        {
            context.Entry(rule).State = EntityState.Detached;
            foreach (var jt in rule.JobTitles)
            {
                context.Entry(jt).State = EntityState.Detached;
            }
        }

        return rules;
    }


    public async Task<PayrollAdjustmentRule?> GetByIdAsync(int id)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            return await context.PayrollAdjustmentRules.Include(r => r.JobTitles).FirstOrDefaultAsync(r => r.Id == id);
        }
    }

    public async Task AddAsync(PayrollAdjustmentRule rule)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var jobTitleIds = rule.JobTitles.Select(j => j.Id).ToList();

            rule.JobTitles = await context.JobTitles
                .Where(j => jobTitleIds.Contains(j.Id))
                .ToListAsync();

            context.PayrollAdjustmentRules.Add(rule);
            await context.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(PayrollAdjustmentRule rule)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var existingRule = await context.PayrollAdjustmentRules
                .Include(r => r.JobTitles)
                .FirstOrDefaultAsync(r => r.Id == rule.Id);

            if (existingRule == null)
                throw new InvalidOperationException($"PayrollAdjustmentRule with Id {rule.Id} not found.");

            context.Entry(existingRule).CurrentValues.SetValues(rule);

            existingRule.JobTitles.Clear();

            var jobTitleIds = rule.JobTitles.Select(j => j.Id).ToList();
            var jobTitles = await context.JobTitles
                .Where(j => jobTitleIds.Contains(j.Id))
                .ToListAsync();

            foreach (var jt in jobTitles)
            {
                existingRule.JobTitles.Add(jt);
            }

            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var rule = await context.PayrollAdjustmentRules.FindAsync(id);
            if (rule != null)
            {
                context.PayrollAdjustmentRules.Remove(rule);
                await context.SaveChangesAsync();
            }
        }
    }
}

