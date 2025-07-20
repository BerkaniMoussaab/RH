using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }

    public class AdvanceService : IAdvanceService
    {
        private readonly ApplicationDbContext _context;

        public AdvanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Advance>> GetAllAsync()
        {
            return await _context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Advance>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Advance>> GetActiveAdvancesForEmployeeAsync(int employeeId)
        {
            return await _context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                .Where(a => a.EmployeeId == employeeId &&
                           a.Status == AdvanceStatus.Active &&
                           a.RemainingAmount > 0)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<Advance> GetByIdAsync(int id)
        {
            return await _context.Advances
                .Include(a => a.Employee)
                .Include(a => a.Deductions)
                    .ThenInclude(d => d.Payroll)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Advance> CreateAsync(Advance advance)
        {
            advance.CreatedAt = DateTime.UtcNow;
            advance.RemainingAmount = advance.Amount;
            advance.Status = AdvanceStatus.Active;

            _context.Advances.Add(advance);
            await _context.SaveChangesAsync();
            return advance;
        }

        public async Task<Advance> UpdateAsync(Advance advance)
        {
            _context.Entry(advance).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return advance;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var advance = await _context.Advances.FindAsync(id);
            if (advance == null) return false;

            var hasDeductions = await _context.AdvanceDeductions
                .AnyAsync(d => d.AdvanceId == id);

            if (hasDeductions)
            {
                throw new InvalidOperationException("Cannot delete advance with existing deductions. Use cancel instead.");
            }

            _context.Advances.Remove(advance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAdvanceAsync(int id)
        {
            var advance = await _context.Advances.FindAsync(id);
            if (advance == null) return false;

            advance.Status = AdvanceStatus.Cancelled;
            advance.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalActiveAdvancesAsync(int employeeId)
        {
            return await _context.Advances
                .Where(a => a.EmployeeId == employeeId &&
                           a.Status == AdvanceStatus.Active)
                .SumAsync(a => a.RemainingAmount);
        }

        public async Task<List<AdvanceDeduction>> GetDeductionHistoryAsync(int advanceId)
        {
            return await _context.AdvanceDeductions
                .Include(d => d.Payroll)
                .Where(d => d.AdvanceId == advanceId)
                .OrderByDescending(d => d.DeductionDate)
                .ToListAsync();
        }

        public async Task<decimal> CalculateMaximumDeductionAsync(int employeeId, decimal preliminaryNetPay)
        {
            const decimal maxDeductionPercentage = 0.5m;
            return Math.Max(0, preliminaryNetPay * maxDeductionPercentage);
        }

        public async Task<List<AdvanceDeduction>> ProcessAdvanceDeductionsAsync(int employeeId, int payrollId, decimal maxDeductionAmount)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var activeAdvances = await GetActiveAdvancesForEmployeeAsync(employeeId);
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

                            _context.AdvanceDeductions.Add(deduction);
                            deductions.Add(deduction);

                            advance.RemainingAmount -= deductionAmount;

                            if (advance.RemainingAmount <= 0)
                            {
                                advance.Status = AdvanceStatus.Completed;
                                advance.CompletedAt = DateTime.UtcNow;
                            }

                            _context.Entry(advance).State = EntityState.Modified;
                            remainingDeductionAmount -= deductionAmount;
                        }
                    }

                    await _context.SaveChangesAsync();
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
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var activeAdvances = await GetActiveAdvancesForEmployeeAsync(employeeId);
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
                        _context.Entry(advance).State = EntityState.Modified;
                        remainingDeduction -= amountToDeduct;
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
