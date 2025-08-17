using RH.Models;
using RH.Models.Attendance;

namespace RH.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service de traitement des données de présence
    /// </summary>
    public interface IAttendanceProcessingService
    {
        /// <summary>
        /// Traite les enregistrements d'attendance et applique les seuils de ponctualité
        /// </summary>
        /// <param name="attendanceRecords">Enregistrements d'attendance</param>
        /// <param name="thresholds">Seuils de ponctualité</param>
        /// <param name="bonusRule">Règle de bonus</param>
        /// <param name="deductionRule">Règle de déduction</param>
        /// <returns>Résultat du traitement</returns>
        Task<AttendanceProcessingResult> ProcessAttendanceAsync(
            List<AttendanceRecord> attendanceRecords,
            PunctualityThresholds thresholds,
            PayrollAdjustmentRule bonusRule,
            PayrollAdjustmentRule deductionRule);

        /// <summary>
        /// Trouve un employé par nom en normalisant les différences de casse et espaces
        /// </summary>
        /// <param name="employeeName">Nom de l'employé à rechercher</param>
        /// <returns>Employé trouvé ou null</returns>
        Task<Employee?> FindEmployeeByNameAsync(string employeeName);

        /// <summary>
        /// Normalise un nom d'employé (supprime espaces, convertit en minuscules)
        /// </summary>
        /// <param name="name">Nom à normaliser</param>
        /// <returns>Nom normalisé</returns>
        string NormalizeName(string name);

        /// <summary>
        /// Calcule le montant du bonus ou de la déduction
        /// </summary>
        /// <param name="rule">Règle d'ajustement</param>
        /// <param name="employee">Employé</param>
        /// <param name="quantity">Quantité (nombre d'occurrences)</param>
        /// <returns>Montant calculé</returns>
        decimal CalculateAmount(PayrollAdjustmentRule rule, Employee employee, int quantity);

        /// <summary>
        /// Crée les PayrollAppliedRule pour les résultats sélectionnés
        /// </summary>
        /// <param name="selectedResults">Résultats sélectionnés</param>
        /// <param name="bonusRule">Règle de bonus</param>
        /// <param name="deductionRule">Règle de déduction</param>
        /// <returns>Liste des règles appliquées créées</returns>
        Task<List<PayrollAppliedRule>> CreateAppliedRulesAsync(
            List<EmployeeAttendanceResult> selectedResults,
            PayrollAdjustmentRule bonusRule,
            PayrollAdjustmentRule deductionRule);

        /// <summary>
        /// Vérifie s'il existe déjà une règle appliquée pour un employé, une règle et une date
        /// </summary>
        /// <param name="employeeId">ID de l'employé</param>
        /// <param name="ruleId">ID de la règle</param>
        /// <param name="date">Date</param>
        /// <returns>True si un doublon existe</returns>
        Task<bool> CheckForDuplicateAsync(int employeeId, int ruleId, DateTime date);

        /// <summary>
        /// Génère un fichier CSV des enregistrements invalides
        /// </summary>
        /// <param name="invalidRecords">Enregistrements invalides</param>
        /// <returns>Contenu CSV</returns>
        string GenerateInvalidRecordsCsv(List<AttendanceRecord> invalidRecords);
    }
}

