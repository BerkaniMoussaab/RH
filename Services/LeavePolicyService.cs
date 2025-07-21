using Microsoft.EntityFrameworkCore;
using RH.Data;
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

        // ✅ Updated for one LeavePolicy per JobTitle (many-to-one)
        public async Task AssignToJobTitleAsync(int policyId, int jobTitleId)
        {
            var jobTitle = await _context.JobTitles.FirstOrDefaultAsync(j => j.Id == jobTitleId);
            if (jobTitle == null) return;

            var policy = await _context.LeavePolicies.FindAsync(policyId);
            if (policy == null) return;

            jobTitle.LeavePolicyId = policy.Id;
            await _context.SaveChangesAsync();
        }

        public async Task<List<LeavePolicy>> GetAllAsync()
        {
            return await _context.LeavePolicies
                .Include(lp => lp.JobTitles) // optional if you want reverse nav
                .ToListAsync();
        }
    }
}
