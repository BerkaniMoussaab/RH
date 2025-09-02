using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using System.Globalization;

namespace RH.Services
{
    public interface IStatisticsService
    {
        Task<EmployeeStatistics> GetEmployeeStatisticsAsync();
        Task<AdvanceStatistics> GetAdvanceStatisticsAsync();
        Task<PayrollStatistics> GetPayrollStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        // Méthodes supplémentaires requises par l'interface existante
        Task<decimal> GetAverageSalaryAsync();
        Task<Dictionary<string, int>> GetEmployeesByStatusAsync();
        Task<decimal> GetTotalAdvanceAmountAsync();
        Task<decimal> GetTotalRemainingAdvanceAmountAsync();
        Task<Dictionary<string, decimal>> GetMonthlyAdvancesAsync();
       
        Task<DashboardStatistics> GetDashboardStatisticsAsync();
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeStatistics> GetEmployeeStatisticsAsync()
        {
            var employeeStats = new EmployeeStatistics();

            // Récupération de tous les employés
            var employees = await _context.Employees.ToListAsync();

            // Calcul du nombre total d'employés
            employeeStats.TotalEmployees = employees.Count;

            // Calcul du salaire moyen
            if (employees.Any())
            {
                employeeStats.AverageSalary = employees.Average(e => e.BaseSalary);
            }

            // Répartition des employés par statut - Conversion d'enum vers string
            var statusGroups = employees
                .GroupBy(e => e.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            employeeStats.EmployeesByStatus = statusGroups;

            // Si aucun statut n'est défini, créer des données par défaut
            if (!employeeStats.EmployeesByStatus.Any())
            {
                employeeStats.EmployeesByStatus = new Dictionary<string, int>
                {
                    { "Active", employees.Count },
                    { "Inactive", 0 },
                    { "OnLeave", 0 }
                };
            }

            return employeeStats;
        }

        public async Task<AdvanceStatistics> GetAdvanceStatisticsAsync()
        {
            var advanceStats = new AdvanceStatistics();

            // Récupération de toutes les avances
            var advances = await _context.Advances.ToListAsync();

            // Calcul du nombre total d'avances
            advanceStats.TotalAdvances = advances.Count;

            // Calcul du montant total des avances
            advanceStats.TotalAmount = advances.Sum(a => a.Amount);

            // Calcul du montant restant à payer
            advanceStats.TotalRemaining = advances.Sum(a => a.RemainingAmount);

            // Répartition des avances par statut - Conversion d'enum vers string
            var statusGroups = advances
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            advanceStats.AdvancesByStatus = statusGroups;

            // Si aucun statut n'est défini, créer des données par défaut
            if (!advanceStats.AdvancesByStatus.Any())
            {
                advanceStats.AdvancesByStatus = new Dictionary<string, int>
                {
                    { "Pending", 0 },
                    { "Approved", 0 },
                    { "Paid", 0 }
                };
            }

            // Calcul de l'évolution mensuelle des avances - Utilisation de CreatedAt au lieu de RequestDate
            var monthlyData = advances
                .GroupBy(a => new { a.Date.Year, a.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(a => a.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            advanceStats.MonthlyAdvances = new Dictionary<string, decimal>();
            foreach (var item in monthlyData)
            {
                var monthName = new DateTime(item.Year, item.Month, 1)
                    .ToString("MMMM yyyy", new CultureInfo("fr-DZ"));
                advanceStats.MonthlyAdvances[monthName] = item.TotalAmount;
            }


            // Si aucune donnée mensuelle, créer des données par défaut pour les 6 derniers mois
            if (!advanceStats.MonthlyAdvances.Any())
            {
                var currentDate = DateTime.Now;
                for (int i = 5; i >= 0; i--)
                {
                    var monthDate = currentDate.AddMonths(-i);
                    var monthName = monthDate.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-DZ"));

                    advanceStats.MonthlyAdvances[monthName] = 0;
                }
            }

            return advanceStats;
        }

        public async Task<PayrollStatistics> GetPayrollStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var payrollStats = new PayrollStatistics();

            // Construction de la requête avec filtres de date
            var query = _context.Payrolls.Include(p => p.Employee).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(p => p.PayDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.PayDate <= endDate.Value);
            }

            var payrolls = await query.ToListAsync();

            if (!payrolls.Any())
            {
                // Si aucune donnée de paie, retourner des statistiques vides
                return payrollStats;
            }

            // Calcul de la masse salariale totale (BaseSalary des employés)
            payrollStats.TotalGrossSalary = payrolls.Sum(p => p.Employee?.BaseSalary ?? p.BaseSalary);

            // Calcul du total des déductions
            payrollStats.TotalDeductions = payrolls.Sum(p => p.Deductions);

            // Calcul du total des bonus
            payrollStats.TotalBonuses = payrolls.Sum(p => p.Bonus);

            // Calcul du salaire net total
            payrollStats.TotalNetPay = payrolls.Sum(p => p.NetPay);

            // Calcul du total des avances déduites
            payrollStats.TotalAdvanceDeductions = payrolls.Sum(p => p.AdvanceDeductionsAmounts);

            // Calcul du total des déductions d'absence
            payrollStats.TotalAbsenceDeductions = payrolls.Sum(p => p.AbsenceDeduction);

           

            // Calcul des moyennes
            var uniqueEmployees = payrolls.Select(p => p.EmployeeId).Distinct().Count();
            if (uniqueEmployees > 0)
            {
                payrollStats.AverageGrossSalary = payrollStats.TotalGrossSalary / uniqueEmployees;
                payrollStats.AverageNetSalary = payrollStats.TotalNetPay / uniqueEmployees;
            }

            // Calcul de l'évolution mensuelle de la masse salariale brute
            var monthlyGrossData = payrolls
                .GroupBy(p => new { p.PayDate.Year, p.PayDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalGrossSalary = g.Sum(p => p.Employee?.BaseSalary ?? p.BaseSalary)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            payrollStats.MonthlyGrossSalary = new Dictionary<string, decimal>();
            foreach (var item in monthlyGrossData)
            {
                var monthName = new DateTime(item.Year, item.Month, 1)
                    .ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-DZ"));
                payrollStats.MonthlyGrossSalary[monthName] = item.TotalGrossSalary;
            }


            // Calcul de l'évolution mensuelle du salaire net
            var monthlyNetData = payrolls
                .GroupBy(p => new { p.PayDate.Year, p.PayDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalNetSalary = g.Sum(p => p.NetPay)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            payrollStats.MonthlyNetSalary = new Dictionary<string, decimal>();
            foreach (var item in monthlyNetData)
            {
                var monthName = new DateTime(item.Year, item.Month, 1)
                    .ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-DZ"));
                payrollStats.MonthlyNetSalary[monthName] = item.TotalNetSalary;
            }


            // Calcul de la répartition des paiements par type (Transaction vs Cash)
            var totalTransaction = payrolls.Sum(p => p.Transaction);
            var totalCash = payrolls.Sum(p => p.Cash);

            payrollStats.PaymentTypeDistribution = new Dictionary<string, decimal>
            {
                { "Virement", totalTransaction },
                { "Espèces", totalCash }
            };

            // Ajout des informations sur la période filtrée
            payrollStats.FilterStartDate = startDate;
            payrollStats.FilterEndDate = endDate;

            return payrollStats;
        }

        // Implémentation des méthodes supplémentaires requises par l'interface
        public async Task<decimal> GetAverageSalaryAsync()
        {
            var employees = await _context.Employees.ToListAsync();
            return employees.Any() ? employees.Average(e => e.BaseSalary) : 0;
        }

        public async Task<Dictionary<string, int>> GetEmployeesByStatusAsync()
        {
            var employees = await _context.Employees.ToListAsync();
            return employees
                .GroupBy(e => e.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<decimal> GetTotalAdvanceAmountAsync()
        {
            return await _context.Advances.SumAsync(a => a.Amount);
        }

        public async Task<decimal> GetTotalRemainingAdvanceAmountAsync()
        {
            return await _context.Advances.SumAsync(a => a.RemainingAmount);
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyAdvancesAsync()
        {
            var advances = await _context.Advances.ToListAsync();
            var monthlyData = advances
                .GroupBy(a => new { a.Date.Year, a.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(a => a.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            var result = new Dictionary<string, decimal>();
            foreach (var item in monthlyData)
            {
                var monthName = new DateTime(item.Year, item.Month, 1)
                    .ToString("MMMM yyyy", new CultureInfo("fr-DZ"));
                result[monthName] = item.TotalAmount;
            }

            return result;
        }

       

        public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
        {
            var employeeStats = await GetEmployeeStatisticsAsync();
            var advanceStats = await GetAdvanceStatisticsAsync();
            var payrollStats = await GetPayrollStatisticsAsync();

            return new DashboardStatistics
            {
                TotalEmployees = employeeStats.TotalEmployees,
                AverageSalary = employeeStats.AverageSalary,
                TotalAdvances = advanceStats.TotalAdvances,
                TotalAdvanceAmount = advanceStats.TotalAmount,
                TotalGrossSalary = payrollStats.TotalGrossSalary,
                TotalNetPay = payrollStats.TotalNetPay
            };
        }
    }
}

