// Payroll Statistics Charts
// This file contains JavaScript functions for rendering charts in the PayrollStatistics component

// Global chart instances to manage chart lifecycle
let salaryDistributionChart = null;
let paymentTrendsChart = null;

// Chart.js default configuration
Chart.defaults.font.family = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";
Chart.defaults.font.size = 12;
Chart.defaults.color = '#64748b';

// Function to render salary distribution chart
window.renderSalaryDistributionChart = function (data) {
    try {
        const ctx = document.getElementById('salaryDistributionChart');
        if (!ctx) {
            console.warn('Salary distribution chart canvas not found');
            return;
        }

        // Validate data
        if (!data || !data.Labels || !data.Data) {
            console.warn('Invalid data provided for salary distribution chart');
            return;
        }

        // Destroy existing chart if it exists
        if (salaryDistributionChart) {
            salaryDistributionChart.destroy();
            salaryDistributionChart = null;
        }

        const chartData = {
            labels: data.Labels || [],
            datasets: [{
                label: 'Nombre d\'employés',
                data: data.Data || [],
                backgroundColor: [
                    'rgba(59, 130, 246, 0.8)',   // Blue
                    'rgba(16, 185, 129, 0.8)',   // Green
                    'rgba(245, 158, 11, 0.8)',   // Yellow
                    'rgba(239, 68, 68, 0.8)',    // Red
                    'rgba(139, 92, 246, 0.8)'    // Purple
                ],
                borderColor: [
                    'rgba(59, 130, 246, 1)',
                    'rgba(16, 185, 129, 1)',
                    'rgba(245, 158, 11, 1)',
                    'rgba(239, 68, 68, 1)',
                    'rgba(139, 92, 246, 1)'
                ],
                borderWidth: 2,
                hoverBackgroundColor: [
                    'rgba(59, 130, 246, 0.9)',
                    'rgba(16, 185, 129, 0.9)',
                    'rgba(245, 158, 11, 0.9)',
                    'rgba(239, 68, 68, 0.9)',
                    'rgba(139, 92, 246, 0.9)'
                ]
            }]
        };

        const config = {
            type: 'doughnut',
            data: chartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                            font: {
                                size: 11
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: 'white',
                        bodyColor: 'white',
                        borderColor: 'rgba(255, 255, 255, 0.1)',
                        borderWidth: 1,
                        cornerRadius: 6,
                        displayColors: true,
                        callbacks: {
                            label: function (context) {
                                const label = context.label || '';
                                const value = context.parsed || 0;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                                return `${label}: ${value} employés (${percentage}%)`;
                            }
                        }
                    }
                },
                animation: {
                    animateRotate: true,
                    animateScale: true,
                    duration: 1000,
                    easing: 'easeOutQuart'
                }
            }
        };

        salaryDistributionChart = new Chart(ctx, config);
    } catch (error) {
        console.error('Error rendering salary distribution chart:', error);
    }
};

// Function to render payment trends chart
window.renderPaymentTrendsChart = function (data) {
    try {
        const ctx = document.getElementById('paymentTrendsChart')?.getContext('2d');
        if (!ctx) return;

        // Validate data structure
        if (!data || !Array.isArray(data.Labels) || !Array.isArray(data.NetPayData) || !Array.isArray(data.DeductionsData)) {
            console.warn('Invalid data format for payment trends chart');
            return;
        }

        // Destroy existing chart if any
        if (window.paymentTrendsChart instanceof Chart) {
            window.paymentTrendsChart.destroy();
        }

        const chartData = {
            labels: data.Labels,
            datasets: [
                {
                    label: 'Net à Payer',
                    data: data.NetPayData,
                    borderColor: 'rgba(59, 130, 246, 1)',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4
                },
                {
                    label: 'Déductions',
                    data: data.DeductionsData,
                    borderColor: 'rgba(239, 68, 68, 1)',
                    backgroundColor: 'rgba(239, 68, 68, 0.1)',
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4
                }
            ]
        };

        const config = {
            type: 'line',
            data: chartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false
                    }
                },
                interaction: {
                    mode: 'nearest',
                    axis: 'x',
                    intersect: false
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) {
                                return value.toLocaleString('fr-FR') + ' DA';
                            }
                        }
                    }
                }
            }
        };

        window.paymentTrendsChart = new Chart(ctx, config);
    } catch (error) {
        console.error('Error rendering payment trends chart:', error);
    }
};



