namespace RH.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using RH.Data;
    using RH.Models;

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
            return await context.Employees.FindAsync(id);
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
                context.Employees.Remove(employee);
                await context.SaveChangesAsync();
            }
        }
       

        public async Task<int> GetJobTitleCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.JobTitles.CountAsync();
        }
    }
}
