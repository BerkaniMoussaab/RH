using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class PayrollStatistics
    {
        [Display(Name = "Masse salariale totale")]
        public decimal TotalGrossSalary { get; set; }

        [Display(Name = "Total des déductions")]
        public decimal TotalDeductions { get; set; }

        [Display(Name = "Total des bonus")]
        public decimal TotalBonuses { get; set; }

        [Display(Name = "Salaire net total")]
        public decimal TotalNetPay { get; set; }

        [Display(Name = "Coût total employeur")]
        public decimal TotalEmployerCost { get; set; }

        [Display(Name = "Salaire brut moyen")]
        public decimal AverageGrossSalary { get; set; }

        [Display(Name = "Salaire net moyen")]
        public decimal AverageNetSalary { get; set; }

        [Display(Name = "Total des avances déduites")]
        public decimal TotalAdvanceDeductions { get; set; }

        [Display(Name = "Total des déductions d'absence")]
        public decimal TotalAbsenceDeductions { get; set; }

        [Display(Name = "Évolution mensuelle de la masse salariale")]
        public Dictionary<string, decimal> MonthlyGrossSalary { get; set; } = new Dictionary<string, decimal>();

        [Display(Name = "Évolution mensuelle du salaire net")]
        public Dictionary<string, decimal> MonthlyNetSalary { get; set; } = new Dictionary<string, decimal>();

        [Display(Name = "Répartition des paiements par type")]
        public Dictionary<string, decimal> PaymentTypeDistribution { get; set; } = new Dictionary<string, decimal>();

        // Nouvelles propriétés pour les filtres de date
        [Display(Name = "Date de début du filtre")]
        public DateTime? FilterStartDate { get; set; }

        [Display(Name = "Date de fin du filtre")]
        public DateTime? FilterEndDate { get; set; }

        // Propriété calculée pour afficher la période filtrée
        public string FilterPeriodDisplay
        {
            get
            {
                if (!FilterStartDate.HasValue && !FilterEndDate.HasValue)
                    return "Toutes les données";

                if (FilterStartDate.HasValue && FilterEndDate.HasValue)
                    return $"Du {FilterStartDate.Value:dd/MM/yyyy} au {FilterEndDate.Value:dd/MM/yyyy}";

                if (FilterStartDate.HasValue)
                    return $"À partir du {FilterStartDate.Value:dd/MM/yyyy}";

                if (FilterEndDate.HasValue)
                    return $"Jusqu'au {FilterEndDate.Value:dd/MM/yyyy}";

                return "Période non définie";
            }
        }
    }

    // Classe pour les filtres de date
    public class DateRange
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsValid => !StartDate.HasValue || !EndDate.HasValue || StartDate.Value <= EndDate.Value;
    }

    // Énumération pour les plages de dates prédéfinies
    public enum PredefinedDateRange
    {
        [Display(Name = "Personnalisé")]
        Custom,

        [Display(Name = "Ce mois-ci")]
        CurrentMonth,

        [Display(Name = "Le mois dernier")]
        PreviousMonth,

        [Display(Name = "Cette année")]
        CurrentYear,

        [Display(Name = "L'année dernière")]
        PreviousYear,

        [Display(Name = "Les 7 derniers jours")]
        Last7Days,

        [Display(Name = "Les 30 derniers jours")]
        Last30Days,

        [Display(Name = "Les 90 derniers jours")]
        Last90Days,

        [Display(Name = "Les 365 derniers jours")]
        Last365Days
    }

    // Classe utilitaire pour calculer les plages de dates
    public static class DateRangeHelper
    {
        public static DateRange GetDateRange(PredefinedDateRange predefinedRange)
        {
            var today = DateTime.Today;
            var dateRange = new DateRange();

            switch (predefinedRange)
            {
                case PredefinedDateRange.CurrentMonth:
                    dateRange.StartDate = new DateTime(today.Year, today.Month, 1);
                    dateRange.EndDate = dateRange.StartDate.Value.AddMonths(1).AddDays(-1);
                    break;

                case PredefinedDateRange.PreviousMonth:
                    var firstDayPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    dateRange.StartDate = firstDayPreviousMonth;
                    dateRange.EndDate = firstDayPreviousMonth.AddMonths(1).AddDays(-1);
                    break;

                case PredefinedDateRange.CurrentYear:
                    dateRange.StartDate = new DateTime(today.Year, 1, 1);
                    dateRange.EndDate = new DateTime(today.Year, 12, 31);
                    break;

                case PredefinedDateRange.PreviousYear:
                    dateRange.StartDate = new DateTime(today.Year - 1, 1, 1);
                    dateRange.EndDate = new DateTime(today.Year - 1, 12, 31);
                    break;

                case PredefinedDateRange.Last7Days:
                    dateRange.StartDate = today.AddDays(-6);
                    dateRange.EndDate = today;
                    break;

                case PredefinedDateRange.Last30Days:
                    dateRange.StartDate = today.AddDays(-29);
                    dateRange.EndDate = today;
                    break;

                case PredefinedDateRange.Last90Days:
                    dateRange.StartDate = today.AddDays(-89);
                    dateRange.EndDate = today;
                    break;

                case PredefinedDateRange.Last365Days:
                    dateRange.StartDate = today.AddDays(-364);
                    dateRange.EndDate = today;
                    break;

                case PredefinedDateRange.Custom:
                default:
                    // Pour Custom, on laisse les dates nulles pour que l'utilisateur les définisse
                    break;
            }

            return dateRange;
        }

        public static string GetDisplayName(PredefinedDateRange predefinedRange)
        {
            var field = predefinedRange.GetType().GetField(predefinedRange.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
                                 .FirstOrDefault() as DisplayAttribute;
            return attribute?.Name ?? predefinedRange.ToString();
        }
    }
    public class EmployeeStatistics
    {
        public int TotalEmployees { get; set; }
        public decimal AverageSalary { get; set; }
        public Dictionary<string, int> EmployeesByStatus { get; set; } = new Dictionary<string, int>();
    }

    public class AdvanceStatistics
    {
        public int TotalAdvances { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalRemaining { get; set; }
        public Dictionary<string, int> AdvancesByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> MonthlyAdvances { get; set; } = new Dictionary<string, decimal>();
    }

   
}