// Function to update charts when data changes
window.updatePayrollCharts = function (salaryData, trendsData) {
    try {
        if (salaryData) {
            renderSalaryDistributionChart(salaryData);
        }
        if (trendsData) {
            renderPaymentTrendsChart(trendsData);
        }
    } catch (error) {
        console.error('Error updating payroll charts:', error);
    }
};

// Function to destroy charts (useful for cleanup)
window.destroyPayrollCharts = function () {
    try {
        if (salaryDistributionChart) {
            salaryDistributionChart.destroy();
            salaryDistributionChart = null;
        }
        if (paymentTrendsChart) {
            paymentTrendsChart.destroy();
            paymentTrendsChart = null;
        }
    } catch (error) {
        console.error('Error destroying payroll charts:', error);
    }
};

// Utility function to format currency
window.formatCurrency = function (amount, locale = 'fr-FR', currency = 'EUR') {
    try {
        if (amount === null || amount === undefined || isNaN(amount)) {
            return '0,00 €';
        }
        return new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: currency
        }).format(amount);
    } catch (error) {
        console.error('Error formatting currency:', error);
        return amount + ' €';
    }
};

// Utility function to format percentage
window.formatPercentage = function (value, decimals = 2, locale = 'fr-FR') {
    try {
        if (value === null || value === undefined || isNaN(value)) {
            return '0%';
        }
        return new Intl.NumberFormat(locale, {
            style: 'percent',
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        }).format(value);
    } catch (error) {
        console.error('Error formatting percentage:', error);
        return (value * 100).toFixed(decimals) + '%';
    }
};

// Handle window resize to maintain chart responsiveness
window.addEventListener('resize', function () {
    try {
        if (salaryDistributionChart) {
            salaryDistributionChart.resize();
        }
        if (paymentTrendsChart) {
            paymentTrendsChart.resize();
        }
    } catch (error) {
        console.error('Error resizing charts:', error);
    }
});

// Report Generation Functions
// These functions handle the generation of various payroll reports

/**
 * Generates a salary summary report
 * @param {Array} payrollData - Array of payroll objects
 */
window.generateSalarySummaryReport = function (payrollData) {
    try {
        if (!payrollData || !Array.isArray(payrollData) || payrollData.length === 0) {
            console.warn('No payroll data provided for salary summary report');
            return;
        }

        console.log('Generating salary summary report...', payrollData);

        // Create CSV content
        const csvContent = generateSalarySummaryCSV(payrollData);

        // Download the CSV file
        downloadCSV(csvContent, 'salary-summary-report.csv');

    } catch (error) {
        console.error('Error generating salary summary report:', error);
        throw error;
    }
};

/**
 * Generates an absence report
 * @param {Array} payrollData - Array of payroll objects
 */
window.generateAbsenceReport = function (payrollData) {
    try {
        if (!payrollData || !Array.isArray(payrollData) || payrollData.length === 0) {
            console.warn('No payroll data provided for absence report');
            return;
        }

        console.log('Generating absence report...', payrollData);

        // Filter employees with absences
        const employeesWithAbsences = payrollData.filter(p => p && p.AbsenceDays > 0);

        if (employeesWithAbsences.length === 0) {
            console.warn('No employees with absences found');
            return;
        }

        // Create CSV content
        const csvContent = generateAbsenceReportCSV(employeesWithAbsences);

        // Download the CSV file
        downloadCSV(csvContent, 'absence-report.csv');

    } catch (error) {
        console.error('Error generating absence report:', error);
        throw error;
    }
};

