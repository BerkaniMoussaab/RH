using RH.Models;

namespace RH.Services
{
    public interface IPayrollService
    {
        Task<List<Payroll>> GetAllAsync();
        Task<Payroll?> GetByIdAsync(int id);
        Task<Payroll> CreateAsync(Payroll payroll);
        Task UpdateAsync(Payroll payroll);
        Task DeleteAsync(int id);
        Task<Payroll> GeneratePayrollForEmployee(int employeeId, DateTime payDate);
    }
}