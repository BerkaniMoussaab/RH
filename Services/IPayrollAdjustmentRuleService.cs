// Interfaces/IPayrollAdjustmentRuleService.cs
public interface IPayrollAdjustmentRuleService
{
    Task<List<PayrollAdjustmentRule>> GetAllAsync();
    Task<PayrollAdjustmentRule?> GetByIdAsync(int id);
    Task AddAsync(PayrollAdjustmentRule rule);
    Task UpdateAsync(PayrollAdjustmentRule rule);
    Task DeleteAsync(int id);
}