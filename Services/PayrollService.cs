using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using RH.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RH.Components.Pages.Payroll.PayrollList;

public class PayrollService : IPayrollService
{
    private readonly ApplicationDbContext _context;

    public PayrollService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Payroll> GeneratePayrollForEmployee(int employeeId, DateTime payDate)
    {
        var employee = await _context.Employees
            .Include(e => e.JobTitle)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
            throw new Exception("Employee not found");

        var rules = await _context.PayrollAdjustmentRules
            .Where(r => r.JobTitles.Any(j => j.Id == employee.JobTitleId))
            .ToListAsync();

        decimal bonus = 0, deduction = 0;

        foreach (var rule in rules)
        {
            var value = rule.IsPercentage
                ? employee.MonthlySalary * (rule.Amount / 100)
                : rule.Amount;

            if (rule.Type == AdjustmentType.Bonus)
                bonus += value;
            else
                deduction += value;
        }

        var payroll = new Payroll
        {
            EmployeeId = employeeId,
            PayDate = payDate,
            BaseSalary = employee.MonthlySalary,
            Bonus = bonus,
            Deductions = deduction
        };

        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();
        return payroll;
    }

    public async Task<List<Payroll>> GetAllAsync()
    {
        return await _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.AppliedRules)
                .ThenInclude(ar => ar.Rule)
            .ToListAsync();
    }

    public async Task<Payroll?> GetByIdAsync(int id)
    {
        return await _context.Payrolls
            .Include(p => p.AppliedRules)
                .ThenInclude(ar => ar.Rule)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payroll> CreateAsync(Payroll payroll)
    {
        foreach (var rule in payroll.AppliedRules)
        {
            rule.Payroll = payroll;
        }

        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();
        return payroll;
    }

    public async Task<Payroll> UpdateAsync(Payroll payroll)
    {
        var existing = await _context.Payrolls
            .Include(p => p.AppliedRules)
            .FirstOrDefaultAsync(p => p.Id == payroll.Id);

        if (existing == null)
            throw new Exception("Payroll not found");

        existing.EmployeeId = payroll.EmployeeId;
        existing.PayDate = payroll.PayDate;
        existing.BaseSalary = payroll.BaseSalary;
        existing.Bonus = payroll.Bonus;
        existing.Deductions = payroll.Deductions;
        existing.AbsenceDays = payroll.AbsenceDays;
        existing.AbsenceDeduction = payroll.AbsenceDeduction;
        existing.ManualAbsenceDays = payroll.ManualAbsenceDays;
        existing.DeductionPerAbsenceDay = payroll.DeductionPerAbsenceDay;
        existing.Cash = payroll.Cash;
        existing.Transaction = payroll.Transaction;
        existing.PayrollStartDate = payroll.PayrollStartDate;
        existing.PayrollEndDate = payroll.PayrollEndDate;
        existing.PayrollStartDate = payroll.PayrollStartDate;
        existing.PayrollEndDate = payroll.PayrollEndDate;
        _context.PayrollAppliedRules.RemoveRange(existing.AppliedRules);
        foreach (var rule in payroll.AppliedRules)
        {
            existing.AppliedRules.Add(new PayrollAppliedRule
            {
                RuleId = rule.RuleId,
                Amount = rule.Amount,
                Quantity = rule.Quantity
            });
        }

        await _context.SaveChangesAsync();
        return existing;
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

    public async Task<Payroll?> GetLastPayrollForEmployeeAsync(int employeeId)
    {
        return await _context.Payrolls
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.PayDate)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetPayrollCountAsync()
    {
        return await _context.Payrolls.CountAsync();
    }

    public async Task<List<Payroll>> GetPayrollsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var allPayrolls = await GetAllAsync();
        return allPayrolls
            .Where(p => p.PayrollStartDate.HasValue && p.PayrollEndDate.HasValue &&
                        p.PayrollStartDate.Value <= endDate &&
                        p.PayrollEndDate.Value >= startDate)
            .OrderByDescending(p => p.PayrollStartDate)
            .ToList();
    }

    public async Task<List<Payroll>> GetEmployeePayrollsByDateRangeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var allPayrolls = await GetAllAsync();
        return allPayrolls
            .Where(p => p.EmployeeId == employeeId &&
                        p.PayrollStartDate.HasValue && p.PayrollEndDate.HasValue &&
                        p.PayrollStartDate.Value <= endDate &&
                        p.PayrollEndDate.Value >= startDate)
            .OrderByDescending(p => p.PayrollStartDate)
            .ToList();
    }

    public async Task<bool> ValidatePayrollPeriodAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludePayrollId = null)
    {
        var existingPayrolls = await GetEmployeePayrollsByDateRangeAsync(employeeId, startDate, endDate);

        if (excludePayrollId.HasValue)
        {
            existingPayrolls = existingPayrolls
                .Where(p => p.Id != excludePayrollId.Value)
                .ToList();
        }
        return !existingPayrolls.Any();
    }

    public async Task<Payroll?> GetLastPayrollBeforeDateAsync(int employeeId, DateTime beforeDate)
    {
        var allPayrolls = await GetAllAsync();
        return allPayrolls
            .Where(p => p.EmployeeId == employeeId &&
                        p.PayrollEndDate.HasValue &&
                        p.PayrollEndDate.Value < beforeDate)
            .OrderByDescending(p => p.PayrollEndDate)
            .FirstOrDefault();
    }

    public async Task<(DateTime StartDate, DateTime EndDate)> GetSuggestedPayrollPeriodAsync(int employeeId)
    {
        var lastPayroll = await GetLastPayrollForEmployeeAsync(employeeId);

        if (lastPayroll?.PayrollEndDate.HasValue == true)
        {
            var startDate = lastPayroll.PayrollEndDate.Value.AddDays(1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return (startDate, endDate);
        }
        else
        {
            var today = DateTime.Today;
            var startDate = new DateTime(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return (startDate, endDate);
        }
    }
}
