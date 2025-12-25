using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;

namespace RH.Services
{
    public interface IFileAttachmentService
    {
        Task<FileAttachment?> GetFileAttachmentAsync(int id);
        Task<IEnumerable<FileAttachment>> GetFileAttachmentsByEmployeeIdAsync(int employeeId);
        Task<FileAttachment> CreateFileAttachmentAsync(FileAttachment fileAttachment);
        Task DeleteFileAttachmentAsync(int id);
        Task<IEnumerable<FileAttachment>> GetFileAttachmentsWithEmployeeAsync();
    }

    public class FileAttachmentService : IFileAttachmentService
    {
        private readonly ApplicationDbContext _context;

        public FileAttachmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FileAttachment?> GetFileAttachmentAsync(int id)
        {
            return await _context.FileAttachments.FindAsync(id);
        }

        public async Task<IEnumerable<FileAttachment>> GetFileAttachmentsByEmployeeIdAsync(int employeeId)
        {
            return await _context.FileAttachments
                .Where(f => f.EmployeeId == employeeId)
                .Include(f => f.Employee)
                .ToListAsync();
        }

        public async Task<FileAttachment> CreateFileAttachmentAsync(FileAttachment fileAttachment)
        {
            _context.FileAttachments.Add(fileAttachment);
            await _context.SaveChangesAsync();
            return fileAttachment;
        }

        public async Task DeleteFileAttachmentAsync(int id)
        {
            var fileAttachment = await _context.FileAttachments.FindAsync(id);
            if (fileAttachment != null)
            {
                _context.FileAttachments.Remove(fileAttachment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<FileAttachment>> GetFileAttachmentsWithEmployeeAsync()
        {
            return await _context.FileAttachments
                .Include(f => f.Employee)
                .ToListAsync();
        }
    }
}
