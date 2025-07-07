using System.ComponentModel.DataAnnotations;

namespace RH.Models
{
    public class Payroll
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime PayDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BaseSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Bonus { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Deductions { get; set; }

        public decimal NetPay => BaseSalary + Bonus - Deductions;

        public ICollection<PayrollAppliedRule> AppliedRules { get; set; } = new List<PayrollAppliedRule>();
        public int AbsenceDays { get; set; } // how many days the employee was absent
        public decimal AbsenceDeduction { get; set; } // the amount deducted due to absences

        public decimal DeductionPerAbsenceDay { get; set; }
        public int ManualAbsenceDays { get; set; }
        public ICollection<AbsenceRecord> Absences { get; set; } = new List<AbsenceRecord>();
        /// <summary>
        /// If true: Transaction is entered, Cash is calculated.
        /// If false: Cash is entered, Transaction is calculated.
        /// </summary>
        public bool TransactionIsManual { get; set; } = true;

        private decimal _transaction;
        private decimal _cash;
        public decimal Transaction
        {
            get => _transaction;
            set
            {
                _transaction = value;
                if (TransactionIsManual)
                {
                    _cash = NetPay - _transaction;
                }
            }
        }

        public decimal Cash
        {
            get => _cash;
            set
            {
                _cash = value;
                if (!TransactionIsManual)
                {
                    _transaction = NetPay - _cash;
                }
            }
        }

   
        /// <summary>
        /// Start date of the payroll period for calculating absences and bonuses
        /// </summary>
        [Display(Name = "Date de début de période")]
        public DateTime? PayrollStartDate { get; set; }

        /// <summary>
        /// End date of the payroll period for calculating absences and bonuses
        /// </summary>
        [Display(Name = "Date de fin de période")]
        public DateTime? PayrollEndDate { get; set; }

        /// <summary>
        /// Calculated property to get the number of working days in the payroll period
        /// </summary>
        public int WorkingDaysInPeriod
        {
            get
            {
                if (!PayrollStartDate.HasValue || !PayrollEndDate.HasValue)
                    return 22; // Default working days per month

                int workingDays = 0;
                for (var date = PayrollStartDate.Value; date <= PayrollEndDate.Value; date = date.AddDays(1))
                {
                    if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    {
                        workingDays++;
                    }
                }
                return workingDays;
            }
        }

        /// <summary>
        /// Calculated property to get the total days in the payroll period
        /// </summary>
        public int TotalDaysInPeriod
        {
            get
            {
                if (!PayrollStartDate.HasValue || !PayrollEndDate.HasValue)
                    return 30; // Default days per month

                return (PayrollEndDate.Value - PayrollStartDate.Value).Days + 1;
            }
        }

        /// <summary>
        /// Gets a formatted string representation of the payroll period
        /// </summary>
        public string PayrollPeriodDisplay
        {
            get
            {
                if (!PayrollStartDate.HasValue || !PayrollEndDate.HasValue)
                    return "Période non définie";

                return $"{PayrollStartDate.Value:dd/MM/yyyy} - {PayrollEndDate.Value:dd/MM/yyyy}";
            }
        }

        /// <summary>
        /// Validates that the payroll period is valid
        /// </summary>
        public bool IsPayrollPeriodValid
        {
            get
            {
                if (!PayrollStartDate.HasValue || !PayrollEndDate.HasValue)
                    return false;

                return PayrollStartDate.Value <= PayrollEndDate.Value;
            }
        }
    }

    /// <summary>
    /// Enum for predefined payroll periods
    /// </summary>
    public enum PayrollPeriodType
    {
        Custom,
        CurrentMonth,
        PreviousMonth,
        Last30Days,
        CurrentQuarter,
        PreviousQuarter
    }

    /// <summary>
    /// Helper class for payroll period calculations
    /// </summary>
    public static class PayrollPeriodHelper
    {
        /// <summary>
        /// Gets the date range for a predefined period type
        /// </summary>
        /// <param name="periodType">The type of period</param>
        /// <param name="referenceDate">Reference date for calculations (defaults to today)</param>
        /// <returns>Tuple containing start and end dates</returns>
        public static (DateTime StartDate, DateTime EndDate) GetPeriodDates(PayrollPeriodType periodType, DateTime? referenceDate = null)
        {
            var today = referenceDate ?? DateTime.Today;

            return periodType switch
            {
                PayrollPeriodType.CurrentMonth => GetCurrentMonthPeriod(today),
                PayrollPeriodType.PreviousMonth => GetPreviousMonthPeriod(today),
                PayrollPeriodType.Last30Days => GetLast30DaysPeriod(today),
                PayrollPeriodType.CurrentQuarter => GetCurrentQuarterPeriod(today),
                PayrollPeriodType.PreviousQuarter => GetPreviousQuarterPeriod(today),
                _ => (today.AddDays(-30), today)
            };
        }

        private static (DateTime StartDate, DateTime EndDate) GetCurrentMonthPeriod(DateTime referenceDate)
        {
            var startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return (startDate, endDate);
        }

        private static (DateTime StartDate, DateTime EndDate) GetPreviousMonthPeriod(DateTime referenceDate)
        {
            var startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1).AddMonths(-1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return (startDate, endDate);
        }

        private static (DateTime StartDate, DateTime EndDate) GetLast30DaysPeriod(DateTime referenceDate)
        {
            return (referenceDate.AddDays(-29), referenceDate);
        }

        private static (DateTime StartDate, DateTime EndDate) GetCurrentQuarterPeriod(DateTime referenceDate)
        {
            var quarter = (referenceDate.Month - 1) / 3 + 1;
            var startDate = new DateTime(referenceDate.Year, (quarter - 1) * 3 + 1, 1);
            var endDate = startDate.AddMonths(3).AddDays(-1);
            return (startDate, endDate);
        }

        private static (DateTime StartDate, DateTime EndDate) GetPreviousQuarterPeriod(DateTime referenceDate)
        {
            var currentQuarter = (referenceDate.Month - 1) / 3 + 1;
            var previousQuarter = currentQuarter == 1 ? 4 : currentQuarter - 1;
            var year = currentQuarter == 1 ? referenceDate.Year - 1 : referenceDate.Year;

            var startDate = new DateTime(year, (previousQuarter - 1) * 3 + 1, 1);
            var endDate = startDate.AddMonths(3).AddDays(-1);
            return (startDate, endDate);
        }

        /// <summary>
        /// Calculates the number of working days between two dates (excluding weekends)
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Number of working days</returns>
        public static int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            int workingDays = 0;
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }
            return workingDays;
        }

        /// <summary>
        /// Calculates the number of working days between two dates, excluding specified holidays
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="holidays">List of holiday dates to exclude</param>
        /// <returns>Number of working days</returns>
        public static int CalculateWorkingDays(DateTime startDate, DateTime endDate, IEnumerable<DateTime> holidays)
        {
            var holidaySet = new HashSet<DateTime>(holidays.Select(h => h.Date));
            int workingDays = 0;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday &&
                    date.DayOfWeek != DayOfWeek.Sunday &&
                    !holidaySet.Contains(date.Date))
                {
                    workingDays++;
                }
            }
            return workingDays;
        }
    }

}