/**
 * Generates a deductions analysis report
 * @param {Array} payrollData - Array of payroll objects
 */
window.generateDeductionsReport = function (payrollData) {
    try {
        if (!payrollData || !Array.isArray(payrollData) || payrollData.length === 0) {
            console.warn('No payroll data provided for deductions report');
            return;
        }

        console.log('Generating deductions report...', payrollData);

        // Create CSV content
        const csvContent = generateDeductionsReportCSV(payrollData);

        // Download the CSV file
        downloadCSV(csvContent, 'deductions-analysis-report.csv');

    } catch (error) {
        console.error('Error generating deductions report:', error);
        throw error;
    }
};

/**
 * Generates a payroll trends report
 * @param {Array} trendsData - Array of trend objects
 */
window.generateTrendsReport = function (trendsData) {
    try {
        if (!trendsData || !Array.isArray(trendsData) || trendsData.length === 0) {
            console.warn('No trends data provided for trends report');
            return;
        }

        console.log('Generating trends report...', trendsData);

        // Create CSV content
        const csvContent = generateTrendsReportCSV(trendsData);

        // Download the CSV file
        downloadCSV(csvContent, 'payroll-trends-report.csv');

    } catch (error) {
        console.error('Error generating trends report:', error);
        throw error;
    }
};

/**
 * Downloads payroll data as CSV
 * @param {Array} payrollData - Array of payroll objects
 */
window.downloadPayrollReport = function (payrollData) {
    try {
        if (!payrollData || !Array.isArray(payrollData) || payrollData.length === 0) {
            console.warn('No payroll data provided for download');
            return;
        }

        console.log('Downloading payroll report...', payrollData);

        // Create comprehensive CSV content
        const csvContent = generateComprehensivePayrollCSV(payrollData);

        // Download the CSV file
        downloadCSV(csvContent, 'payroll-data-export.csv');

    } catch (error) {
        console.error('Error downloading payroll report:', error);
        throw error;
    }
};

/**
 * Navigates to a specific page (placeholder for SPA navigation)
 * @param {string} url - URL to navigate to
 */
window.navigateToPage = function (url) {
    try {
        if (!url || typeof url !== 'string') {
            console.warn('Invalid URL provided for navigation');
            return;
        }

        console.log('Navigating to:', url);

        // For Blazor Server, you might want to use NavigationManager
        // This is a basic implementation that could be enhanced
        if (typeof window.blazorNavigationManager !== 'undefined') {
            window.blazorNavigationManager.navigateTo(url);
        } else {
            // Fallback to standard navigation
            window.location.href = url;
        }

    } catch (error) {
        console.error('Error navigating to page:', error);
        // Fallback navigation
        try {
            window.location.href = url;
        } catch (fallbackError) {
            console.error('Fallback navigation also failed:', fallbackError);
        }
    }
};

// Helper Functions for CSV Generation

/**
 * Generates CSV content for salary summary report
 * @param {Array} payrollData - Array of payroll objects
 * @returns {string} CSV content
 */
function generateSalarySummaryCSV(payrollData) {
    const headers = [
        'Employé',
        'ID Employé',
        'Salaire de Base (€)',
        'Bonus (€)',
        'Déductions (€)',
        'Salaire Net (€)',
        'Date de Paie',
        'Période'
    ];

    const rows = payrollData.map(p => [
        (p.Employee && p.Employee.FullName) || p.Employee?.Name || 'N/A',
        p.EmployeeId || 'N/A',
        formatNumber(p.BaseSalary),
        formatNumber(p.Bonus),
        formatNumber(p.Deductions),
        formatNumber(p.NetPay),
        formatDate(p.PayDate),
        p.PayrollPeriodDisplay || 'N/A'
    ]);

    return createCSVContent(headers, rows);
}

