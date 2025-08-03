using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RH.Models;

namespace RH.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<MonthlyAttendanceSummary> MonthlyAttendanceSummaries { get; set; }
        public DbSet<PayrollAdjustmentRule> PayrollAdjustmentRules { get; set; }
        public DbSet<PayrollAppliedRule> PayrollAppliedRules { get; set; }
        public DbSet<AbsenceRecord> AbsenceRecords { get; set; }
        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        public DbSet<WorkedDayOff> WorkedDaysOff { get; set; }
        public DbSet<RecoveryDay> RecoveryDays { get; set; }
        public DbSet<CompanyInfo> CompanyInfos { get; set; }

        public DbSet<Advance> Advances { get; set; }
        public DbSet<AdvanceDeduction> AdvanceDeductions { get; set; }
        public DbSet<RecoveryDayUsage> RecoveryDayUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply default decimal precision (18,2) globally
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(18);
                        property.SetScale(2);
                    }
                }
            }

            modelBuilder.Entity<JobTitle>()
                .HasOne(jt => jt.LeavePolicy)
                .WithMany(lp => lp.JobTitles)
                .HasForeignKey(jt => jt.LeavePolicyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payrolls)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PerformanceReview>()
                .HasOne(r => r.Employee)
                .WithMany(e => e.PerformanceReviews)
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PayrollAdjustmentRule>()
                .HasMany(r => r.JobTitles)
                .WithMany(j => j.PayrollAdjustmentRules);

            modelBuilder.Entity<AbsenceRecord>()
                .HasOne(a => a.Payroll)
                .WithMany(p => p.Absences)
                .HasForeignKey(a => a.PayrollId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RecoveryDayUsage>()
                .HasOne(ru => ru.RecoveryDay)
                .WithMany()
                .HasForeignKey(ru => ru.RecoveryDayId)
                .OnDelete(DeleteBehavior.Cascade);

            ConfigureAdvanceEntities(modelBuilder);
        }

        private void ConfigureAdvanceEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Advance>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Deductions)
                    .WithOne(d => d.Advance)
                    .HasForeignKey(d => d.AdvanceId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.EmployeeId).HasDatabaseName("IX_Advances_EmployeeId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Advances_Status");
                entity.HasIndex(e => e.Date).HasDatabaseName("IX_Advances_Date");
            });

            modelBuilder.Entity<AdvanceDeduction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DeductionDate).IsRequired();

                entity.HasOne(e => e.Advance)
                    .WithMany(a => a.Deductions)
                    .HasForeignKey(e => e.AdvanceId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Payroll)
                    .WithMany(p => p.AdvanceDeductions)
                    .HasForeignKey(e => e.PayrollId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.AdvanceId).HasDatabaseName("IX_AdvanceDeductions_AdvanceId");
                entity.HasIndex(e => e.PayrollId).HasDatabaseName("IX_AdvanceDeductions_PayrollId");
                entity.HasIndex(e => e.DeductionDate).HasDatabaseName("IX_AdvanceDeductions_DeductionDate");
            });
        }
    }
}
