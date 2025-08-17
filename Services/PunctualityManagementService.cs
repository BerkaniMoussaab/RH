using Microsoft.EntityFrameworkCore;
using RH.Data;
using RH.Models;
using RH.Models.Attendance;
using RH.Services.Interfaces;
using RH.Extensions;

namespace RH.Services
{
    /// <summary>
    /// Service principal pour la gestion de la ponctualité
    /// Orchestre les opérations entre les différents services
    /// </summary>
    public class PunctualityManagementService : IPunctualityManagementService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IExcelReaderService _excelReaderService;
        private readonly IAttendanceProcessingService _attendanceProcessingService;
        private readonly IPayrollAdjustmentRuleService _payrollRuleService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<PunctualityManagementService> _logger;

        public PunctualityManagementService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IExcelReaderService excelReaderService,
            IAttendanceProcessingService attendanceProcessingService,
            IPayrollAdjustmentRuleService payrollRuleService,
            IEmployeeService employeeService,
            ILogger<PunctualityManagementService> logger)
        {
            _contextFactory = contextFactory;
            _excelReaderService = excelReaderService;
            _attendanceProcessingService = attendanceProcessingService;
            _payrollRuleService = payrollRuleService;
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task<List<PayrollAdjustmentRule>> GetAvailableBonusRulesAsync()
        {
            try
            {
                var allRules = await _payrollRuleService.GetAllAsync();
                return allRules.Where(r => r.Type == AdjustmentType.Bonus).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des règles de bonus");
                throw;
            }
        }

        public async Task<List<PayrollAdjustmentRule>> GetAvailableDeductionRulesAsync()
        {
            try
            {
                var allRules = await _payrollRuleService.GetAllAsync();
                return allRules.Where(r => r.Type == AdjustmentType.Deduction).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des règles de déduction");
                throw;
            }
        }

        public async Task<AttendanceProcessingResult> ProcessExcelFileAsync(
            Stream excelStream, 
            string fileName, 
            ColumnMapping columnMapping, 
            PunctualityThresholds thresholds,
            PayrollAdjustmentRule bonusRule,
            PayrollAdjustmentRule deductionRule)
        {
            try
            {
                _logger.LogInformation("Début du traitement du fichier Excel: {FileName}", fileName);

                // 1. Lire le fichier Excel
                var excelData = await _excelReaderService.ReadExcelFileAsync(excelStream, fileName);
                _logger.LogInformation("Fichier Excel lu avec succès. {RowCount} lignes trouvées", excelData.Rows.Count);

                // 2. Convertir en enregistrements d'attendance
                var attendanceRecords = await _excelReaderService.ConvertToAttendanceRecordsAsync(excelData, columnMapping);
                _logger.LogInformation("Conversion terminée. {ValidCount} enregistrements valides, {InvalidCount} invalides", 
                    attendanceRecords.Count(r => r.IsValid), 
                    attendanceRecords.Count(r => !r.IsValid));

                // 3. Traiter les données de présence
                var result = await _attendanceProcessingService.ProcessAttendanceAsync(
                    attendanceRecords, thresholds, bonusRule, deductionRule);

                _logger.LogInformation("Traitement terminé. {EmployeeCount} employés traités", result.EmployeeResults.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du fichier Excel: {FileName}", fileName);
                throw;
            }
        }

        public async Task<int> SaveAppliedRulesAsync(
            List<EmployeeAttendanceResult> selectedResults,
            PayrollAdjustmentRule bonusRule,
            PayrollAdjustmentRule deductionRule)
        {
            try
            {
                _logger.LogInformation("Début de la sauvegarde des règles appliquées pour {Count} employés", selectedResults.Count);

                var appliedRules = await _attendanceProcessingService.CreateAppliedRulesAsync(
                    selectedResults, bonusRule, deductionRule);

                if (!appliedRules.Any())
                {
                    _logger.LogWarning("Aucune règle à sauvegarder");
                    return 0;
                }

                using var context = _contextFactory.CreateDbContext();
                
                // Vérifier les doublons avant la sauvegarde
                var duplicateChecks = new List<Task<bool>>();
                foreach (var rule in appliedRules)
                {
                    duplicateChecks.Add(_attendanceProcessingService.CheckForDuplicateAsync(
                        rule.EmployeeId!.Value, rule.RuleId, rule.Date!.Value));
                }

                var duplicateResults = await Task.WhenAll(duplicateChecks);
                var rulesToSave = appliedRules.Where((rule, index) => !duplicateResults[index]).ToList();

                if (rulesToSave.Count != appliedRules.Count)
                {
                    _logger.LogWarning("Suppression de {Count} règles en doublon", appliedRules.Count - rulesToSave.Count);
                }

                if (rulesToSave.Any())
                {
                    context.PayrollAppliedRules.AddRange(rulesToSave);
                    await context.SaveChangesAsync();
                    
                    _logger.LogInformation("Sauvegarde réussie de {Count} règles appliquées", rulesToSave.Count);
                }

                return rulesToSave.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde des règles appliquées");
                throw;
            }
        }

        public async Task<List<Employee>> FindSimilarEmployeesAsync(string employeeName, double similarityThreshold = 80.0)
        {
            try
            {
                var allEmployees = await _employeeService.GetAllAsync();
                var normalizedSearchName = employeeName.NormalizeNameAdvanced();

                var similarEmployees = allEmployees
                    .Where(e => e.FullName.IsSimilarTo(employeeName, similarityThreshold))
                    .OrderByDescending(e => e.FullName.NormalizeNameAdvanced().SimilarityPercentage(normalizedSearchName))
                    .ToList();

                return similarEmployees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche d'employés similaires pour: {EmployeeName}", employeeName);
                throw;
            }
        }

        public async Task<bool> ValidateRulesAsync(int bonusRuleId, int deductionRuleId)
        {
            try
            {
                var bonusRule = await _payrollRuleService.GetByIdAsync(bonusRuleId);
                var deductionRule = await _payrollRuleService.GetByIdAsync(deductionRuleId);

                if (bonusRule == null || deductionRule == null)
                {
                    _logger.LogWarning("Règles non trouvées. Bonus: {BonusRuleId}, Déduction: {DeductionRuleId}", 
                        bonusRuleId, deductionRuleId);
                    return false;
                }

                if (bonusRule.Type != AdjustmentType.Bonus)
                {
                    _logger.LogWarning("La règle {RuleId} n'est pas de type Bonus", bonusRuleId);
                    return false;
                }

                if (deductionRule.Type != AdjustmentType.Deduction)
                {
                    _logger.LogWarning("La règle {RuleId} n'est pas de type Deduction", deductionRuleId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation des règles");
                return false;
            }
        }

        public async Task<List<PayrollAppliedRule>> GetRecentAppliedRulesAsync(int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-days);
                
                using var context = _contextFactory.CreateDbContext();
                
                var recentRules = await context.PayrollAppliedRules
                    .Include(r => r.Employee)
                    .Include(r => r.Rule)
                    .Where(r => r.Date.HasValue && r.Date.Value >= cutoffDate)
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();

                return recentRules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des règles appliquées récentes");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetStatisticsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var stats = new Dictionary<string, object>();

                // Statistiques générales
                stats["TotalEmployees"] = await context.Employees.CountAsync(e => !e.Deleted);
                stats["TotalRules"] = await context.PayrollAdjustmentRules.CountAsync();
                stats["TotalAppliedRules"] = await context.PayrollAppliedRules.CountAsync();

                // Statistiques des 30 derniers jours
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                stats["RecentAppliedRules"] = await context.PayrollAppliedRules
                    .CountAsync(r => r.Date.HasValue && r.Date.Value >= thirtyDaysAgo);

                // Statistiques par type de règle
                var bonusRulesCount = await context.PayrollAppliedRules
                    .Include(r => r.Rule)
                    .CountAsync(r => r.Rule.Type == AdjustmentType.Bonus && 
                                    r.Date.HasValue && r.Date.Value >= thirtyDaysAgo);

                var deductionRulesCount = await context.PayrollAppliedRules
                    .Include(r => r.Rule)
                    .CountAsync(r => r.Rule.Type == AdjustmentType.Deduction && 
                                    r.Date.HasValue && r.Date.Value >= thirtyDaysAgo);

                stats["RecentBonusRules"] = bonusRulesCount;
                stats["RecentDeductionRules"] = deductionRulesCount;

                // Montants totaux
                var totalBonusAmount = await context.PayrollAppliedRules
                    .Include(r => r.Rule)
                    .Where(r => r.Rule.Type == AdjustmentType.Bonus && 
                               r.Date.HasValue && r.Date.Value >= thirtyDaysAgo)
                    .SumAsync(r => r.Amount);

                var totalDeductionAmount = await context.PayrollAppliedRules
                    .Include(r => r.Rule)
                    .Where(r => r.Rule.Type == AdjustmentType.Deduction && 
                               r.Date.HasValue && r.Date.Value >= thirtyDaysAgo)
                    .SumAsync(r => r.Amount);

                stats["TotalBonusAmount"] = totalBonusAmount;
                stats["TotalDeductionAmount"] = totalDeductionAmount;
                stats["NetImpact"] = totalBonusAmount - totalDeductionAmount;

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques");
                throw;
            }
        }
    }
}

