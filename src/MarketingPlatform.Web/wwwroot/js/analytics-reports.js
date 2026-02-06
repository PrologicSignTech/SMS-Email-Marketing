// Analytics Reports functionality
(function() {
    'use strict';

    function initializeReports() {
        // Bind all generate report buttons
        const generateBtns = document.querySelectorAll('[data-action="generate-report"]');
        generateBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const reportType = this.getAttribute('data-report-type');
                generateReport(reportType);
            });
        });
    }

    function generateReport(type) {
        showNotification(`Generating ${type} report... (Demo)`, 'info');
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeReports);
    } else {
        initializeReports();
    }
})();
