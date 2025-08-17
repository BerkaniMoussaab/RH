using RH.Models.Attendance;

namespace RH.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service de lecture de fichiers Excel
    /// </summary>
    public interface IExcelReaderService
    {
        /// <summary>
        /// Lit un fichier Excel et retourne les données brutes
        /// </summary>
        /// <param name="stream">Stream du fichier Excel</param>
        /// <param name="fileName">Nom du fichier</param>
        /// <returns>Données Excel brutes</returns>
        Task<ExcelData> ReadExcelFileAsync(Stream stream, string fileName);

        /// <summary>
        /// Convertit les données Excel en enregistrements d'attendance
        /// </summary>
        /// <param name="excelData">Données Excel brutes</param>
        /// <param name="columnMapping">Configuration des colonnes</param>
        /// <returns>Liste des enregistrements d'attendance</returns>
        Task<List<AttendanceRecord>> ConvertToAttendanceRecordsAsync(ExcelData excelData, ColumnMapping columnMapping);

        /// <summary>
        /// Valide le format de date et tente de parser
        /// </summary>
        /// <param name="dateString">Chaîne de date à parser</param>
        /// <param name="format">Format de date attendu</param>
        /// <returns>DateTime parsé ou null si échec</returns>
        DateTime? TryParseDateTime(string dateString, string format);

        /// <summary>
        /// Détecte automatiquement le format de date le plus probable
        /// </summary>
        /// <param name="dateStrings">Échantillon de chaînes de date</param>
        /// <returns>Format de date détecté</returns>
        string DetectDateFormat(IEnumerable<string> dateStrings);
    }
}

