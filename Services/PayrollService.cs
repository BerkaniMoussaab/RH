using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using RH.Services;
using System.Data;

public class PayrollService : IPayrollService
{
    private readonly ApplicationDbContext _context;
    private readonly IAdvanceService _advanceService;

    public PayrollService(ApplicationDbContext context, IAdvanceService advanceService)
    {
        _context = context;
        _advanceService = advanceService ?? throw new ArgumentNullException(nameof(advanceService));
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

    public async Task<List<Payroll>> GetAllAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee)
            .ThenInclude(e => e.JobTitle)
            .Include(p => p.AppliedRules)
            .ThenInclude(ar => ar.Rule)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(p => p.PayDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.PayDate <= endDate.Value.AddDays(1));

        return await query.OrderByDescending(d => d.PayDate).ToListAsync();
    }



    public async Task<Payroll?> GetByIdAsync(int id)
    {
        return await _context.Payrolls
            .Include(p => p.AppliedRules)
                .ThenInclude(ar => ar.Rule)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // CORRIGÉ : Utilisation directe de _context
    public async Task<Payroll> CreateAsync(Payroll payroll)
    {
        var freshPayroll = new Payroll
        {
            EmployeeId = payroll.EmployeeId,
            BaseSalary = payroll.BaseSalary,
            DeductionPerAbsenceDay = payroll.DeductionPerAbsenceDay,
            AdvanceDeductionsAmounts = payroll.AdvanceDeductionsAmounts,
            Bonus = payroll.Bonus,
            Deductions = payroll.Deductions,
            Cash = payroll.Cash,
            Transaction = payroll.Transaction,
            PayDate = DateTime.Now,
            AbsenceDays = payroll.AbsenceDays,
            AbsenceDeduction = payroll.AbsenceDeduction,
            ManualAbsenceDays = payroll.ManualAbsenceDays,
            TransactionIsManual = payroll.TransactionIsManual,
            PayrollEndDate = payroll.PayrollEndDate,
            PayrollStartDate = payroll.PayrollStartDate,
        };

        _context.Payrolls.Add(freshPayroll);
        await _context.SaveChangesAsync();

        foreach (var rule in payroll.AppliedRules)
        {
            if (rule.Id != 0)
            {
                rule.PayrollId = payroll.Id;
            }else{
                var newRule = new PayrollAppliedRule
                {
                    RuleId = rule.RuleId,
                    Amount = rule.Amount,
                    Quantity = rule.Quantity,
                    EmployeeId = rule.EmployeeId,
                    PayrollId = freshPayroll.Id,
                    Date = DateTime.Now,
                    Notes = rule.Notes
                };
                _context.PayrollAppliedRules.Add(newRule);
            }
            
           
        }
        
        await _context.SaveChangesAsync();
        var rulesWithOutPayroll =
            _context.PayrollAppliedRules.Where(r => r.EmployeeId == payroll.EmployeeId && r.PayrollId == null);
         _context.PayrollAppliedRules.RemoveRange(rulesWithOutPayroll);
         await _context.SaveChangesAsync();
        return freshPayroll;
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

        _context.PayrollAppliedRules.RemoveRange(existing.AppliedRules);

        foreach (var rule in payroll.AppliedRules)
        {
            existing.AppliedRules.Add(new PayrollAppliedRule
            {
                RuleId = rule.RuleId,
                Amount = rule.Amount,
                Quantity = rule.Quantity,
                Date = rule.Date,
                Notes = rule.Notes,
                PayrollId = rule.PayrollId,
                EmployeeId = rule.EmployeeId
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
            .Include(p => p.Employee) // ✅ Include the related Employee
            .FirstOrDefaultAsync();
    }


    public async Task<int> GetPayrollCountAsync()
    {
        return await _context.Payrolls.CountAsync();
    }



    // CORRIGÉ : Utilisation directe de _context
    // Existing one
    public async Task<List<PayrollAppliedRule>> GetAppliedRulesForEmployeeAsync(int employeeId)
    {
        return await _context.PayrollAppliedRules
            .Include(r => r.Rule)
            .Where(r => r.EmployeeId == employeeId && r.PayrollId == null)
            .ToListAsync();
    }

    // Overload with date range
    public async Task<List<PayrollAppliedRule>> GetAppliedRulesForEmployeeAsync(
        int employeeId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.PayrollAppliedRules
            .Include(r => r.Rule)
            .Where(r => r.EmployeeId == employeeId );

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.Date != null && r.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.Date != null && r.Date <= toDate.Value);
        }

        return await query.OrderByDescending(d=>d.Date).ToListAsync();
    }




    public async Task<List<Payroll>> GetEmployeePayrollsByDateRangeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var allPayrolls = await GetAllAsync(null, null);
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
        var allPayrolls = await GetAllAsync(null, null);
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
    public async Task RemoveAppliedRuleAsync (int AppliedRuleId)
    {
        _context.Remove(int.Parse(AppliedRuleId.ToString()));
        await _context.SaveChangesAsync();
    }
    public async Task<decimal> CalculateAdvanceDeductionsAsync(int employeeId, decimal preliminaryNetPay)
    {
        try
        {
            var maxDeductionAmount = await _advanceService.CalculateMaximumDeductionAsync(employeeId, preliminaryNetPay);
            var activeAdvances = await _advanceService.GetActiveAdvancesForEmployeeAsync(employeeId);
            var totalPossibleDeduction = activeAdvances.Sum(a => a.RemainingAmount);
            return Math.Min(totalPossibleDeduction, maxDeductionAmount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating advance deductions: {ex.Message}");
            return 0;
        }
    }

    public async Task<List<AdvanceDeduction>> ProcessAdvanceDeductionsForPayrollAsync(Payroll payroll)
    {
        try
        {
            if (payroll.AdvanceDeductionsAmounts <= 0)
                return new List<AdvanceDeduction>();

            var deductions = await _advanceService.ProcessAdvanceDeductionsAsync(
                payroll.EmployeeId,
                payroll.Id,
                payroll.AdvanceDeductionsAmounts);

            return deductions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing advance deductions: {ex.Message}");
            throw;
        }
    }

    public async Task<AdvanceDeductionSummary> GetAdvanceDeductionSummaryAsync(int employeeId, decimal preliminaryNetPay)
    {
        try
        {
            var activeAdvances = await _advanceService.GetActiveAdvancesForEmployeeAsync(employeeId) ?? new List<Advance>();
            var maxDeductionAmount = await _advanceService.CalculateMaximumDeductionAsync(employeeId, preliminaryNetPay);
            var totalActiveAmount = activeAdvances.Sum(a => a.RemainingAmount);

            return new AdvanceDeductionSummary
            {
                ActiveAdvances = activeAdvances,
                TotalActiveAmount = totalActiveAmount,
                MaximumDeductionAllowed = maxDeductionAmount,
                ProposedDeduction = Math.Min(totalActiveAmount, maxDeductionAmount)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting advance deduction summary: {ex.Message}");
            return new AdvanceDeductionSummary();
        }
    }

    public async Task ReverseAdvanceDeductionsAsync(int payrollId)
    {
        try
        {
            var deductions = await _context.AdvanceDeductions
                .Where(d => d.PayrollId == payrollId)
                .ToListAsync();

            foreach (var deduction in deductions)
            {
                var advance = await _advanceService.GetByIdAsync(deduction.AdvanceId);
                if (advance != null)
                {
                    advance.RemainingAmount += deduction.DeductedAmount;
                    if (advance.Status == AdvanceStatus.Completed)
                    {
                        advance.Status = AdvanceStatus.Active;
                        advance.CompletedAt = null;
                    }
                    await _advanceService.UpdateAsync(advance);
                }
            }

            _context.AdvanceDeductions.RemoveRange(deductions);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reversing advance deductions: {ex.Message}");
            throw;
        }
    }

    public List<AdvanceDeduction> GetDeductionsForPayroll(int payrollId)
    {
        return _context.AdvanceDeductions
            .Where(d => d.PayrollId == payrollId)
            .ToList();
    }

    // CORRIGÉ : Utilisation directe de _context
    public async Task ApplyRulesAsync(int employeeId, List<PayrollAppliedRule> rules)
    {
        var appliedEntities = rules.Select(r => new PayrollAppliedRule
        {
            EmployeeId = employeeId,
            RuleId = r.RuleId,
            Amount = r.Amount,
            Quantity = r.Quantity,
            Notes = r.Notes,
            Date = DateTime.Now

        });

        _context.PayrollAppliedRules.AddRange(appliedEntities);
        await _context.SaveChangesAsync();
    }
    public async Task deleteAppliedRule(int Id)
    {
        var rule = _context.PayrollAppliedRules.Find(Id);
        if (rule.PayrollId == null)
        {
            _context.PayrollAppliedRules.Remove(rule);
            await _context.SaveChangesAsync();
        }
     
    }
    public class AdvanceDeductionSummary
    {
        public List<Advance> ActiveAdvances { get; set; } = new();
        public decimal TotalActiveAmount { get; set; }
        public decimal MaximumDeductionAllowed { get; set; }
        public decimal ProposedDeduction { get; set; }
        public bool HasActiveAdvances => ActiveAdvances.Any();
        public bool CanDeductFully => TotalActiveAmount <= MaximumDeductionAllowed;
    }
}
