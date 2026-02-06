// Analytics Campaigns functionality
(function() {
    'use strict';

    function initializeCampaigns() {
        // Bind export button
        const exportBtn = document.querySelector('[data-action="export-report"]');
        if (exportBtn) {
            exportBtn.addEventListener('click', exportReport);
        }
    }

    function exportReport() {
        showNotification('Exporting report... (Demo)', 'info');
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeCampaigns);
    } else {
        initializeCampaigns();
    }
})();
