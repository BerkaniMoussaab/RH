using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RH.Services
{
    public class PayrollService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdvanceService _advanceService;

        public PayrollService(ApplicationDbContext context, IAdvanceService advanceService)
        {
            _context = context;
            _advanceService = advanceService;
        }

        public async Task<List<Payroll>> GetAllAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Payrolls
                .Include(p => p.Employee)
                    .ThenInclude(e => e.JobTitle)
                .Include(p => p.AppliedRules)
                    .ThenInclude(ar => ar.Rule)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(p => p.PayDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.PayDate <= endDate.Value);
            }

            return await query.OrderByDescending(p => p.PayDate).ToListAsync();
        }

        public async Task<Payroll> GetByIdAsync(int id)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                    .ThenInclude(e => e.JobTitle)
                .Include(p => p.AppliedRules)
                    .ThenInclude(ar => ar.Rule)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payroll> CreateAsync(Payroll payroll)
        {
            // Ensure that AppliedRules are correctly linked to the new payroll
            foreach (var rule in payroll.AppliedRules)
            {
                rule.PayrollId = payroll.Id; // This will be 0 for new payrolls, will be updated by EF Core
                _context.Entry(rule).State = EntityState.Added;
            }

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();

            // After saving, the payroll.Id will be populated, update applied rules with the correct PayrollId
            foreach (var rule in payroll.AppliedRules)
            {
                rule.PayrollId = payroll.Id;
                _context.Entry(rule).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();

            return payroll;
        }

        public async Task<Payroll> UpdateAsync(Payroll payroll)
        {
            var existingPayroll = await _context.Payrolls
                .Include(p => p.AppliedRules)
                .FirstOrDefaultAsync(p => p.Id == payroll.Id);

            if (existingPayroll == null)
            {
                throw new KeyNotFoundException($"Payroll with ID {payroll.Id} not found.");
            }

            // Update scalar properties
            _context.Entry(existingPayroll).CurrentValues.SetValues(payroll);

            // Handle AppliedRules: Delete, Add, Update
            var existingRuleIds = existingPayroll.AppliedRules.Select(ar => ar.Id).ToList();
            var newRuleIds = payroll.AppliedRules.Select(ar => ar.Id).ToList();

            // Rules to remove
            foreach (var existingRule in existingPayroll.AppliedRules.ToList())
            {
                if (!newRuleIds.Contains(existingRule.Id))
                {
                    _context.PayrollAppliedRules.Remove(existingRule);
                }
            }

            // Rules to add or update
            foreach (var newRule in payroll.AppliedRules)
            {
                var existingRule = existingPayroll.AppliedRules.FirstOrDefault(ar => ar.Id == newRule.Id);
                if (existingRule == null) // New rule
                {
                    newRule.PayrollId = payroll.Id;
                    _context.PayrollAppliedRules.Add(newRule);
                }
                else // Existing rule, update properties
                {
                    _context.Entry(existingRule).CurrentValues.SetValues(newRule);
                }
            }

            await _context.SaveChangesAsync();
            return payroll;
        }

        public async Task DeleteAsync(int id)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll != null)
            {
                _context.Payrolls.Remove(payroll);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Payroll> GetLastPayrollForEmployeeAsync(int employeeId)
        {
            return await _context.Payrolls
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.PayDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PayrollAppliedRule>> GetAppliedRulesForPayrollAsync(int payrollId)
        {
            return await _context.PayrollAppliedRules
                .Include(ar => ar.Rule)
                .Where(ar => ar.PayrollId == payrollId)
                .ToListAsync();
        }

        // REVISED: GetAppliedRulesForEmployeeAsync to include rules from last payroll AND unassigned rules
        public async Task<List<PayrollAppliedRule>> GetAppliedRulesForEmployeeAsync(int employeeId)
        {
            var rules = new List<PayrollAppliedRule>();

            // 1. Get rules from the last saved payroll for this employee
            var lastPayroll = await GetLastPayrollForEmployeeAsync(employeeId);
            if (lastPayroll != null)
            {
                var lastPayrollRules = await _context.PayrollAppliedRules
                    .Include(r => r.Rule)
                    .Where(r => r.EmployeeId == employeeId && r.PayrollId == lastPayroll.Id)
                    .ToListAsync();
                rules.AddRange(lastPayrollRules);
            }

            // 2. Get rules that are applied but not yet linked to any payroll (PayrollId is null)
            // These are typically rules added in the UI for a new payroll before saving
            var unassignedRules = await _context.PayrollAppliedRules
                .Include(r => r.Rule)
                .Where(r => r.EmployeeId == employeeId && r.PayrollId == null)
                .ToListAsync();
            rules.AddRange(unassignedRules);

            // Order for consistent display
            return rules.OrderBy(r => r.Rule.Name).ThenBy(r => r.Id).ToList();
        }

        public async Task ProcessAdvanceDeductionsForPayrollAsync(Payroll payroll)
        {
            if (payroll.AdvanceDeductionsAmounts > 0 && payroll.EmployeeId > 0)
            {
                await _advanceService.ApplyDeductionToAdvancesAsync(payroll.EmployeeId, payroll.AdvanceDeductionsAmounts);
            }
        }

        public async Task ReverseAdvanceDeductionsAsync(int payrollId)
        {
            var payroll = await _context.Payrolls
                .Include(p => p.AppliedAdvances)
                .FirstOrDefaultAsync(p => p.Id == payrollId);

            if (payroll != null && payroll.AppliedAdvances != null)
            {
                foreach (var appliedAdvance in payroll.AppliedAdvances)
                {
                    var advance = await _advanceService.GetByIdAsync(appliedAdvance.AdvanceId);
                    if (advance != null)
                    {
                        advance.RemainingAmount += appliedAdvance.DeductedAmount;
                        await _advanceService.UpdateAsync(advance);
                    }
                }
                _context.Advances.RemoveRange(payroll.AppliedAdvances);
                await _context.SaveChangesAsync();
            }
        }

        public class AdvanceDeductionSummary
        {
            public List<Advance> ActiveAdvances { get; set; } = new List<Advance>();
            public decimal TotalActiveAmount => ActiveAdvances.Sum(a => a.RemainingAmount);
            public decimal MaximumDeductionAllowed { get; set; }
            public decimal ProposedDeduction { get; set; }
            public bool CanDeductFully => TotalActiveAmount <= MaximumDeductionAllowed;
            public bool HasActiveAdvances => ActiveAdvances.Any();
        }

        public async Task<AdvanceDeductionSummary> GetAdvanceDeductionSummaryAsync(int employeeId, decimal preliminaryNetPay)
        {
            var summary = new AdvanceDeductionSummary();
            summary.ActiveAdvances = await _advanceService.GetActiveAdvancesForEmployeeAsync(employeeId);

            // Calculate maximum allowed deduction (e.g., 30% of preliminary net pay)
            // This is a business rule, adjust as needed
            summary.MaximumDeductionAllowed = preliminaryNetPay * 0.3m; // Example: 30% of net pay

            // Proposed deduction is the minimum of total active advances and maximum allowed deduction
            summary.ProposedDeduction = Math.Min(summary.TotalActiveAmount, summary.MaximumDeductionAllowed);

            return summary;
        }
    }
}