/**
 * Generates CSV content for absence report
 * @param {Array} payrollData - Array of payroll objects with absences
 * @returns {string} CSV content
 */
function generateAbsenceReportCSV(payrollData) {
    const headers = [
        'Employé',
        'ID Employé',
        'Jours d\'Absence',
        'Déductions d\'Absence (€)',
        'Jours Travaillés',
        'Taux d\'Absence (%)',
        'Date de Paie',
        'Période'
    ];

    const rows = payrollData.map(p => [
        (p.Employee && p.Employee.FullName) || p.Employee?.Name || 'N/A',
        p.EmployeeId || 'N/A',
        p.AbsenceDays || 0,
        formatNumber(p.AbsenceDeduction || 0),
        p.WorkingDaysInPeriod || 0,
        formatPercentageValue(p.WorkingDaysInPeriod > 0 ? p.AbsenceDays / p.WorkingDaysInPeriod : 0),
        formatDate(p.PayDate),
        p.PayrollPeriodDisplay || 'N/A'
    ]);

    return createCSVContent(headers, rows);
}

/**
 * Generates CSV content for deductions report
 * @param {Array} payrollData - Array of payroll objects
 * @returns {string} CSV content
 */
function generateDeductionsReportCSV(payrollData) {
    const headers = [
        'Employé',
        'ID Employé',
        'Déductions Totales (€)',
        'Déductions d\'Absence (€)',
        'Avances Déduites (€)',
        'Autres Déductions (€)',
        'Salaire de Base (€)',
        '% Déductions',
        'Date de Paie'
    ];

    const rows = payrollData.map(p => {
        const totalDeductions = p.Deductions || 0;
        const absenceDeductions = p.AbsenceDeduction || 0;
        const advanceDeductions = p.AdvanceDeductionsAmounts || 0;
        const otherDeductions = totalDeductions - absenceDeductions - advanceDeductions;
        const baseSalary = p.BaseSalary || 0;
        const deductionPercentage = baseSalary > 0 ? totalDeductions / baseSalary : 0;

        return [
            (p.Employee && p.Employee.FullName) || p.Employee?.Name || 'N/A',
            p.EmployeeId || 'N/A',
            formatNumber(totalDeductions),
            formatNumber(absenceDeductions),
            formatNumber(advanceDeductions),
            formatNumber(otherDeductions),
            formatNumber(baseSalary),
            formatPercentageValue(deductionPercentage),
            formatDate(p.PayDate)
        ];
    });

    return createCSVContent(headers, rows);
}

/**
 * Generates CSV content for trends report
 * @param {Array} trendsData - Array of trend objects
 * @returns {string} CSV content
 */
function generateTrendsReportCSV(trendsData) {
    const headers = [
        'Période',
        'Salaire Net Total (€)',
        'Déductions Totales (€)',
        'Nombre d\'Employés',
        'Salaire Moyen (€)',
        'Déductions Moyennes (€)'
    ];

    const rows = trendsData.map(t => [
        t.Period || 'N/A',
        formatNumber(t.TotalNetPay || 0),
        formatNumber(t.TotalDeductions || 0),
        t.EmployeeCount || 0,
        formatNumber(t.EmployeeCount > 0 ? (t.TotalNetPay || 0) / t.EmployeeCount : 0),
        formatNumber(t.EmployeeCount > 0 ? (t.TotalDeductions || 0) / t.EmployeeCount : 0)
    ]);

    return createCSVContent(headers, rows);
}

/**
 * Generates comprehensive CSV content for payroll data
 * @param {Array} payrollData - Array of payroll objects
 * @returns {string} CSV content
 */
