using RH.Data;
using Microsoft.EntityFrameworkCore;
using RH.Models;

namespace RH.Services
{
    public class LeavePolicyService : ILeavePolicyService
    {
        private readonly ApplicationDbContext _context;

        public LeavePolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LeavePolicy> CreateAsync(LeavePolicy policy)
        {
            _context.LeavePolicies.Add(policy);
            await _context.SaveChangesAsync();
            return policy;
        }

        public async Task AssignToJobTitleAsync(int policyId, int jobTitleId)
        {
            var jobTitle = await _context.JobTitles
                .Include(j => j.LeavePolicies)
                .FirstOrDefaultAsync(j => j.Id == jobTitleId);

            var policy = await _context.LeavePolicies.FindAsync(policyId);

            if (jobTitle != null && policy != null && !jobTitle.LeavePolicies.Contains(policy))
            {
                jobTitle.LeavePolicies.Add(policy);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<LeavePolicy>> GetAllAsync()
        {
            return await _context.LeavePolicies.Include(lp => lp.JobTitles).ToListAsync();
        }
    }

}
