namespace RH.Services
{
    using Microsoft.EntityFrameworkCore;
    using RH.Data;
    using RH.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class EmployeeService : IEmployeeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public EmployeeService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Employees.Where(e=>e.Deleted == false)
                .Include(e => e.JobTitle)
                .Include(e => e.FileAttachments)
                .ToListAsync();
        }



        public async Task<Employee> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Employees.Where(p=>!p.Deleted)
                .Include(e => e.JobTitle) // Include the related JobTitle
                .Include(e => e.FileAttachments) // Include file attachments
                .FirstOrDefaultAsync(e => e.Id == id); // Use FirstOrDefaultAsync instead of FindAsync
        }


        public async Task AddAsync(Employee employee)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Employees.Add(employee);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Employees.Update(employee);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var employee = await context.Employees.FindAsync(id);
            if (employee != null)
            {
                employee.Deleted = true;
                await context.SaveChangesAsync();
            }
        }


        public async Task<int> GetJobTitleCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.JobTitles.CountAsync();
        }
        public async Task<(Stream FileStream, string FileName, string ContentType)> GetContractFileAsync(int employeeId)
        {
            using var context = _contextFactory.CreateDbContext();
            var employee = await context.Employees.FindAsync(employeeId);

            if (employee?.ContractFile == null)
                return (null, null, null);

            var stream = new MemoryStream(employee.ContractFile);

            // Use original filename if available, otherwise generate fallback
            var fileName = !string.IsNullOrWhiteSpace(employee.ContractFileName)
                ? employee.ContractFileName
                : $"contract_{employeeId}";

            // Ensure the file has an extension
            if (!Path.HasExtension(fileName) && !string.IsNullOrWhiteSpace(employee.ContractContentType))
            {
                var extension = employee.ContractContentType switch
                {
                    "application/pdf" => ".pdf",
                    "application/msword" => ".doc",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                    "image/png" => ".png",
                    "image/jpeg" => ".jpg",
                    _ => ".bin"
                };

                fileName += extension;
            }

            var contentType = !string.IsNullOrWhiteSpace(employee.ContractContentType)
                ? employee.ContractContentType
                : "application/octet-stream";

            return (stream, fileName, contentType);
        }


    }
}
