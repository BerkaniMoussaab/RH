using System.ComponentModel.DataAnnotations;

namespace RH.Models.Attendance
{
    /// <summary>
    /// Modèle pour représenter une ligne de données Excel d'attendance
    /// </summary>
    public class AttendanceRecord
    {
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime AttendanceDateTime { get; set; }
        public string OriginalDateTimeString { get; set; } = string.Empty;
        public int RowNumber { get; set; }
        public bool IsValid { get; set; } = true;
        public string ValidationError { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modèle pour les données Excel brutes
    /// </summary>
    public class ExcelData
    {
        public List<List<string>> Rows { get; set; } = new();
        public List<string> Headers { get; set; } = new();
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modèle pour la configuration des colonnes
    /// </summary>
    public class ColumnMapping
    {
        public int NameColumnIndex { get; set; } = -1;
        public int DateTimeColumnIndex { get; set; } = -1;
        public string DateFormat { get; set; } = "dd/MM/yyyy HH:mm";
    }

    /// <summary>
    /// Modèle pour les seuils de ponctualité
    /// </summary>
    public class PunctualityThresholds
    {
        [Required]
        public TimeSpan EarlyThreshold { get; set; } = new TimeSpan(8, 0, 0); // 08:00
        
        [Required]
        public TimeSpan LateThreshold { get; set; } = new TimeSpan(8, 30, 0); // 08:30
        
        public int BonusRuleId { get; set; }
        public int DeductionRuleId { get; set; }
    }

    /// <summary>
    /// Modèle pour le résultat du traitement d'un employé
    /// </summary>
    public class EmployeeAttendanceResult
    {
        public string EmployeeName { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public List<AttendanceRecord> Records { get; set; } = new();
        public int EarlyArrivals { get; set; }
        public int LateArrivals { get; set; }
        public bool HasMatchingEmployee { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = true;
        public decimal BonusAmount { get; set; }
        public decimal DeductionAmount { get; set; }
    }

    /// <summary>
    /// Modèle pour le résultat complet du traitement
    /// </summary>
    public class AttendanceProcessingResult
    {
        public List<EmployeeAttendanceResult> EmployeeResults { get; set; } = new();
        public List<AttendanceRecord> InvalidRecords { get; set; } = new();
        public int TotalRecordsProcessed { get; set; }
        public int ValidRecordsCount { get; set; }
        public int InvalidRecordsCount { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Modèle pour les formats de date supportés
    /// </summary>
    public static class SupportedDateFormats
    {
        public static readonly List<string> Formats = new()
        {
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy HH:mm:ss",
            "MM/dd/yyyy HH:mm",
            "MM/dd/yyyy HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "dd-MM-yyyy HH:mm",
            "dd-MM-yyyy HH:mm:ss",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "yyyy-MM-dd",
            "dd-MM-yyyy"
        };

        public static readonly Dictionary<string, string> FormatDescriptions = new()
        {
            { "dd/MM/yyyy HH:mm", "Jour/Mois/Année Heure:Minute (ex: 15/03/2024 08:30)" },
            { "dd/MM/yyyy HH:mm:ss", "Jour/Mois/Année Heure:Minute:Seconde (ex: 15/03/2024 08:30:45)" },
            { "MM/dd/yyyy HH:mm", "Mois/Jour/Année Heure:Minute (ex: 03/15/2024 08:30)" },
            { "MM/dd/yyyy HH:mm:ss", "Mois/Jour/Année Heure:Minute:Seconde (ex: 03/15/2024 08:30:45)" },
            { "yyyy-MM-dd HH:mm", "Année-Mois-Jour Heure:Minute (ex: 2024-03-15 08:30)" },
            { "yyyy-MM-dd HH:mm:ss", "Année-Mois-Jour Heure:Minute:Seconde (ex: 2024-03-15 08:30:45)" },
            { "dd-MM-yyyy HH:mm", "Jour-Mois-Année Heure:Minute (ex: 15-03-2024 08:30)" },
            { "dd-MM-yyyy HH:mm:ss", "Jour-Mois-Année Heure:Minute:Seconde (ex: 15-03-2024 08:30:45)" },
            { "dd/MM/yyyy", "Jour/Mois/Année (ex: 15/03/2024)" },
            { "MM/dd/yyyy", "Mois/Jour/Année (ex: 03/15/2024)" },
            { "yyyy-MM-dd", "Année-Mois-Jour (ex: 2024-03-15)" },
            { "dd-MM-yyyy", "Jour-Mois-Année (ex: 15-03-2024)" }
        };
    }
}

