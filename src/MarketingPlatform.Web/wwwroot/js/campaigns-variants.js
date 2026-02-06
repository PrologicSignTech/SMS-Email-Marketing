/**
 * Campaigns Variants Page JavaScript
 * Handles A/B test variant management, traffic allocation, and performance comparison
 */

let campaignId = 0;

document.addEventListener('DOMContentLoaded', function() {
    // Initialize campaign ID from global config if available
    if (window.campaignsVariantsConfig && window.campaignsVariantsConfig.campaignId) {
        campaignId = window.campaignsVariantsConfig.campaignId;
    } else {
        // Try to get from ViewBag passed to window
        campaignId = parseInt(document.querySelector('[data-campaign-id]')?.dataset.campaignId || '0');
    }

    if (campaignId > 0) {
        loadCampaignInfo();
        loadVariants();
    }

    setupEventListeners();
});

/**
 * Setup event listeners for buttons
 */
function setupEventListeners() {
    // Modal create variant button
    const createBtn = document.querySelector('button[data-action="create-variant"]');
    if (createBtn) {
        createBtn.addEventListener('click', createVariant);
    }

    // Refresh comparison button
    const refreshBtn = document.querySelector('button[data-action="refresh-comparison"]');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', loadComparison);
    }

    // Event delegation for delete buttons in variants list
    document.addEventListener('click', function(e) {
        if (e.target.closest('button[data-action="delete-variant"]')) {
            const variantId = e.target.closest('button[data-action="delete-variant"]').dataset.variantId;
            deleteVariant(variantId);
        }
    });

    // Event delegation for select winner buttons
    document.addEventListener('click', function(e) {
        if (e.target.closest('button[data-action="select-winner"]')) {
            const variantId = e.target.closest('button[data-action="select-winner"]').dataset.variantId;
            selectWinner(variantId);
        }
    });
}

/**
 * Get authorization headers for API requests
 */
function getAuthHeaders() {
    return {
        'Authorization': 'Bearer ' + localStorage.getItem('token'),
        'Content-Type': 'application/json'
    };
}

/**
 * Load campaign information
 */
function loadCampaignInfo() {
    const url = window.AppUrls ? 
        window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.get(campaignId)) : 
        `/api/campaigns/${campaignId}`;
    
    fetch(url, {
        method: 'GET',
        headers: getAuthHeaders()
    })
    .then(response => response.json())
    .then(data => {
        if (data.success && data.data) {
            document.getElementById('campaignName').textContent = data.data.name;
        }
    })
    .catch(error => {
        console.error('Error loading campaign info:', error);
    });
}

/**
 * Load all variants for the campaign
 */
function loadVariants() {
    const url = window.AppUrls ? 
        window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.variants.list(campaignId)) : 
        `/api/campaigns/${campaignId}/variants`;
    
    fetch(url, {
        method: 'GET',
        headers: getAuthHeaders()
    })
    .then(response => response.json())
    .then(data => {
        if (data.success && data.data) {
            renderVariants(data.data);
            updateTrafficAllocation(data.data);
        }
    })
    .catch(error => {
        document.getElementById('variantsList').innerHTML = 
            '<div class="col-12"><div class="alert alert-danger">Failed to load variants</div></div>';
        console.error('Error loading variants:', error);
    });
}

/**
 * Render variants list
 */
function renderVariants(variants) {
    const container = document.getElementById('variantsList');

    if (!variants || variants.length === 0) {
        container.innerHTML = '<div class="col-12"><div class="alert alert-info">No variants created yet. Click "Add Variant" to get started.</div></div>';
        return;
    }

    let html = '';
    variants.forEach(variant => {
        html += `
            <div class="col-md-6 mb-3">
                <div class="card ${variant.isControl ? 'border-primary' : ''}">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div>
                            <h6 class="mb-0">${variant.name}</h6>
                            ${variant.isControl ? '<span class="badge bg-primary">Control</span>' : ''}
                            ${variant.isActive ? '<span class="badge bg-success">Active</span>' : '<span class="badge bg-secondary">Inactive</span>'}
                        </div>
                        <div class="btn-group btn-group-sm">
                            ${!variant.isControl ? `<button class="btn btn-outline-danger" data-action="delete-variant" data-variant-id="${variant.id}"><i class="bi bi-trash"></i></button>` : ''}
                        </div>
                    </div>
                    <div class="card-body">
                        <p class="text-muted small mb-2">${variant.description || 'No description'}</p>
                        <div class="row text-center mb-3">
                            <div class="col-4">
                                <strong>${variant.trafficPercentage}%</strong>
                                <br><small class="text-muted">Traffic</small>
                            </div>
                            <div class="col-4">
                                <strong>${variant.sentCount}</strong>
                                <br><small class="text-muted">Sent</small>
                            </div>
                            <div class="col-4">
                                <strong>${variant.deliveredCount}</strong>
                                <br><small class="text-muted">Delivered</small>
                            </div>
                        </div>
                        ${variant.analytics ? `
                        <div class="row text-center">
                            <div class="col-4">
                                <strong>${variant.analytics.deliveryRate.toFixed(2)}%</strong>
                                <br><small class="text-muted">Delivery Rate</small>
                            </div>
                            <div class="col-4">
                                <strong>${variant.analytics.clickRate.toFixed(2)}%</strong>
                                <br><small class="text-muted">Click Rate</small>
                            </div>
                            <div class="col-4">
                                <strong>${variant.analytics.conversionRate.toFixed(2)}%</strong>
                                <br><small class="text-muted">Conversion Rate</small>
                            </div>
                        </div>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
    });

    container.innerHTML = html;
}

