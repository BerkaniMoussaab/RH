using RH.Models;
using RH.Models.Attendance;

namespace RH.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service principal de gestion de la ponctualité
    /// </summary>
    public interface IPunctualityManagementService
    {
        /// <summary>
        /// Récupère toutes les règles de bonus disponibles
        /// </summary>
        /// <returns>Liste des règles de bonus</returns>
        Task<List<PayrollAdjustmentRule>> GetAvailableBonusRulesAsync();

        /// <summary>
        /// Récupère toutes les règles de déduction disponibles
        /// </summary>
        /// <returns>Liste des règles de déduction</returns>
        Task<List<PayrollAdjustmentRule>> GetAvailableDeductionRulesAsync();

        /// <summary>
        /// Traite un fichier Excel de présence et retourne les résultats
        /// </summary>
        /// <param name="excelStream">Stream du fichier Excel</param>
        /// <param name="fileName">Nom du fichier</param>
        /// <param name="columnMapping">Configuration des colonnes</param>
        /// <param name="thresholds">Seuils de ponctualité</param>
        /// <param name="bonusRule">Règle de bonus</param>
        /// <param name="deductionRule">Règle de déduction</param>
        /// <returns>Résultat du traitement</returns>
        Task<AttendanceProcessingResult> ProcessExcelFileAsync(
            Stream excelStream,
            string fileName,
            ColumnMapping columnMapping,
            PunctualityThresholds thresholds,
            PayrollAdjustmentRule bonusRule,
            PayrollAdjustmentRule deductionRule);

        /// <summary>
        /// Sauvegarde les règles appliquées sélectionnées
        /// </summary>
        /// <param name="selectedResults">Résultats sélectionnés</param>
        /// <param name="bonusRule">Règle de bonus</param>
        /// <param name="deductionRule">Règle de déduction</param>
        /// <returns>Nombre de règles sauvegardées</returns>
        Task<int> SaveAppliedRulesAsync(
            List<EmployeeAttendanceResult> selectedResults,
            PayrollAdjustmentRule bonusRule,
            PayrollAdjustmentRule deductionRule);

        /// <summary>
        /// Trouve des employés similaires par nom
        /// </summary>
        /// <param name="employeeName">Nom de l'employé à rechercher</param>
        /// <param name="similarityThreshold">Seuil de similarité (par défaut 80%)</param>
        /// <returns>Liste des employés similaires</returns>
        Task<List<Employee>> FindSimilarEmployeesAsync(string employeeName, double similarityThreshold = 80.0);

        /// <summary>
        /// Valide que les règles sélectionnées sont appropriées
        /// </summary>
        /// <param name="bonusRuleId">ID de la règle de bonus</param>
        /// <param name="deductionRuleId">ID de la règle de déduction</param>
        /// <returns>True si les règles sont valides</returns>
        Task<bool> ValidateRulesAsync(int bonusRuleId, int deductionRuleId);

        /// <summary>
        /// Récupère les règles appliquées récemment
        /// </summary>
        /// <param name="days">Nombre de jours à considérer (par défaut 30)</param>
        /// <returns>Liste des règles appliquées récentes</returns>
        Task<List<PayrollAppliedRule>> GetRecentAppliedRulesAsync(int days = 30);

        /// <summary>
        /// Récupère les statistiques de ponctualité
        /// </summary>
        /// <returns>Dictionnaire des statistiques</returns>
        Task<Dictionary<string, object>> GetStatisticsAsync();
    }
}

