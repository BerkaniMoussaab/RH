using RH.Models;

namespace RH.Services
{
    public interface IPayrollService
    {
        Task<List<Payroll>> GetAllAsync(DateTime? startDate, DateTime? endDate);
        

        Task<Payroll?> GetByIdAsync(int id);
        void DeleteAsync(int id , bool deleteAppliedRules);
        Task<Payroll> GeneratePayrollForEmployee(int employeeId, DateTime payDate);
        Task<Payroll> CreateAsync(Payroll payroll);
        Task<Payroll> UpdateAsync(Payroll payroll);
        Task<Payroll?> GetLastPayrollForEmployeeAsync(int employeeId);
        Task<int> GetPayrollCountAsync();
        Task ApplyRulesAsync(int payrollId, List<PayrollAppliedRule> rules);
    }

}