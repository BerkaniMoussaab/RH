using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class DashboardStatistics
    {
        [Display(Name = "Nombre total d'employés")]
        public int TotalEmployees { get; set; }

        [Display(Name = "Salaire moyen")]
        public decimal AverageSalary { get; set; }

        [Display(Name = "Nombre total d'avances")]
        public int TotalAdvances { get; set; }

        [Display(Name = "Montant total des avances")]
        public decimal TotalAdvanceAmount { get; set; }

        [Display(Name = "Masse salariale brute totale")]
        public decimal TotalGrossSalary { get; set; }

        [Display(Name = "Salaire net total")]
        public decimal TotalNetPay { get; set; }

        [Display(Name = "Taux de déduction moyen")]
        public decimal AverageDeductionRate
        {
            get
            {
                if (TotalGrossSalary == 0) return 0;
                return ((TotalGrossSalary - TotalNetPay) / TotalGrossSalary) * 100;
            }
        }

        [Display(Name = "Coût moyen par employé")]
        public decimal AverageCostPerEmployee
        {
            get
            {
                if (TotalEmployees == 0) return 0;
                return TotalGrossSalary / TotalEmployees;
            }
        }
    }
}