/**
 * Update traffic allocation display
 */
function updateTrafficAllocation(variants) {
    const total = variants.reduce((sum, v) => sum + v.trafficPercentage, 0);
    const totalElement = document.getElementById('totalTraffic');
    
    totalElement.textContent = total.toFixed(0) + '%';
    totalElement.className = 'badge';
    
    if (total === 100) {
        totalElement.classList.add('bg-success');
    } else if (total > 100) {
        totalElement.classList.add('bg-danger');
    } else {
        totalElement.classList.add('bg-warning');
    }
}

/**
 * Create a new variant
 */
function createVariant() {
    const data = {
        name: document.getElementById('variantName').value,
        description: document.getElementById('variantDescription').value,
        trafficPercentage: parseFloat(document.getElementById('variantTraffic').value),
        isControl: document.getElementById('isControl').checked,
        channel: 0,
        subject: document.getElementById('variantSubject').value,
        messageBody: document.getElementById('variantMessage').value
    };

    const url = window.AppUrls ? 
        window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.variants.create(campaignId)) : 
        `/api/campaigns/${campaignId}/variants`;

    fetch(url, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(data)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            const modal = bootstrap.Modal.getInstance(document.getElementById('createVariantModal'));
            modal.hide();
            document.getElementById('createVariantForm').reset();
            loadVariants();
            showNotification('Variant created successfully!', 'success');
        }
    })
    .catch(error => {
        showNotification('Error creating variant: ' + error.message, 'error');
        console.error('Error creating variant:', error);
    });
}

/**
 * Delete a variant
 */
function deleteVariant(variantId) {
    if (!confirm('Are you sure you want to delete this variant?')) return;

    const url = window.AppUrls ? 
        window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.variants.delete(campaignId, variantId)) : 
        `/api/campaigns/${campaignId}/variants/${variantId}`;

    fetch(url, {
        method: 'DELETE',
        headers: getAuthHeaders()
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            loadVariants();
            showNotification('Variant deleted successfully!', 'success');
        }
    })
    .catch(error => {
        showNotification('Error deleting variant', 'error');
        console.error('Error deleting variant:', error);
    });
}

/**
 * Load variant comparison data
 */
function loadComparison() {
    const url = window.AppUrls ? 
        window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.variants.comparison(campaignId)) : 
        `/api/campaigns/${campaignId}/variants/comparison`;

    fetch(url, {
        method: 'GET',
        headers: getAuthHeaders()
    })
    .then(response => response.json())
    .then(data => {
        if (data.success && data.data) {
            renderComparison(data.data);
        }
    })
    .catch(error => {
        document.getElementById('comparisonContent').innerHTML = 
            '<div class="alert alert-danger">Failed to load comparison</div>';
        console.error('Error loading comparison:', error);
    });
}

/**
 * Render variant comparison results
 */
function renderComparison(comparison) {
    let html = '';
    
    if (comparison.recommendedAction) {
        html += `<div class="alert alert-info">${comparison.recommendedAction}</div>`;
    }
    
    if (comparison.winningVariant) {
        html += `<div class="alert alert-success">
            <strong>Top Performer:</strong> ${comparison.winningVariant.name}
            <br>Click Rate: ${comparison.winningVariant.analytics.clickRate.toFixed(2)}%
            | Conversion Rate: ${comparison.winningVariant.analytics.conversionRate.toFixed(2)}%
        </div>`;
    }

    if (comparison.variants && comparison.variants.length > 0) {
        html += '<div class="table-responsive"><table class="table table-hover">';
        html += '<thead><tr><th>Variant</th><th>Traffic</th><th>Sent</th><th>Delivery Rate</th><th>Click Rate</th><th>Conversion Rate</th><th>Action</th></tr></thead><tbody>';
        
        comparison.variants.forEach(v => {
            html += `<tr>
                <td>${v.name} ${v.isControl ? '<span class="badge bg-primary">Control</span>' : ''}</td>
                <td>${v.trafficPercentage}%</td>
                <td>${v.sentCount}</td>
                <td>${v.analytics ? v.analytics.deliveryRate.toFixed(2) : '0'}%</td>
                <td>${v.analytics ? v.analytics.clickRate.toFixed(2) : '0'}%</td>
                <td>${v.analytics ? v.analytics.conversionRate.toFixed(2) : '0'}%</td>
                <td><button class="btn btn-sm btn-success" data-action="select-winner" data-variant-id="${v.id}">Select Winner</button></td>
            </tr>`;
        });
        html += '</tbody></table></div>';
    }

    document.getElementById('comparisonContent').innerHTML = html;
}

/**
 * Select a variant as the winner
 */
function selectWinner(variantId) {
    if (!confirm('Are you sure you want to select this variant as the winner?')) return;

    const url = window.AppUrls ? 
        window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.variants.selectWinner(campaignId, variantId)) : 
        `/api/campaigns/${campaignId}/variants/${variantId}/select-winner`;

    fetch(url, {
        method: 'POST',
        headers: getAuthHeaders()
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('Winner selected successfully!', 'success');
            loadComparison();
        }
    })
    .catch(error => {
        showNotification('Error selecting winner', 'error');
        console.error('Error selecting winner:', error);
    });
}
