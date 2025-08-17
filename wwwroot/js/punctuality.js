// Punctuality Management JavaScript Functions

// Trigger file input click
window.triggerFileInput = (element) => {
    if (element) {
        element.click();
    }
};

// Download file function
window.downloadFile = (filename, content, contentType) => {
    const blob = new Blob([content], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};

// Show toast notification
window.showToast = (message, type = 'info') => {
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    
    // Add to toast container or create one
    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }
    
    toastContainer.appendChild(toast);
    
    // Initialize and show toast
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
    
    // Remove toast element after it's hidden
    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
};

// Smooth scroll to element
window.scrollToElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};

// Format currency for display
window.formatCurrency = (amount, locale = 'fr-FR', currency = 'EUR') => {
    return new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: currency
    }).format(amount);
};

// Validate time format
window.validateTimeFormat = (timeString) => {
    const timeRegex = /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/;
    return timeRegex.test(timeString);
};

// Auto-resize textarea
window.autoResizeTextarea = (element) => {
    if (element) {
        element.style.height = 'auto';
        element.style.height = element.scrollHeight + 'px';
    }
};

// Copy text to clipboard
window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        showToast('Texte copié dans le presse-papiers', 'success');
        return true;
    } catch (err) {
        console.error('Erreur lors de la copie:', err);
        showToast('Erreur lors de la copie', 'danger');
        return false;
    }
};

// Initialize tooltips
window.initializeTooltips = () => {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
};

// Initialize popovers
window.initializePopovers = () => {
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
};

// Debounce function for search inputs
window.debounce = (func, wait) => {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

// Local storage helpers
window.localStorageHelper = {
    set: (key, value) => {
        try {
            localStorage.setItem(key, JSON.stringify(value));
            return true;
        } catch (e) {
            console.error('Erreur lors de la sauvegarde:', e);
            return false;
        }
    },
    
    get: (key, defaultValue = null) => {
        try {
            const item = localStorage.getItem(key);
            return item ? JSON.parse(item) : defaultValue;
        } catch (e) {
            console.error('Erreur lors de la lecture:', e);
            return defaultValue;
        }
    },
    
    remove: (key) => {
        try {
            localStorage.removeItem(key);
            return true;
        } catch (e) {
            console.error('Erreur lors de la suppression:', e);
            return false;
        }
    }
};

// Theme management
window.themeManager = {
    setTheme: (theme) => {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorageHelper.set('theme', theme);
    },
    
    getTheme: () => {
        return localStorageHelper.get('theme', 'light');
    },
    
    toggleTheme: () => {
        const currentTheme = themeManager.getTheme();
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        themeManager.setTheme(newTheme);
        return newTheme;
    }
};

// Initialize theme on page load
document.addEventListener('DOMContentLoaded', () => {
    const savedTheme = themeManager.getTheme();
    themeManager.setTheme(savedTheme);
    
    // Initialize Bootstrap components
    initializeTooltips();
    initializePopovers();
});

// Print functionality
window.printElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (!element) {
        showToast('Élément à imprimer non trouvé', 'danger');
        return;
    }
    
    const printWindow = window.open('', '_blank');
    printWindow.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Impression</title>
            <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
            <link href="/css/punctuality.css" rel="stylesheet">
            <style>
                @media print {
                    .no-print { display: none !important; }
                    .print-break { page-break-after: always; }
                }
            </style>
        </head>
        <body>
            ${element.outerHTML}
        </body>
        </html>
    `);
    
    printWindow.document.close();
    printWindow.focus();
    
    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 250);
};

// Export to Excel (basic CSV export)
window.exportTableToCSV = (tableId, filename = 'export.csv') => {
    const table = document.getElementById(tableId);
    if (!table) {
        showToast('Tableau non trouvé', 'danger');
        return;
    }
    
    let csv = [];
    const rows = table.querySelectorAll('tr');
    
    for (let i = 0; i < rows.length; i++) {
        const row = [];
        const cols = rows[i].querySelectorAll('td, th');
        
        for (let j = 0; j < cols.length; j++) {
            let cellText = cols[j].innerText.replace(/"/g, '""');
            row.push('"' + cellText + '"');
        }
        
        csv.push(row.join(','));
    }
    
    downloadFile(filename, csv.join('\n'), 'text/csv');
};

// Form validation helpers
window.formValidation = {
    validateRequired: (value) => {
        return value !== null && value !== undefined && value.toString().trim() !== '';
    },
    
    validateEmail: (email) => {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    },
    
    validateNumber: (value, min = null, max = null) => {
        const num = parseFloat(value);
        if (isNaN(num)) return false;
        if (min !== null && num < min) return false;
        if (max !== null && num > max) return false;
        return true;
    },
    
    validateDate: (dateString) => {
        const date = new Date(dateString);
        return date instanceof Date && !isNaN(date);
    }
};

// Animation helpers
window.animationHelpers = {
    fadeIn: (element, duration = 300) => {
        element.style.opacity = '0';
        element.style.display = 'block';
        
        const start = performance.now();
        
        const animate = (currentTime) => {
            const elapsed = currentTime - start;
            const progress = Math.min(elapsed / duration, 1);
            
            element.style.opacity = progress.toString();
            
            if (progress < 1) {
                requestAnimationFrame(animate);
            }
        };
        
        requestAnimationFrame(animate);
    },
    
    fadeOut: (element, duration = 300) => {
        const start = performance.now();
        const initialOpacity = parseFloat(element.style.opacity) || 1;
        
        const animate = (currentTime) => {
            const elapsed = currentTime - start;
            const progress = Math.min(elapsed / duration, 1);
            
            element.style.opacity = (initialOpacity * (1 - progress)).toString();
            
            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                element.style.display = 'none';
            }
        };
        
        requestAnimationFrame(animate);
    }
};

// Keyboard shortcuts
document.addEventListener('keydown', (e) => {
    // Ctrl+S to save (prevent default browser save)
    if (e.ctrlKey && e.key === 's') {
        e.preventDefault();
        const saveButton = document.querySelector('[data-action="save"]');
        if (saveButton && !saveButton.disabled) {
            saveButton.click();
        }
    }
    
    // Escape to close modals
    if (e.key === 'Escape') {
        const openModal = document.querySelector('.modal.show');
        if (openModal) {
            const modal = bootstrap.Modal.getInstance(openModal);
            if (modal) {
                modal.hide();
            }
        }
    }
});

// Console log for debugging
console.log('Punctuality Management JavaScript loaded successfully');

