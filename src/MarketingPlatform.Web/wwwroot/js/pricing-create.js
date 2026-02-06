// Pricing Create functionality
(function() {
    'use strict';

    function initializePricingCreate() {
        // Bind add feature button
        const addFeatureBtn = document.querySelector('[data-action="add-feature"]');
        if (addFeatureBtn) {
            addFeatureBtn.addEventListener('click', addFeature);
        }

        // Bind existing remove feature buttons
        bindRemoveButtons();

        // Bind form submit
        const form = document.getElementById('pricingForm');
        if (form) {
            form.addEventListener('submit', handleSubmit);
        }
    }

    function addFeature() {
        const featuresDiv = document.getElementById('features');
        const newFeature = document.createElement('div');
        newFeature.className = 'input-group mb-2';
        newFeature.innerHTML = `
            <input type="text" class="form-control" placeholder="Feature description">
            <button class="btn btn-outline-danger" type="button" data-action="remove-feature">
                <i class="bi bi-trash"></i>
            </button>
        `;
        featuresDiv.appendChild(newFeature);
        
        // Bind the new remove button
        bindRemoveButtons();
    }

    function bindRemoveButtons() {
        const removeBtns = document.querySelectorAll('[data-action="remove-feature"]');
        removeBtns.forEach(btn => {
            // Remove existing listener if any
            btn.replaceWith(btn.cloneNode(true));
        });
        
        // Re-query and bind
        const newRemoveBtns = document.querySelectorAll('[data-action="remove-feature"]');
        newRemoveBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                this.closest('.input-group').remove();
            });
        });
    }

    function handleSubmit(e) {
        e.preventDefault();
        showNotification('Pricing plan created successfully! (Demo)', 'success');
        setTimeout(() => {
            window.location.href = '/Pricing/Index';
        }, 1000);
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializePricingCreate);
    } else {
        initializePricingCreate();
    }
})();
