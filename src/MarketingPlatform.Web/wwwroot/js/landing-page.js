/**
 * landing-page.js - Landing page functionality
 * Handles pricing plans loading and display
 */

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    loadPricingPlans();
});

/**
 * Load pricing plans from API
 */
async function loadPricingPlans() {
    const container = document.getElementById('pricing-plans');
    if (!container) {
        console.error('pricing-plans container not found');
        return;
    }

    try {
        // Build full API URL using AppUrls helper - NO STATIC URLS
        const apiUrl = window.AppUrls ?
            window.AppUrls.buildApiUrl(window.AppUrls.api.subscriptionPlans.landing) :
            '/api/subscriptionplans/landing';

        console.log('Fetching pricing plans from:', apiUrl);

        // Fetch plans from API that are marked to show on landing page
        const response = await fetch(apiUrl);

        console.log('Response status:', response.status);

        if (!response.ok) {
            const errorText = await response.text();
            console.error('API error response:', errorText);
            throw new Error('Failed to load pricing plans');
        }

        const result = await response.json();
        console.log('API result:', result);

        if (result.success && result.data && result.data.length > 0) {
            console.log('Rendering', result.data.length, 'plans');
            renderPricingPlans(result.data);
        } else {
            console.warn('No plans data:', result);
            container.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-info text-center">
                        <i class="bi bi-info-circle"></i> No pricing plans available at this time.
                    </div>
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading pricing plans:', error);
        container.innerHTML = `
            <div class="col-12">
                <div class="alert alert-warning text-center">
                    <i class="bi bi-exclamation-triangle"></i> Unable to load pricing plans at this time.
                    <br><small class="text-muted">Error: ${error.message}</small>
                </div>
            </div>
        `;
    }
}

/**
 * Render pricing plans in the container
 * @param {Array} plans - Array of pricing plan objects
 */
function renderPricingPlans(plans) {
    const container = document.getElementById('pricing-plans');
    console.log('renderPricingPlans called with:', plans);

    let html = '';
    plans.forEach((plan, index) => {
        console.log(`Rendering plan ${index + 1}:`, plan.name, 'Features:', plan.planFeatures?.length);
        // Use IsMostPopular flag from database
        const isPopular = plan.isMostPopular === true;
        const popularBadge = isPopular ?
            '<div class="position-absolute top-0 start-50 translate-middle" style="z-index: 10;"><span class="badge px-4 py-2 shadow" style="background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); color: white; font-size: 0.875rem; font-weight: 600; border-radius: 50px; letter-spacing: 0.5px;"><i class="bi bi-star-fill me-1"></i>MOST POPULAR</span></div>' : '';
        const cardClass = isPopular ? 'border-primary border-3 shadow-lg' : 'border shadow-sm';

        // Build features list from database
        let featuresList = '';
        if (plan.planFeatures && plan.planFeatures.length > 0) {
            // Get all features (both included and not included for comparison)
            const allPlanFeatures = plan.planFeatures;

            featuresList = allPlanFeatures.map(pf => {
                if (!pf.isIncluded) {
                    // Show features that are NOT included with an X mark
                    return `
                        <li class="mb-3 d-flex align-items-start">
                            <i class="bi bi-x-circle text-muted me-2 mt-1 flex-shrink-0"></i>
                            <span class="text-muted"><s>${escapeHtml(pf.featureName)}</s></span>
                        </li>
                    `;
                }

                // Show included features with check mark and value if available
                const featureText = pf.featureValue ?
                    `<strong>${escapeHtml(pf.featureName)}</strong>: ${escapeHtml(pf.featureValue)}` :
                    `<strong>${escapeHtml(pf.featureName)}</strong>`;

                const description = pf.featureDescription ?
                    `<br><small class="text-muted">${escapeHtml(pf.featureDescription)}</small>` : '';

                return `
                    <li class="mb-3 d-flex align-items-start">
                        <i class="bi bi-check-circle-fill text-success me-2 mt-1 flex-shrink-0"></i>
                        <span>${featureText}${description}</span>
                    </li>
                `;
            }).join('');
        } else {
            // Fallback to legacy features
            let features = [];
            try {
                features = plan.features ? (typeof plan.features === 'string' ? JSON.parse(plan.features) : plan.features) : [];
                if (Array.isArray(features)) {
                    featuresList = features.map(feature => `
                        <li class="mb-3 d-flex align-items-start">
                            <i class="bi bi-check-circle-fill text-success me-2 mt-1 flex-shrink-0"></i>
                            <span>${escapeHtml(feature)}</span>
                        </li>
                    `).join('');
                }
            } catch (e) {
                console.error('Error parsing legacy features for plan:', plan.name, e);
            }
        }

        if (!featuresList) {
            featuresList = '<li class="text-muted">No features listed</li>';
        }

        html += `
            <div class="col-lg-4 col-md-6 mb-4" data-aos="fade-up" data-aos-delay="${index * 100}" style="${isPopular ? 'margin-top: 30px;' : ''}">
                <div class="card h-100 ${cardClass} position-relative" style="overflow: visible;">
                    ${popularBadge}
                    <div class="card-body p-4 ${isPopular ? 'mt-3' : ''} d-flex flex-column">
                        <!-- Plan Header -->
                        <div class="text-center mb-3">
                            <span class="badge px-3 py-2 mb-3" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 1px;">
                                ${escapeHtml(plan.planCategory || 'Standard')}
                            </span>
                            <h3 class="fw-bold mb-2" style="color: #1f2937; font-size: 1.75rem;">${escapeHtml(plan.name)}</h3>
                            <p class="text-muted mb-0" style="font-size: 0.95rem;">${escapeHtml(plan.description || '')}</p>
                        </div>

                        <!-- Pricing -->
                        <div class="text-center mb-4 py-4 bg-light rounded-3">
                            <div class="mb-2">
                                <span class="h2 fw-bold text-primary">$${plan.priceMonthly ? plan.priceMonthly.toFixed(2) : '0.00'}</span>
                                <span class="text-muted">/month</span>
                            </div>
                            ${plan.priceYearly > 0 ? `
                                <div class="small text-muted mb-2">
                                    or $${plan.priceYearly.toFixed(2)}/year
                                </div>
                                <div class="badge bg-success bg-opacity-10">
                                    Save ${calculateYearlySavings(plan.priceMonthly, plan.priceYearly)}% yearly
                                </div>
                            ` : ''}
                        </div>

                        <!-- Features Section -->
                        <div class="mb-4 flex-grow-1">
                            <h6 class="text-uppercase text-muted small fw-bold mb-3">
                                <i class="bi bi-list-check"></i> Features Included
                            </h6>
                            <ul class="list-unstyled">
                                ${featuresList}
                            </ul>
                        </div>

                        <!-- CTA Button -->
                        <div class="text-center mt-auto">
                            <a href="/Auth/Register?plan=${plan.id}"
                               class="btn ${isPopular ? 'btn-primary' : 'btn-outline-primary'} w-100 py-3 fw-semibold">
                                <i class="bi bi-rocket-takeoff"></i> Get Started with ${escapeHtml(plan.name)}
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });

    console.log('Setting container HTML, length:', html.length);
    container.innerHTML = html;
    console.log('Pricing plans rendered successfully!');

    // Re-initialize scroll animations for dynamically loaded content
    reinitScrollAnimations();

    // Force immediate visibility as fallback
    setTimeout(() => {
        const cards = document.querySelectorAll('#pricing-plans [data-aos]');
        cards.forEach(card => {
            if (!card.classList.contains('aos-animate')) {
                card.classList.add('aos-animate');
            }
        });
        console.log('Forced animation on', cards.length, 'pricing cards');
    }, 100);
}

/**
 * Re-initialize scroll animations for dynamically loaded pricing cards
 */
function reinitScrollAnimations() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('aos-animate');
            }
        });
    }, observerOptions);

    // Observe pricing card elements with data-aos attribute
    const pricingCards = document.querySelectorAll('#pricing-plans [data-aos]');
    pricingCards.forEach(el => observer.observe(el));

    console.log('Reinitialized scroll animations for', pricingCards.length, 'pricing cards');
}

/**
 * Calculate yearly savings percentage
 * @param {number} monthly - Monthly price
 * @param {number} yearly - Yearly price
 * @returns {number} Percentage savings
 */
function calculateYearlySavings(monthly, yearly) {
    if (!monthly || !yearly || yearly <= 0) return 0;
    const monthlyCost = monthly * 12;
    const savings = ((monthlyCost - yearly) / monthlyCost) * 100;
    return Math.round(savings);
}

/**
 * Escape HTML to prevent XSS
 * @param {string} text - Text to escape
 * @returns {string} Escaped text
 */
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

