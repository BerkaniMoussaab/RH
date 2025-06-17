using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using System;

public class PayrollAdjustmentRuleService : IPayrollAdjustmentRuleService
{
    private readonly ApplicationDbContext _context;

    public PayrollAdjustmentRuleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PayrollAdjustmentRule>> GetAllAsync()
    {
        return await _context.PayrollAdjustmentRules
            .Include(r => r.JobTitles) // This is mandatory
            .ToListAsync();
    }


    public async Task<PayrollAdjustmentRule?> GetByIdAsync(int id)
    {
        return await _context.PayrollAdjustmentRules.Include(r => r.JobTitles).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(PayrollAdjustmentRule rule)
    {
        // Reuse tracked JobTitle entities from context
        var jobTitleIds = rule.JobTitles.Select(j => j.Id).ToList();

        rule.JobTitles = await _context.JobTitles
            .Where(j => jobTitleIds.Contains(j.Id))
            .ToListAsync();

        _context.PayrollAdjustmentRules.Add(rule);
        await _context.SaveChangesAsync();
    }




    public async Task UpdateAsync(PayrollAdjustmentRule rule)
    {
        // Load the existing entity from the context
        var existingRule = await _context.PayrollAdjustmentRules
            .Include(r => r.JobTitles)
            .FirstOrDefaultAsync(r => r.Id == rule.Id);

        if (existingRule == null)
            throw new InvalidOperationException($"PayrollAdjustmentRule with Id {rule.Id} not found.");

        // Update scalar properties (if any)
        _context.Entry(existingRule).CurrentValues.SetValues(rule);

        // Update many-to-many JobTitles
        existingRule.JobTitles.Clear();

        var jobTitleIds = rule.JobTitles.Select(j => j.Id).ToList();
        var jobTitles = await _context.JobTitles
            .Where(j => jobTitleIds.Contains(j.Id))
            .ToListAsync();

        foreach (var jt in jobTitles)
        {
            existingRule.JobTitles.Add(jt);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var rule = await _context.PayrollAdjustmentRules.FindAsync(id);
        if (rule != null)
        {
            _context.PayrollAdjustmentRules.Remove(rule);
            await _context.SaveChangesAsync();
        }
    }
}
