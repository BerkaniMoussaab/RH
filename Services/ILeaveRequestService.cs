using RH.Models;

namespace RH.Services
{
    public interface ILeaveRequestService
    {
        Task<List<LeaveRequest>> GetAllAsync();
        Task<LeaveRequest?> GetByIdAsync(int id);
        Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest);
        Task UpdateStatusAsync(int id , LeaveStatus status);
        Task UpdateAsync(LeaveRequest leaveRequest);
        Task DeleteAsync(int id);
    }
}