function generateComprehensivePayrollCSV(payrollData) {
    const headers = [
        'Employé',
        'ID Employé',
        'Salaire de Base (€)',
        'Bonus (€)',
        'Déductions (€)',
        'Salaire Net (€)',
        'Jours d\'Absence',
        'Déductions d\'Absence (€)',
        'Avances Déduites (€)',
        'Transaction (€)',
        'Espèces (€)',
        'Date de Paie',
        'Période'
    ];

    const rows = payrollData.map(p => [
        (p.Employee && p.Employee.FullName) || p.Employee?.Name || 'N/A',
        p.EmployeeId || 'N/A',
        formatNumber(p.BaseSalary),
        formatNumber(p.Bonus),
        formatNumber(p.Deductions),
        formatNumber(p.NetPay),
        p.AbsenceDays || 0,
        formatNumber(p.AbsenceDeduction || 0),
        formatNumber(p.AdvanceDeductionsAmounts || 0),
        formatNumber(p.Transaction || 0),
        formatNumber(p.Cash || 0),
        formatDate(p.PayDate),
        p.PayrollPeriodDisplay || 'N/A'
    ]);

    return createCSVContent(headers, rows);
}

// Utility functions for formatting

/**
 * Formats a number for CSV output
 * @param {number} value - Number to format
 * @returns {string} Formatted number
 */
function formatNumber(value) {
    if (value === null || value === undefined || isNaN(value)) {
        return '0';
    }
    return value.toFixed(2);
}

/**
 * Formats a date for CSV output
 * @param {Date|string} date - Date to format
 * @returns {string} Formatted date
 */
function formatDate(date) {
    try {
        if (!date) return 'N/A';
        const d = new Date(date);
        if (isNaN(d.getTime())) return 'N/A';
        return d.toLocaleDateString('fr-FR');
    } catch (error) {
        console.error('Error formatting date:', error);
        return 'N/A';
    }
}

/**
 * Formats a percentage value for CSV output
 * @param {number} value - Percentage value (0-1)
 * @returns {string} Formatted percentage
 */
function formatPercentageValue(value) {
    if (value === null || value === undefined || isNaN(value)) {
        return '0%';
    }
    return (value * 100).toFixed(2) + '%';
}

/**
 * Creates CSV content from headers and rows
 * @param {Array} headers - Array of header strings
 * @param {Array} rows - Array of row arrays
 * @returns {string} CSV content
 */
function createCSVContent(headers, rows) {
    try {
        const csvRows = [headers];
        csvRows.push(...rows);

        return csvRows.map(row =>
            row.map(field => {
                // Escape quotes and wrap in quotes if necessary
                const stringField = String(field || '');
                if (stringField.includes(',') || stringField.includes('"') || stringField.includes('\n')) {
                    return '"' + stringField.replace(/"/g, '""') + '"';
                }
                return stringField;
            }).join(',')
        ).join('\n');
    } catch (error) {
        console.error('Error creating CSV content:', error);
        return '';
    }
}

/**
 * Downloads CSV content as a file
 * @param {string} csvContent - CSV content to download
 * @param {string} filename - Name of the file to download
 */
function downloadCSV(csvContent, filename) {
    try {
        if (!csvContent) {
            console.warn('No CSV content to download');
            return;
        }

        // Add BOM for proper UTF-8 encoding in Excel
        const BOM = '\uFEFF';
        const blob = new Blob([BOM + csvContent], { type: 'text/csv;charset=utf-8;' });

        // Create download link
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', filename);
        link.style.visibility = 'hidden';

        // Trigger download
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // Clean up
        URL.revokeObjectURL(url);

        console.log('CSV file downloaded:', filename);
    } catch (error) {
        console.error('Error downloading CSV file:', error);
    }
}

// Export functions for module usage (if needed)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        renderSalaryDistributionChart,
        renderPaymentTrendsChart,
        updatePayrollCharts,
        destroyPayrollCharts,
        formatCurrency,
        formatPercentage,
        generateSalarySummaryReport,
        generateAbsenceReport,
        generateDeductionsReport,
        generateTrendsReport,
        downloadPayrollReport,
        navigateToPage
    };
}

