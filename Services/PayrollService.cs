using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using RH.Services;
using System;
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
        var employee = await _context.Employees.Include(e => e.JobTitle).FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null) throw new Exception("Employee not found");

        var rules = await _context.PayrollAdjustmentRules
     .Where(r => r.JobTitles.Any(j => j.Id == employee.JobTitleId))
     .ToListAsync();


        decimal bonus = 0, deduction = 0;

        foreach (var rule in rules)
        {
            var value = rule.IsPercentage ? employee.MonthlySalary * (rule.Amount / 100) : rule.Amount;

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
            .Include(p => p.Employee) // ✅ Inclut l'employé
            .Include(p => p.AppliedRules)
            .ThenInclude(ar => ar.Rule)
            .ToListAsync();
    }


    public async Task<Payroll?> GetByIdAsync(int id)
    {
        return await _context.Payrolls
            .Include(p => p.AppliedRules)
                .ThenInclude(ar => ar.Rule) // 👍 OK, on a besoin du nom, % etc.
            .FirstOrDefaultAsync(p => p.Id == id); // ✅ Pas de ToListAsync ici
    }


    public async Task<Payroll> CreateAsync(Payroll payroll)
    {
        // Ensure AppliedRules contains all required data (Amount, Quantity)
        foreach (var rule in payroll.AppliedRules)
        {
            rule.Payroll = payroll; // Optional, but good for EF tracking
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

        // Update scalar fields
        existing.EmployeeId = payroll.EmployeeId;
        existing.PayDate = payroll.PayDate;
        existing.BaseSalary = payroll.BaseSalary;
        existing.Bonus = payroll.Bonus;
        existing.Deductions = payroll.Deductions;
        existing.AbsenceDays = payroll.AbsenceDays;
        existing.AbsenceDeduction = payroll.AbsenceDeduction;
        existing.ManualAbsenceDays = payroll.ManualAbsenceDays;
        existing.DeductionPerAbsenceDay = payroll.DeductionPerAbsenceDay;

        // Replace AppliedRules
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


    public async Task<Payroll?> GetLastPayrollForEmployeeAsync(int employeeId)
    {
        return await _context.Payrolls
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.PayDate)
            .FirstOrDefaultAsync();
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
    public async Task<int> GetPayrollCountAsync()
    {
        return await _context.Payrolls.CountAsync();
    }
}