using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using RH.Services;
using System;

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

        var rules = await _context.PayrollAdjustmentRules.Where(r => r.JobTitleId == employee.JobTitleId).ToListAsync();

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
        return await _context.Payrolls.Include(p => p.Employee).ToListAsync();
    }

    public async Task<Payroll?> GetByIdAsync(int id)
    {
        return await _context.Payrolls.Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payroll> CreateAsync(Payroll payroll)
    {
        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();
        return payroll;
    }

    public async Task UpdateAsync(Payroll payroll)
    {
        _context.Payrolls.Update(payroll);
        await _context.SaveChangesAsync();
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
}