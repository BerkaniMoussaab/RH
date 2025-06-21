using RH.Models;

namespace RH.Services
{
    public interface ILeavePolicyService
    {
        Task<LeavePolicy> CreateAsync(LeavePolicy policy);
        Task AssignToJobTitleAsync(int policyId, int jobTitleId);
        Task<List<LeavePolicy>> GetAllAsync();
    }
}
