using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public class CompanyInfoService
    {
        private readonly ApplicationDbContext _context;

        public CompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyInfo> GetAsync()
        {
            var existing = await _context.CompanyInfos.FirstOrDefaultAsync();
            if (existing != null)
                return existing;

            var newInfo = new CompanyInfo
            {
                Name = string.Empty,
                Address = string.Empty,
                RC = string.Empty,
                NIF = string.Empty
            };

            _context.CompanyInfos.Add(newInfo);
            await _context.SaveChangesAsync();

            return newInfo;
        }

        public async Task SaveAsync(CompanyInfo info)
        {
            var existing = await _context.CompanyInfos.FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Name = info.Name;
                existing.Address = info.Address;
                existing.RC = info.RC;
                existing.NIF = info.NIF;
                existing.LogoBytes = info.LogoBytes;
                existing.LogoMimeType = info.LogoMimeType;
            }
            else
            {
                _context.CompanyInfos.Add(info);
            }

            await _context.SaveChangesAsync();
        }
    }

}
