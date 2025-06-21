using RH.Models;

namespace RH.Services
{
    public interface IJobTitleService
    {
        Task<List<JobTitle>> GetAllAsync();
        Task<JobTitle?> GetByIdAsync(int id);
        Task<JobTitle> AddAsync(JobTitle jobTitle);
        Task UpdateAsync(JobTitle jobTitle);
        Task DeleteAsync(int id);
        Task<List<JobTitle>> GetAllWithLeavePoliciesAsync();
    }
}
