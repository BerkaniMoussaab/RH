using OfficeOpenXml;
using RH.Models.Attendance;
using RH.Services.Interfaces;
using System.Globalization;

namespace RH.Services
{
    /// <summary>
    /// Service pour la lecture de fichiers Excel
    /// </summary>
    public class ExcelReaderService : IExcelReaderService
    {
        public async Task<ExcelData> ReadExcelFileAsync(Stream stream, string fileName)
        {
            var excelData = new ExcelData { FileName = fileName };

            try
            {
                // Copier le flux en mémoire pour éviter l'erreur
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                ExcelPackage.License.SetNonCommercialPersonal("MOSSAAB");

                using var package = new ExcelPackage(memoryStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    throw new InvalidOperationException("Le fichier Excel ne contient aucune feuille de calcul.");

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                var colCount = worksheet.Dimension?.Columns ?? 0;

                if (rowCount == 0 || colCount == 0)
                    throw new InvalidOperationException("La feuille de calcul est vide.");

                // Lecture des en-têtes
                for (int col = 1; col <= colCount; col++)
                {
                    var headerValue = worksheet.Cells[1, col].Value?.ToString() ?? $"Colonne {col}";
                    excelData.Headers.Add(headerValue);
                }

                // Lecture des données
                for (int row = 2; row <= rowCount; row++)
                {
                    var rowData = new List<string>();
                    bool hasData = false;

                    for (int col = 1; col <= colCount; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? string.Empty;
                        rowData.Add(cellValue);

                        if (!string.IsNullOrWhiteSpace(cellValue))
                            hasData = true;
                    }

                    if (hasData)
                        excelData.Rows.Add(rowData);
                }

                return excelData;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la lecture du fichier Excel: {ex.Message}", ex);
            }
        }


        public async Task<List<AttendanceRecord>> ConvertToAttendanceRecordsAsync(ExcelData excelData, ColumnMapping columnMapping)
        {
            var records = new List<AttendanceRecord>();

            if (columnMapping.NameColumnIndex < 0 || columnMapping.DateTimeColumnIndex < 0)
            {
                throw new ArgumentException("Les indices des colonnes nom et date/heure doivent être spécifiés.");
            }

            for (int i = 0; i < excelData.Rows.Count; i++)
            {
                var row = excelData.Rows[i];
                var record = new AttendanceRecord
                {
                    RowNumber = i + 2 // +2 car on commence à la ligne 2 dans Excel
                };

                try
                {
                    // Extraction du nom
                    if (columnMapping.NameColumnIndex < row.Count)
                    {
                        record.EmployeeName = row[columnMapping.NameColumnIndex]?.Trim() ?? string.Empty;
                    }

                    // Extraction de la date/heure
                    if (columnMapping.DateTimeColumnIndex < row.Count)
                    {
                        record.OriginalDateTimeString = row[columnMapping.DateTimeColumnIndex]?.Trim() ?? string.Empty;
                        
                        var parsedDateTime = TryParseDateTime(record.OriginalDateTimeString, columnMapping.DateFormat);
                        
                        if (parsedDateTime.HasValue)
                        {
                            record.AttendanceDateTime = parsedDateTime.Value;
                            record.IsValid = true;
                        }
                        else
                        {
                            record.IsValid = false;
                            record.ValidationError = $"Format de date invalide: '{record.OriginalDateTimeString}'";
                        }
                    }

                    // Validation du nom
                    if (string.IsNullOrWhiteSpace(record.EmployeeName))
                    {
                        record.IsValid = false;
                        record.ValidationError = "Nom d'employé manquant";
                    }
                }
                catch (Exception ex)
                {
                    record.IsValid = false;
                    record.ValidationError = $"Erreur de traitement: {ex.Message}";
                }

                records.Add(record);
            }

            return records;
        }

        public DateTime? TryParseDateTime(string dateString, string format)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // Essayer d'abord avec le format spécifié
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }

            // Essayer avec tous les formats supportés
            foreach (var supportedFormat in SupportedDateFormats.Formats)
            {
                if (DateTime.TryParseExact(dateString, supportedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }
            }

            // Essayer un parsing générique
            if (DateTime.TryParse(dateString, out result))
            {
                return result;
            }

            return null;
        }

        public string DetectDateFormat(IEnumerable<string> dateStrings)
        {
            var sampleStrings = dateStrings.Take(10).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            
            if (!sampleStrings.Any())
            {
                return SupportedDateFormats.Formats.First();
            }

            // Compter les succès pour chaque format
            var formatScores = new Dictionary<string, int>();

            foreach (var format in SupportedDateFormats.Formats)
            {
                int successCount = 0;
                
                foreach (var dateString in sampleStrings)
                {
                    if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    {
                        successCount++;
                    }
                }
                
                formatScores[format] = successCount;
            }

            // Retourner le format avec le plus de succès
            var bestFormat = formatScores.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
            
            return bestFormat.Value > 0 ? bestFormat.Key : SupportedDateFormats.Formats.First();
        }
    }
}

