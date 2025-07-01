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
        Task<Payroll> CreateAsync(Payroll payroll);
        Task<Payroll> UpdateAsync(Payroll payroll);
        Task<Payroll?> GetLastPayrollForEmployeeAsync(int employeeId);
        Task<int> GetPayrollCountAsync();
    }

}