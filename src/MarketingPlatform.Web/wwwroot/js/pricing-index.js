// Pricing Index functionality
(function() {
    'use strict';

    function initializePricing() {
        // Bind all edit buttons
        const editBtns = document.querySelectorAll('[data-action="edit-plan"]');
        editBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const planId = this.getAttribute('data-plan-id');
                editPlan(planId);
            });
        });

        // Bind all delete buttons
        const deleteBtns = document.querySelectorAll('[data-action="delete-plan"]');
        deleteBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const planId = this.getAttribute('data-plan-id');
                deletePlan(planId);
            });
        });
    }

    function editPlan(id) {
        window.location.href = `/Pricing/Edit/${id}`;
    }

    function deletePlan(id) {
        if (confirm('Are you sure you want to delete this pricing plan?')) {
            showNotification('Plan deleted! (Demo)', 'success');
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializePricing);
    } else {
        initializePricing();
    }
})();
