// Pricing Edit functionality
(function() {
    'use strict';

    function initializePricingEdit() {
        // Bind delete button
        const deleteBtn = document.querySelector('[data-action="delete-plan"]');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', deletePlan);
        }

        // Bind form submit
        const form = document.getElementById('pricingForm');
        if (form) {
            form.addEventListener('submit', handleSubmit);
        }
    }

    function deletePlan() {
        if (confirm('Are you sure you want to delete this plan?')) {
            showNotification('Plan deleted! (Demo)', 'success');
            setTimeout(() => {
                window.location.href = '/Pricing/Index';
            }, 1000);
        }
    }

    function handleSubmit(e) {
        e.preventDefault();
        showNotification('Plan updated successfully! (Demo)', 'success');
        setTimeout(() => {
            window.location.href = '/Pricing/Index';
        }, 1000);
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializePricingEdit);
    } else {
        initializePricingEdit();
    }
})();
