using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public interface IAdvanceService
    {
        Task<List<Advance>> GetAllAsync();
        Task<List<Advance>> GetByEmployeeIdAsync(int employeeId);
        Task<List<Advance>> GetActiveAdvancesForEmployeeAsync(int employeeId);
        Task<Advance> GetByIdAsync(int id);
        Task<Advance> CreateAsync(Advance advance);
        Task<Advance> UpdateAsync(Advance advance);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelAdvanceAsync(int id);
        Task<decimal> GetTotalActiveAdvancesAsync(int employeeId);
        Task<List<AdvanceDeduction>> GetDeductionHistoryAsync(int advanceId);
        Task<decimal> CalculateMaximumDeductionAsync(int employeeId, decimal preliminaryNetPay);
        Task<List<AdvanceDeduction>> ProcessAdvanceDeductionsAsync(int employeeId, int payrollId, decimal maxDeductionAmount);
        Task ApplyDeductionToAdvancesAsync(int employeeId, decimal deductionAmount);
        Task<List<Advance>> GetAllWithDeductionsAsync();
        Task DeleteDeductionAsync(int deductionId);
    }

    public class AdvanceService : IAdvanceService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AdvanceService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Advance>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Advance>> GetByEmployeeIdAsync(int employeeId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteDeductionAsync(int deductionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var deduction = await context.AdvanceDeductions.FindAsync(deductionId);
            if (deduction == null)
                return;

            var advance = await context.Advances.FindAsync(deduction.AdvanceId);
            if (advance != null)
            {
                advance.RemainingAmount += deduction.DeductedAmount;
            }

            context.AdvanceDeductions.Remove(deduction);
            await context.SaveChangesAsync();
        }

        public async Task<List<Advance>> GetActiveAdvancesForEmployeeAsync(int employeeId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                .Where(a => a.EmployeeId == employeeId &&
                           a.Status == AdvanceStatus.Active &&
                           a.RemainingAmount > 0)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<List<AdvanceDeduction>> GetDeductionsForPayroll(int payrollId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AdvanceDeductions
                .Where(a => a.PayrollId == payrollId)
                .Include(a => a.Advance)
                .ToListAsync();
        }

        public async Task<Advance> GetByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                    .ThenInclude(d => d.Payroll)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Advance> CreateAsync(Advance advance)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            advance.CreatedAt = DateTime.UtcNow;
            advance.RemainingAmount = advance.Amount;
            advance.Status = AdvanceStatus.Active;

            context.Advances.Add(advance);
            await context.SaveChangesAsync();
            return advance;
        }

        public async Task<Advance> UpdateAsync(Advance advance)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            context.Entry(advance).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return advance;
        }

        public async Task<List<Advance>> GetAllWithDeductionsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var advance = await context.Advances.FindAsync(id);
            if (advance == null) return false;

            var hasDeductions = await context.AdvanceDeductions
                .AnyAsync(d => d.AdvanceId == id);

            if (hasDeductions)
            {
                throw new InvalidOperationException("Cannot delete advance with existing deductions. Use cancel instead.");
            }

            context.Advances.Remove(advance);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAdvanceAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var advance = await context.Advances.FindAsync(id);
            if (advance == null) return false;

            advance.Status = AdvanceStatus.Cancelled;
            advance.CompletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalActiveAdvancesAsync(int employeeId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Advances
                .Where(a => a.EmployeeId == employeeId &&
                           a.Status == AdvanceStatus.Active)
                .SumAsync(a => a.RemainingAmount);
        }

        public async Task<List<AdvanceDeduction>> GetDeductionHistoryAsync(int advanceId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AdvanceDeductions
                .Include(d => d.Payroll)
                .Where(d => d.AdvanceId == advanceId)
                .OrderByDescending(d => d.DeductionDate)
                .ToListAsync();
        }

        public async Task<decimal> CalculateMaximumDeductionAsync(int employeeId, decimal preliminaryNetPay)
        {
            const decimal maxDeductionPercentage = 1m;
            return Math.Max(0, preliminaryNetPay * maxDeductionPercentage);
        }

        public async Task<List<AdvanceDeduction>> ProcessAdvanceDeductionsAsync(int employeeId, int payrollId, decimal maxDeductionAmount)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var executionStrategy = context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var activeAdvances = await context.Advances
                        .Include(a => a.Employee)
                        .Include(a => a.Deductions)
                        .Where(a => a.EmployeeId == employeeId &&
                                   a.Status == AdvanceStatus.Active &&
                                   a.RemainingAmount > 0)
                        .OrderBy(a => a.Date)
                        .ToListAsync();

                    var deductions = new List<AdvanceDeduction>();
                    var remainingDeductionAmount = maxDeductionAmount;

                    foreach (var advance in activeAdvances)
                    {
                        if (remainingDeductionAmount <= 0) break;

                        var deductionAmount = Math.Min(advance.RemainingAmount, remainingDeductionAmount);

                        if (deductionAmount > 0)
                        {
                            var deduction = new AdvanceDeduction
                            {
                                AdvanceId = advance.Id,
                                PayrollId = payrollId,
                                DeductedAmount = deductionAmount,
                                DeductionDate = DateTime.UtcNow,
                            };

                            context.AdvanceDeductions.Add(deduction);
                            deductions.Add(deduction);

                            advance.RemainingAmount -= deductionAmount;

                            if (advance.RemainingAmount <= 0)
                            {
                                advance.Status = AdvanceStatus.Completed;
                                advance.CompletedAt = DateTime.UtcNow;
                            }

                            context.Entry(advance).State = EntityState.Modified;
                            remainingDeductionAmount -= deductionAmount;
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return deductions;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task ApplyDeductionToAdvancesAsync(int employeeId, decimal deductionAmount)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var executionStrategy = context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    var activeAdvances = await context.Advances
                        .Include(a => a.Employee)
                        .Include(a => a.Deductions)
                        .Where(a => a.EmployeeId == employeeId &&
                                   a.Status == AdvanceStatus.Active &&
                                   a.RemainingAmount > 0)
                        .OrderBy(a => a.Date)
                        .ToListAsync();

                    decimal remainingDeduction = deductionAmount;

                    foreach (var advance in activeAdvances)
                    {
                        if (remainingDeduction <= 0) break;

                        decimal amountToDeduct = Math.Min(advance.RemainingAmount, remainingDeduction);
                        advance.RemainingAmount -= amountToDeduct;

                        if (advance.RemainingAmount <= 0)
                        {
                            advance.Status = AdvanceStatus.Completed;
                            advance.CompletedAt = DateTime.UtcNow;
                        }
                        context.Entry(advance).State = EntityState.Modified;
                        remainingDeduction -= amountToDeduct;
                    }
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<Advance>> GetActiveAdvancesByEmployeeIdAsync(int employeeId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                .Where(a => a.EmployeeId == employeeId &&
                           a.Status == AdvanceStatus.Active &&
                           a.RemainingAmount > 0)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }
    }
}