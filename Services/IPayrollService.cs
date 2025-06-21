using RH.Models;
using static RH.Components.Pages.Payroll.PayrollList;

namespace RH.Services
{
    public interface IPayrollService
    {
        Task<List<Payroll>> GetAllAsync();
        Task<Payroll?> GetByIdAsync(int id);
        
        Task DeleteAsync(int id);
        Task<Payroll> GeneratePayrollForEmployee(int employeeId, DateTime payDate);
        Task<Payroll> CreateAsync(Payroll payroll, List<SelectableRule> bonuses, List<SelectableRule> deductions);
        Task<Payroll> UpdateAsync(Payroll payroll, List<SelectableRule> bonuses, List<SelectableRule> deductions);
        Task<int> GetPayrollCountAsync();
    }
}