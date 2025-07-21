using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RH.Models;

namespace RH.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
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

        // Advance payment entities (already present in your context)
        public DbSet<Advance> Advances { get; set; }
        public DbSet<AdvanceDeduction> AdvanceDeductions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee-JobTitle (many-to-one)
            modelBuilder.Entity<JobTitle>()
                .HasOne(jt => jt.LeavePolicy)
                .WithMany(lp => lp.JobTitles)
                .HasForeignKey(jt => jt.LeavePolicyId)
                .OnDelete(DeleteBehavior.SetNull); // or .Restrict / .Cascade as needed

            // LeaveRequest-Employee (many-to-one)
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payroll-Employee (many-to-one)
            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payrolls)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // PerformanceReview-Employee (many-to-one)
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
                .OnDelete(DeleteBehavior.SetNull); // Or Restrict, depending on business logic

            // CORRECTED: Configure advance payment entities with NoAction for both foreign keys
            ConfigureAdvanceEntities(modelBuilder);
        }

        /// <summary>
        /// Configure advance payment entities and their relationships
        /// CORRECTED: Both foreign keys to AdvanceDeduction use NoAction to prevent multiple cascade paths
        /// </summary>
        private void ConfigureAdvanceEntities(ModelBuilder modelBuilder)
        {
            // Configure Advance entity
            modelBuilder.Entity<Advance>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.RemainingAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Reason)
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                // Configure relationship with Employee
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                // CORRECTED: Configure relationship with AdvanceDeductions using NoAction
                entity.HasMany(e => e.Deductions)
                    .WithOne(d => d.Advance)
                    .HasForeignKey(d => d.AdvanceId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Indexes for performance
                entity.HasIndex(e => e.EmployeeId)
                    .HasDatabaseName("IX_Advances_EmployeeId");
                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Advances_Status");
                entity.HasIndex(e => e.Date)
                    .HasDatabaseName("IX_Advances_Date");
            });

            // Configure AdvanceDeduction entity
            modelBuilder.Entity<AdvanceDeduction>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DeductedAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();



                entity.Property(e => e.DeductionDate)
                    .IsRequired();

                // CORRECTED: Configure relationship with Advance using NoAction
                entity.HasOne(e => e.Advance)
                    .WithMany(a => a.Deductions)
                    .HasForeignKey(e => e.AdvanceId)
                    .OnDelete(DeleteBehavior.NoAction);

                // CORRECTED: Configure relationship with Payroll using NoAction
                entity.HasOne(e => e.Payroll)
                    .WithMany(p => p.AdvanceDeductions)
                    .HasForeignKey(e => e.PayrollId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Indexes for performance
                entity.HasIndex(e => e.AdvanceId)
                    .HasDatabaseName("IX_AdvanceDeductions_AdvanceId");
                entity.HasIndex(e => e.PayrollId)
                    .HasDatabaseName("IX_AdvanceDeductions_PayrollId");
                entity.HasIndex(e => e.DeductionDate)
                    .HasDatabaseName("IX_AdvanceDeductions_DeductionDate");
            });
        }
    }
}

/*
CORRECTED CONFIGURATION NOTES:

1. MULTIPLE CASCADE PATHS RESOLVED: Both foreign keys pointing to AdvanceDeduction now use 
   DeleteBehavior.NoAction, eliminating the ambiguity that caused the SQL Server error.

2. CONSISTENT BEHAVIOR: Both Advance->AdvanceDeduction and Payroll->AdvanceDeduction relationships 
   now have the same delete behavior, providing predictable and consistent data management.

3. AUDIT TRAIL PRESERVATION: AdvanceDeduction records are never automatically deleted, ensuring 
   complete audit trail preservation for financial transactions.

4. APPLICATION RESPONSIBILITY: The application now has full control over AdvanceDeduction lifecycle,
   allowing for proper business rule enforcement when cleanup is needed.

IMPACT ON EXISTING RELATIONSHIPS:
- Employee->Advance: Still Cascade (advances are deleted when employee is deleted)
- Employee->Payroll: Still Cascade (payrolls are deleted when employee is deleted)
- Advance->AdvanceDeduction: Changed to NoAction (deductions preserved when advance is deleted)
- Payroll->AdvanceDeduction: NoAction (deductions preserved when payroll is deleted)

BUSINESS LOGIC IMPLICATIONS:
1. When an Employee is deleted:
   - Their Advances are deleted (cascade)
   - Their Payrolls are deleted (cascade)
   - Their AdvanceDeductions are preserved (no action from either path)

2. When an Advance is deleted/cancelled:
   - Associated AdvanceDeductions are preserved for audit trail

3. When a Payroll is deleted:
   - Associated AdvanceDeductions are preserved for audit trail

4. Manual cleanup of AdvanceDeductions must be handled by application logic when appropriate.

This configuration ensures the migration will succeed while maintaining data integrity and 
audit trail requirements for the advance payment system.
*/

