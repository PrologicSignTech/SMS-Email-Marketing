/**
 * billing-page.js - Billing page functionality
 * Handles subscription display, invoice management, and payment methods
 */

const token = localStorage.getItem('token');

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    loadSubscription();
    loadRecentInvoices();
});

/**
 * Load current subscription
 */
async function loadSubscription() {
    try {
        const response = await fetch(window.AppUrls.buildApiUrl(window.AppUrls.api.billing.subscription), {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to load subscription');
        }

        const result = await response.json();
        
        if (result.success && result.data) {
            displaySubscription(result.data);
        } else {
            displayNoSubscription();
        }
    } catch (error) {
        console.error('Error loading subscription:', error);
        displayNoSubscription();
    }
}

/**
 * Display subscription information
 * @param {Object} subscription - Subscription data
 */
function displaySubscription(subscription) {
    const container = document.getElementById('currentSubscription');
    if (!container) return;
    
    const html = `
        <div class="row">
            <div class="col-md-8">
                <h4>${subscription.planName}</h4>
                <p class="text-muted">${subscription.planDescription || ''}</p>
                <div class="mt-3">
                    <span class="badge bg-success">Active</span>
                    <span class="badge bg-info">${subscription.billingCycle}</span>
                </div>
            </div>
            <div class="col-md-4 text-end">
                <h3 class="text-primary">$${subscription.amount}/month</h3>
                <div class="mt-3">
                    <a href="/billing/subscribe" class="btn btn-outline-primary btn-sm me-2">
                        <i class="bi bi-arrow-up-circle"></i> Upgrade
                    </a>
                    <button class="btn btn-outline-danger btn-sm" onclick="cancelSubscription()">
                        <i class="bi bi-x-circle"></i> Cancel
                    </button>
                </div>
            </div>
        </div>
        <hr>
        <div class="row mt-3">
            <div class="col-md-6">
                <p><strong>Started:</strong> ${new Date(subscription.startDate).toLocaleDateString()}</p>
                <p><strong>Status:</strong> ${subscription.status}</p>
            </div>
            <div class="col-md-6">
                <p><strong>Next Billing:</strong> ${subscription.nextBillingDate ? new Date(subscription.nextBillingDate).toLocaleDateString() : 'N/A'}</p>
                <p><strong>Auto Renew:</strong> ${subscription.autoRenew ? 'Yes' : 'No'}</p>
            </div>
        </div>
    `;
    container.innerHTML = html;

    // Update quick stats
    const nextBillingEl = document.getElementById('nextBillingDate');
    if (nextBillingEl && subscription.nextBillingDate) {
        nextBillingEl.textContent = new Date(subscription.nextBillingDate).toLocaleDateString();
    }
    const amountDueEl = document.getElementById('amountDue');
    if (amountDueEl) {
        amountDueEl.textContent = `$${subscription.amount.toFixed(2)}`;
    }
}

/**
 * Display no subscription message
 */
function displayNoSubscription() {
    const container = document.getElementById('currentSubscription');
    if (!container) return;
    
    const html = `
        <div class="text-center py-5">
            <i class="bi bi-inbox" style="font-size: 3rem; color: #ccc;"></i>
            <h5 class="mt-3">No Active Subscription</h5>
            <p class="text-muted">Subscribe to a plan to start using our services</p>
            <a href="/billing/subscribe" class="btn btn-primary mt-3">
                <i class="bi bi-box-seam"></i> View Plans
            </a>
        </div>
    `;
    container.innerHTML = html;
}

/**
 * Load recent invoices
 */
async function loadRecentInvoices() {
    try {
        const response = await fetch(window.AppUrls.buildApiUrl(window.AppUrls.api.billing.invoices), {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to load invoices');
        }

        const result = await response.json();
        
        if (result.success && result.data && result.data.length > 0) {
            displayInvoices(result.data.slice(0, 5));
        } else {
            displayNoInvoices();
        }
    } catch (error) {
        console.error('Error loading invoices:', error);
        displayNoInvoices();
    }
}

/**
 * Display invoices in table
 * @param {Array} invoices - Array of invoice objects
 */
function displayInvoices(invoices) {
    const container = document.getElementById('recentInvoices');
    if (!container) return;
    
    const html = `
        <div class="table-responsive">
            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Invoice #</th>
                        <th>Date</th>
                        <th>Amount</th>
                        <th>Status</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    ${invoices.map(invoice => `
                        <tr>
                            <td>${invoice.invoiceNumber}</td>
                            <td>${new Date(invoice.invoiceDate).toLocaleDateString()}</td>
                            <td>$${invoice.amount.toFixed(2)}</td>
                            <td>
                                <span class="badge bg-${getStatusColor(invoice.status)}">
                                    ${invoice.status}
                                </span>
                            </td>
                            <td>
                                <button class="btn btn-sm btn-outline-primary" onclick="downloadInvoice('${invoice.id}')">
                                    <i class="bi bi-download"></i> Download
                                </button>
                            </td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;
    container.innerHTML = html;
}

/**
 * Display no invoices message
 */
function displayNoInvoices() {
    const container = document.getElementById('recentInvoices');
    if (container) {
        container.innerHTML = `
            <div class="text-center py-3 text-muted">
                <i class="bi bi-receipt"></i> No invoices yet
            </div>
        `;
    }
}

/**
 * Get Bootstrap color class for invoice status
 * @param {string} status - Invoice status
 * @returns {string} Bootstrap color class
 */
function getStatusColor(status) {
    switch (status.toLowerCase()) {
        case 'paid': return 'success';
        case 'pending': return 'warning';
        case 'failed': return 'danger';
        default: return 'secondary';
    }
}

/**
 * Cancel subscription
 */
async function cancelSubscription() {
    if (!confirm('Are you sure you want to cancel your subscription?')) {
        return;
    }

    const reason = prompt('Please provide a reason for cancellation (optional):');
    
    try {
        const response = await fetch(window.AppUrls.buildApiUrl(window.AppUrls.api.billing.cancel), {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ reason: reason || 'User cancelled' })
        });

        const result = await response.json();

        if (result.success) {
            if (typeof showNotification === 'function') {
                showNotification('Subscription cancelled successfully', 'success');
            } else {
                alert('Subscription cancelled successfully');
            }
            setTimeout(() => location.reload(), 2000);
        } else {
            const errorMsg = 'Failed to cancel subscription: ' + (result.message || 'Unknown error');
            if (typeof showNotification === 'function') {
                showNotification(errorMsg, 'error');
            } else {
                alert(errorMsg);
            }
        }
    } catch (error) {
        console.error('Error cancelling subscription:', error);
        if (typeof showNotification === 'function') {
            showNotification('Failed to cancel subscription', 'error');
        } else {
            alert('Failed to cancel subscription');
        }
    }
}

/**
 * Add payment method (placeholder)
 */
function addPaymentMethod() {
    if (typeof showNotification === 'function') {
        showNotification('Payment method management will be available soon', 'info');
    } else {
        alert('Payment method management will be available soon');
    }
}

/**
 * Download invoice
 * @param {string} invoiceId - Invoice ID
 */
function downloadInvoice(invoiceId) {
    window.open(window.AppUrls.buildApiUrl(window.AppUrls.api.billing.downloadInvoice(invoiceId)), '_blank');
}

// Make functions available globally for event handlers
if (typeof window !== 'undefined') {
    window.cancelSubscription = cancelSubscription;
    window.addPaymentMethod = addPaymentMethod;
    window.downloadInvoice = downloadInvoice;
}
