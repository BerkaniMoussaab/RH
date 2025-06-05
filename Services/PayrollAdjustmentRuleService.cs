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
        return await _context.PayrollAdjustmentRules.Include(r => r.JobTitle).ToListAsync();
    }

    public async Task<PayrollAdjustmentRule?> GetByIdAsync(int id)
    {
        return await _context.PayrollAdjustmentRules.Include(r => r.JobTitle).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(PayrollAdjustmentRule rule)
    {
        _context.PayrollAdjustmentRules.Add(rule);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PayrollAdjustmentRule rule)
    {
        _context.PayrollAdjustmentRules.Update(rule);
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
