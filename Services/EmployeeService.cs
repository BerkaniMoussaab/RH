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
            return await context.Employees.Include(j => j.JobTitle).ToListAsync();
        }



        public async Task<Employee> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Employees.Where(p=>!p.Deleted)
                .Include(e => e.JobTitle) // Include the related JobTitle
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
            return (stream, $"contract_{employeeId}.pdf", "application/pdf");
        }

    }
}
