using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public class JobTitleService : IJobTitleService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public JobTitleService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<JobTitle>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            // Ensure that we're not starting multiple queries at once
            return await context.JobTitles.ToListAsync();
        }

        public async Task<JobTitle?> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.JobTitles.FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<JobTitle> AddAsync(JobTitle jobTitle)
        {
            using var context = _contextFactory.CreateDbContext();
            context.JobTitles.Add(jobTitle);
            await context.SaveChangesAsync();
            return jobTitle;
        }

        public async Task UpdateAsync(JobTitle jobTitle)
        {
            using var context = _contextFactory.CreateDbContext();
            context.JobTitles.Update(jobTitle);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var jobTitle = await context.JobTitles.FindAsync(id);
            if (jobTitle != null)
            {
                context.JobTitles.Remove(jobTitle);
                await context.SaveChangesAsync();
            }
        }
        public async Task<List<JobTitle>> GetAllWithLeavePoliciesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.JobTitles.Include(jt => jt.LeavePolicy).ToListAsync();
        }
    }
}
