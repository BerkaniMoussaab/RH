using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using RH.Models.Attendance;
using RH.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace RH.Services
{
    /// <summary>
    /// Service pour le traitement des données de présence
    /// </summary>
    public class AttendanceProcessingService : IAttendanceProcessingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IEmployeeService _employeeService;

        public AttendanceProcessingService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IEmployeeService employeeService)
        {
            _contextFactory = contextFactory;
            _employeeService = employeeService;
        }

        public async Task<AttendanceProcessingResult> ProcessAttendanceAsync(
            List<AttendanceRecord> attendanceRecords,
            PunctualityThresholds thresholds,
            PayrollAdjustmentRule bonusRule,
            PayrollAdjustmentRule deductionRule)
        {
            var result = new AttendanceProcessingResult();

            // Séparer les enregistrements valides et invalides
            var validRecords = attendanceRecords.Where(r => r.IsValid).ToList();
            result.InvalidRecords = attendanceRecords.Where(r => !r.IsValid).ToList();

            result.TotalRecordsProcessed = attendanceRecords.Count;
            result.ValidRecordsCount = validRecords.Count;
            result.InvalidRecordsCount = result.InvalidRecords.Count;

            // Grouper par employé
            var employeeGroups = validRecords.GroupBy(r => NormalizeName(r.EmployeeName));

            foreach (var group in employeeGroups)
            {
                var employeeResult = new EmployeeAttendanceResult
                {
                    EmployeeName = group.First().EmployeeName,
                    Records = group.ToList()
                };

                // Rechercher l'employé dans la base de données
                var employee = await FindEmployeeByNameAsync(group.Key);

                if (employee != null)
                {
                    employeeResult.EmployeeId = employee.Id;
                    employeeResult.HasMatchingEmployee = true;

                    // Analyser les arrivées pour chaque jour
                    var dailyAttendance = group.GroupBy(r => r.AttendanceDateTime.Date);

                    foreach (var dayGroup in dailyAttendance)
                    {
                        // Prendre la première arrivée de la journée
                        var firstArrival = dayGroup.OrderBy(r => r.AttendanceDateTime.TimeOfDay).First();
                        var arrivalTime = firstArrival.AttendanceDateTime.TimeOfDay;

                        // Calculer les minutes par rapport aux seuils
                        if (arrivalTime <= thresholds.EarlyThreshold)
                        {
                            // Arrivée précoce : calculer les minutes avant le seuil
                            var minutesEarly = (thresholds.EarlyThreshold - arrivalTime).TotalMinutes;
                            employeeResult.TotalEarlyMinutes +=(decimal) Math.Floor(minutesEarly); ;
                        }
                        // Vérifier si c'est une arrivée tardive
                        else if (arrivalTime > thresholds.LateThreshold)
                        {
                            // Arrivée tardive : calculer les minutes après le seuil
                            var minutesLate = (arrivalTime - thresholds.LateThreshold).TotalMinutes;
                            employeeResult.TotalLateMinutes += (decimal) Math.Floor(minutesLate);
                        }
                    }

                    // Calculer les montants
                    if (employeeResult.TotalEarlyMinutes > 0)
                    {
                        employeeResult.BonusAmount = CalculateAmount(bonusRule, employee, (int)employeeResult.TotalEarlyMinutes);
                    }

                    if (employeeResult.TotalLateMinutes > 0)
                    {
                        employeeResult.DeductionAmount = CalculateAmount(deductionRule, employee, (int)employeeResult.TotalLateMinutes);
                    }
                }
                else
                {
                    employeeResult.HasMatchingEmployee = false;
                    employeeResult.ValidationMessage = $"Aucun employé trouvé avec le nom: {employeeResult.EmployeeName}";
                    employeeResult.IsSelected = false;
                }

                result.EmployeeResults.Add(employeeResult);
            }

            return result;
        }

        public async Task<Employee?> FindEmployeeByNameAsync(string employeeName)
        {
            var normalizedSearchName = NormalizeName(employeeName);
            var employees = await _employeeService.GetAllAsync();

            return employees.FirstOrDefault(e => NormalizeName(e.FullName) == normalizedSearchName);
        }

        public string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return name.Trim()
                      .ToLowerInvariant()
                      .Replace("  ", " ") // Remplacer les doubles espaces par des espaces simples
                      .Replace(".", "")   // Supprimer les points
                      .Replace(",", "");  // Supprimer les virgules
        }

        public decimal CalculateAmount(PayrollAdjustmentRule rule, Employee employee, int quantity)
        {
            if (rule == null || employee == null)
                return 0m;

            // If the rule is percentage-based, calculate percentage of salary
            decimal baseAmount = rule.IsPercentage
                ? employee.BaseSalary * (rule.Amount / 100m)
                : rule.Amount;

            // If rule is countable, multiply by quantity (but guard against <= 0)
            if (rule.IsCountable && quantity > 0)
                return baseAmount * quantity;

            return baseAmount;
        }


        public async Task<List<PayrollAppliedRule>> CreateAppliedRulesAsync(
            List<EmployeeAttendanceResult> selectedResults,
            PayrollAdjustmentRule bonusRule,
            PayrollAdjustmentRule deductionRule)
        {
            var appliedRules = new List<PayrollAppliedRule>();
            var currentDate = DateTime.Now.Date;

            foreach (var result in selectedResults.Where(r => r.IsSelected && r.HasMatchingEmployee))
            {
                // Créer la règle de bonus si applicable
                if (result.TotalEarlyMinutes > 0 && result.BonusAmount > 0)
                {
                    // Vérifier les doublons
                    var bonusDuplicate = await CheckForDuplicateAsync(result.EmployeeId!.Value, bonusRule.Id, currentDate);

                    if (!bonusDuplicate)
                    {
                        appliedRules.Add(new PayrollAppliedRule
                        {
                            EmployeeId = result.EmployeeId.Value,
                            RuleId = bonusRule.Id,
                            Quantity = result.TotalEarlyMinutes,
                            Amount = bonusRule.Amount,
                            Date = currentDate,
                            Notes = $"Bonus ponctualité - {result.TotalEarlyMinutes} minute(s) d'arrivée précoce"
                        });
                    }
                }

                // Créer la règle de déduction si applicable
                if (result.TotalLateMinutes > 0 && result.DeductionAmount > 0)
                {
                    // Vérifier les doublons
                    var deductionDuplicate = await CheckForDuplicateAsync(result.EmployeeId!.Value, deductionRule.Id, currentDate);

                    if (!deductionDuplicate)
                    {
                        appliedRules.Add(new PayrollAppliedRule
                        {
                            EmployeeId = result.EmployeeId.Value,
                            RuleId = deductionRule.Id,
                            Quantity = result.TotalLateMinutes,
                            Amount = deductionRule.Amount,
                            Date = currentDate,
                            Notes = $"Déduction retard - {result.TotalLateMinutes} minute(s) de retard"
                        });
                    }
                }
            }

            return appliedRules;
        }

        public async Task<bool> CheckForDuplicateAsync(int employeeId, int ruleId, DateTime date)
        {
            using var context = _contextFactory.CreateDbContext();

            return await context.PayrollAppliedRules
                .AnyAsync(r => r.EmployeeId == employeeId &&
                              r.RuleId == ruleId &&
                              r.Date.HasValue &&
                              r.Date.Value.Date == date.Date);
        }

        public string GenerateInvalidRecordsCsv(List<AttendanceRecord> invalidRecords)
        {
            if (!invalidRecords.Any())
                return string.Empty;

            var csv = new StringBuilder();

            // En-têtes
            csv.AppendLine("Ligne,Nom Employé,Date/Heure Originale,Erreur");

            // Données
            foreach (var record in invalidRecords)
            {
                csv.AppendLine($"{record.RowNumber},\"{record.EmployeeName}\",\"{record.OriginalDateTimeString}\",\"{record.ValidationError}\"");
            }

            return csv.ToString();
        }
    }
}
