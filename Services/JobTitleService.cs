using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public class JobTitleService : IJobTitleService
    {
        private readonly ApplicationDbContext _context;

        public JobTitleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<JobTitle>> GetAllAsync()
        {
            // Ensure that we're not starting multiple queries at once
            return await _context.JobTitles.ToListAsync();
        }

        public async Task<JobTitle?> GetByIdAsync(int id)
        {
            return await _context.JobTitles.FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<JobTitle> AddAsync(JobTitle jobTitle)
        {
            _context.JobTitles.Add(jobTitle);
            await _context.SaveChangesAsync();
            return jobTitle;
        }

        public async Task UpdateAsync(JobTitle jobTitle)
        {
            _context.JobTitles.Update(jobTitle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var jobTitle = await _context.JobTitles.FindAsync(id);
            if (jobTitle != null)
            {
                _context.JobTitles.Remove(jobTitle);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<JobTitle>> GetAllWithLeavePoliciesAsync()
        {
            return await _context.JobTitles.Include(jt => jt.LeavePolicies).ToListAsync();
        }
    }
}
