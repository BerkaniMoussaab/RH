using RH.Models;

namespace RH.Services
{
    public interface ILeaveRequestService
    {
        Task<List<LeaveRequest>> GetAllAsync();
        Task<LeaveRequest?> GetByIdAsync(int id);
        Task<LeaveRequest> CreateAsync(LeaveRequest request);
        Task<LeaveRequest> UpdateAsync(LeaveRequest request);
        Task<float> GetRemainingDaysAsync(int employeeId);
        Task UpdateStatusAsync(int requestId, LeaveStatus newStatus);

    }
}
