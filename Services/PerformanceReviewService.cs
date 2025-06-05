using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public class PerformanceReviewService : IPerformanceReviewService
    {
        private readonly ApplicationDbContext _context;

        public PerformanceReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PerformanceReview>> GetAllAsync() =>
            await _context.PerformanceReviews.Include(r => r.Employee).ToListAsync();

        public async Task<PerformanceReview?> GetByIdAsync(int id) =>
            await _context.PerformanceReviews.Include(r => r.Employee).FirstOrDefaultAsync(r => r.Id == id);

        public async Task<PerformanceReview> CreateAsync(PerformanceReview review)
        {
            _context.PerformanceReviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task UpdateAsync(PerformanceReview review)
        {
            _context.PerformanceReviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.PerformanceReviews.FindAsync(id);
            if (entity != null)
            {
                _context.PerformanceReviews.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
