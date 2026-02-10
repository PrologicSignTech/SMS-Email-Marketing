/**
 * landing-features.js - Dynamic landing page features with flip cards
 * Loads features from database and displays with interactive flip functionality
 * Version: 2.0 - Fixed hover flip issue (only buttons trigger flip)
 */

console.log('Landing Features JS v2.0 loaded - Flip on button click only');

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    loadLandingFeatures();
});

/**
 * Load landing features from API
 */
async function loadLandingFeatures() {
    const container = document.getElementById('features-cards');
    if (!container) {
        console.error('features-cards container not found');
        return;
    }

    try {
        // Build full API URL using AppUrls helper - NO STATIC URLS
        const apiUrl = window.AppUrls ?
            window.AppUrls.buildApiUrl(window.AppUrls.api.landingFeatures.list) :
            '/api/landingfeatures';

        console.log('Fetching landing features from:', apiUrl);

        // Fetch features from API
        const response = await fetch(apiUrl);

        console.log('Response status:', response.status);

        if (!response.ok) {
            const errorText = await response.text();
            console.error('API error response:', errorText);
            throw new Error('Failed to load features');
        }

        const result = await response.json();
        console.log('API result:', result);

        if (result.success && result.data && result.data.length > 0) {
            console.log('Rendering', result.data.length, 'features');
            renderFeatures(result.data);
        } else {
            console.warn('No features data:', result);
            container.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-info text-center">
                        <i class="bi bi-info-circle"></i> No features available at this time.
                    </div>
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading features:', error);
        container.innerHTML = `
            <div class="col-12">
                <div class="alert alert-warning text-center">
                    <i class="bi bi-exclamation-triangle"></i> Unable to load features at this time.
                    <br><small class="text-muted">Error: ${error.message}</small>
                </div>
            </div>
        `;
    }
}

/**
 * Render features in the container with flip card functionality
 * @param {Array} features - Array of feature objects
 */
function renderFeatures(features) {
    const container = document.getElementById('features-cards');
    console.log('renderFeatures called with:', features);

    let html = '';
    features.forEach((feature, index) => {
        console.log(`Rendering feature ${index + 1}:`, feature.title);
        console.log(`  Stats: 1="${feature.statValue1}" 2="${feature.statValue2}" 3="${feature.statValue3}"`);

        // Calculate delay for staggered animation
        const delay = (index % 3) * 100 + 100;

        html += `
            <div class="col-md-6 col-lg-4" data-aos="fade-up" data-aos-delay="${delay}">
                <div class="flip-card-container">
                    <div class="flip-card" id="feature-card-${feature.id}">
                        <!-- Front Side -->
                        <div class="flip-card-front feature-card h-100">
                            <div class="feature-icon-wrapper mb-4 text-center">
                                <div class="feature-icon-bg bg-${feature.colorClass}"></div>
                                <i class="bi ${feature.iconClass} feature-icon-lg text-${feature.colorClass}"></i>
                            </div>
                            <h4 class="fw-bold mb-3 text-center">${escapeHtml(feature.title)}</h4>
                            <p class="text-muted mb-4">
                                ${escapeHtml(feature.shortDescription)}
                            </p>
                            <a href="#" class="text-${feature.colorClass} fw-semibold text-decoration-none flip-trigger" data-feature-id="${feature.id}">
                                Learn more <i class="bi bi-arrow-right"></i>
                            </a>
                        </div>

                        <!-- Back Side -->
                        <div class="flip-card-back feature-card h-100 bg-${feature.colorClass} text-white">
                            <div class="d-flex flex-column h-100 position-relative">
                                <!-- Back Button at Top -->
                                <div class="mb-3">
                                    <button class="btn btn-outline-light btn-sm flip-back-trigger" data-feature-id="${feature.id}">
                                        <i class="bi bi-arrow-left"></i> Back
                                    </button>
                                </div>

                                <!-- Content -->
                                <div class="flex-grow-1 overflow-auto mb-3">
                                    <h4 class="fw-bold mb-3">${escapeHtml(feature.title)}</h4>
                                    <p class="mb-0 small" style="line-height: 1.6;">
                                        ${escapeHtml(feature.detailedDescription)}
                                    </p>
                                </div>

                                <!-- View Details Button at Bottom -->
                                <div class="mt-auto">
                                    <a href="/features/${feature.id}" class="btn btn-light w-100">
                                        <i class="bi bi-eye"></i> View Full Details
                                    </a>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });

    console.log('Setting container HTML, length:', html.length);
    container.innerHTML = html;
    console.log('Features rendered successfully!');

    // Initialize flip card interactions
    initializeFlipCards();

    // Re-initialize scroll animations for dynamically loaded content
    
}

/**
 * Initialize flip card click handlers using event delegation
 */
let flipCardsInitialized = false; // Prevent multiple initializations

function initializeFlipCards() {
    const container = document.getElementById('features-cards');
    if (!container) return;

    // Only initialize once to prevent event listener stacking
    if (flipCardsInitialized) {
        console.log('Flip cards already initialized, skipping');
        return;
    }

    // Use event delegation for better reliability with dynamic content
    container.addEventListener('click', function(e) {
        // Don't interfere with "View Full Details" links
        if (e.target.closest('a[href^="/features/"]')) {
            console.log('Allowing navigation to detail page');
            return; // Let the link work normally
        }

        // Check if clicked element is a flip trigger or inside one
        const flipTrigger = e.target.closest('.flip-trigger');
        const flipBackTrigger = e.target.closest('.flip-back-trigger');

        if (flipTrigger) {
            e.preventDefault();
            e.stopPropagation();
            const featureId = flipTrigger.getAttribute('data-feature-id');
            const card = document.getElementById(`feature-card-${featureId}`);
            
            // ONLY flip this specific card forward, don't touch other cards
            if (card && !card.classList.contains('flipped')) {
                console.log('Flipping card forward:', featureId);
                card.classList.add('flipping');
                card.classList.add('flipped');
                // Remove flipping class after transition
                setTimeout(() => card.classList.remove('flipping'), 600);
            }
        } else if (flipBackTrigger) {
            e.preventDefault();
            e.stopPropagation();
            const featureId = flipBackTrigger.getAttribute('data-feature-id');
            const card = document.getElementById(`feature-card-${featureId}`);
            
            // ONLY flip this specific card back, don't touch other cards
            if (card && card.classList.contains('flipped')) {
                console.log('Flipping card back:', featureId);
                card.classList.add('flipping');
                card.classList.remove('flipped');
                // Remove flipping class after transition
                setTimeout(() => card.classList.remove('flipping'), 600);
            }
        }
    });

    flipCardsInitialized = true; // Mark as initialized
    console.log('Flip card interactions initialized with event delegation');

    reinitScrollAnimations();
}

/**
 * Re-initialize scroll animations for dynamically loaded feature cards
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

    // Observe feature card elements with data-aos attribute
    const featureCards = document.querySelectorAll('#features-cards [data-aos]');
    featureCards.forEach(el => observer.observe(el));

    console.log('Reinitialized scroll animations for', featureCards.length, 'feature cards');
    setTimeout(() => {
        featureCards.forEach(item => {
            if (!item.classList.contains('aos-animate')) {
                item.classList.add('aos-animate');
            }
        });
        console.log('Forced animation on', featureCards.length, 'Feature cards');
    }, 100);
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
