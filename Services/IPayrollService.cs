using RH.Models;
using static PayrollService;

namespace RH.Services
{
    public interface IPayrollService
    {
        Task<List<Payroll>> GetAllAsync(DateTime? startDate, DateTime? endDate);
        Task<List<AdvanceDeduction>> ProcessAdvanceDeductionsForPayrollAsync(Payroll payroll);
        Task<List<PayrollAppliedRule>> GetAppliedRulesForEmployeeAsync(int employeeId);
        Task<AdvanceDeductionSummary> GetAdvanceDeductionSummaryAsync(int employeeId, decimal preliminaryNetPay);
        Task DeleteAppliedRule(int id);
        Task<List<PayrollAppliedRule>> GetAppliedRulesForEmployeeAsync(
            int employeeId, DateTime? fromDate, DateTime? toDate);
        Task<Payroll?> GetByIdAsync(int id);
        Task DeleteAsync(int payrollId, bool deleteAppliedRules);
        Task<Payroll> GeneratePayrollForEmployee(int employeeId, DateTime payDate);
        Task<Payroll> CreateAsync(Payroll payroll);
        Task<Payroll> UpdateAsync(Payroll payroll);
        Task<Payroll?> GetLastPayrollForEmployeeAsync(int employeeId);
        Task<int> GetPayrollCountAsync();
        Task ApplyRulesAsync(int payrollId, List<PayrollAppliedRule> rules);
    }

}