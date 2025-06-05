using RH.Models;

namespace RH.Services
{
    public interface IPerformanceReviewService
    {
        Task<IEnumerable<PerformanceReview>> GetAllAsync();
        Task<PerformanceReview?> GetByIdAsync(int id);
        Task<PerformanceReview> CreateAsync(PerformanceReview review);
        Task UpdateAsync(PerformanceReview review);
        Task DeleteAsync(int id);
    }
}
