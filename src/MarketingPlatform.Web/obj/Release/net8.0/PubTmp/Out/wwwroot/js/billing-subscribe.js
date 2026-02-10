/**
 * Billing Subscribe Page JavaScript
 * Handles subscription plan selection and payment processing
 */

let selectedPlan = null;
let billingCycle = 'monthly';
let stripe = null;

document.addEventListener('DOMContentLoaded', function() {
    // Initialize Stripe if key is available
    if (window.billingSubscribeConfig && window.billingSubscribeConfig.stripePublishableKey) {
        stripe = Stripe(window.billingSubscribeConfig.stripePublishableKey);
    }

    setupEventListeners();
    loadPlans();
});

/**
 * Setup event listeners for billing cycle toggle and payment button
 */
function setupEventListeners() {
    // Billing cycle change
    const billingRadios = document.querySelectorAll('input[name="billingCycle"]');
    billingRadios.forEach(radio => {
        radio.addEventListener('change', handleBillingCycleChange);
    });

    // Payment button
    const paymentBtn = document.querySelector('button[data-action="process-payment"]');
    if (paymentBtn) {
        paymentBtn.addEventListener('click', processPayment);
    }
}

/**
 * Handle billing cycle toggle
 */
function handleBillingCycleChange(e) {
    billingCycle = e.target.value;
    
    // Toggle price display
    document.querySelectorAll('.price-monthly, .yearly-note').forEach(el => {
        el.classList.toggle('d-none', billingCycle === 'yearly');
    });
    document.querySelectorAll('.price-yearly').forEach(el => {
        el.classList.toggle('d-none', billingCycle === 'monthly');
    });
}

/**
 * Load subscription plans from API
 */
async function loadPlans() {
    try {
        const token = localStorage.getItem('token');

        const response = await fetch(window.AppUrls.buildApiUrl(window.AppUrls.api.subscriptionPlans.visible), {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to load plans');
        }

        const result = await response.json();
        
        if (result.success && result.data) {
            displayPlans(result.data);
        } else {
            displayError('No plans available');
        }
    } catch (error) {
        console.error('Error loading plans:', error);
        displayError('Failed to load subscription plans');
    }
}

/**
 * Display subscription plans
 */
function displayPlans(plans) {
    const container = document.getElementById('subscriptionPlans');
    if (!container) return;
    
    if (plans.length === 0) {
        container.innerHTML = `
            <div class="col-12 text-center py-5">
                <i class="bi bi-inbox" style="font-size: 3rem; color: #ccc;"></i>
                <h5 class="mt-3">No Plans Available</h5>
                <p class="text-muted">Please check back later</p>
            </div>
        `;
        return;
    }

    const html = plans.map((plan, index) => {
        const features = plan.features ? JSON.parse(plan.features) : [];
        const isPopular = index === 1; // Middle plan is popular
        const monthlyPrice = plan.priceMonthly || 0;
        const yearlyPrice = plan.priceYearly || (monthlyPrice * 12 * 0.8);

        return `
            <div class="col-lg-4 col-md-6 mb-4">
                <div class="card h-100 shadow-sm ${isPopular ? 'border-primary' : ''}">
                    ${isPopular ? '<div class="card-header bg-primary text-white text-center"><small><i class="bi bi-star-fill"></i> Most Popular</small></div>' : ''}
                    <div class="card-body d-flex flex-column">
                        <h3 class="card-title">${plan.name}</h3>
                        <p class="text-muted">${plan.description || ''}</p>
                        
                        <div class="mb-4">
                            <h2 class="mb-0">
                                <span class="price-monthly ${billingCycle === 'yearly' ? 'd-none' : ''}">
                                    $${monthlyPrice.toFixed(2)}
                                </span>
                                <span class="price-yearly ${billingCycle === 'monthly' ? 'd-none' : ''}">
                                    $${(yearlyPrice / 12).toFixed(2)}
                                </span>
                                <small class="text-muted">/month</small>
                            </h2>
                            <small class="text-muted yearly-note ${billingCycle === 'monthly' ? 'd-none' : ''}">
                                Billed yearly at $${yearlyPrice.toFixed(2)}
                            </small>
                        </div>

                        <ul class="list-unstyled mb-4 flex-grow-1">
                            ${features.map(feature => `
                                <li class="mb-2">
                                    <i class="bi bi-check-circle text-success"></i> ${feature}
                                </li>
                            `).join('')}
                        </ul>

                        <button class="btn btn-${isPopular ? 'primary' : 'outline-primary'} w-100" 
                                data-action="select-plan" 
                                data-plan-id="${plan.id}"
                                data-plan-name="${plan.name}"
                                data-monthly-price="${monthlyPrice}"
                                data-yearly-price="${yearlyPrice}">
                            <i class="bi bi-box-seam"></i> Select Plan
                        </button>
                    </div>
                </div>
            </div>
        `;
    }).join('');

    container.innerHTML = html;

    // Setup select plan button listeners
    document.querySelectorAll('button[data-action="select-plan"]').forEach(btn => {
        btn.addEventListener('click', function() {
            const planId = parseInt(this.dataset.planId);
            const planName = this.dataset.planName;
            const monthlyPrice = parseFloat(this.dataset.monthlyPrice);
            const yearlyPrice = parseFloat(this.dataset.yearlyPrice);
            selectPlan(planId, planName, monthlyPrice, yearlyPrice);
        });
    });
}

/**
 * Display error message
 */
function displayError(message) {
    const container = document.getElementById('subscriptionPlans');
    if (container) {
        container.innerHTML = `
            <div class="col-12 text-center py-5">
                <i class="bi bi-exclamation-triangle text-warning" style="font-size: 3rem;"></i>
                <h5 class="mt-3">${message}</h5>
            </div>
        `;
    }
}

/**
 * Select a plan
 */
function selectPlan(planId, planName, monthlyPrice, yearlyPrice) {
    selectedPlan = {
        id: planId,
        name: planName,
        price: billingCycle === 'monthly' ? monthlyPrice : yearlyPrice,
        cycle: billingCycle
    };

    // Display selected plan details
    const detailsContainer = document.getElementById('selectedPlanDetails');
    if (detailsContainer) {
        detailsContainer.innerHTML = `
            <div class="card">
                <div class="card-body">
                    <h5>${planName}</h5>
                    <p class="mb-0">
                        <strong>$${selectedPlan.price.toFixed(2)}</strong>
                        <span class="text-muted">/${billingCycle === 'monthly' ? 'month' : 'year'}</span>
                    </p>
                </div>
            </div>
        `;
    }

    // Show payment modal
    const modalElement = document.getElementById('paymentModal');
    if (modalElement) {
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
}

/**
 * Process payment and create subscription
 */
async function processPayment() {
    if (!selectedPlan) {
        showNotification('Please select a plan first', 'warning');
        return;
    }

    try {
        const token = localStorage.getItem('token');

        // Create subscription
        const response = await fetch(window.AppUrls.buildApiUrl(window.AppUrls.api.billing.subscribe), {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                subscriptionPlanId: selectedPlan.id,
                billingCycle: selectedPlan.cycle,
                paymentMethodId: 'stripe' // Would be actual payment method ID from Stripe
            })
        });

        const result = await response.json();

        if (result.success) {
            showNotification('Subscription activated successfully!', 'success');
            setTimeout(() => {
                window.location.href = '/Billing';
            }, 1500);
        } else {
            showNotification('Failed to activate subscription: ' + (result.message || 'Unknown error'), 'error');
        }
    } catch (error) {
        console.error('Error processing payment:', error);
        showNotification('Failed to process payment', 'error');
    }
}
