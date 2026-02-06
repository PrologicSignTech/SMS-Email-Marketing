/**
 * landing-faqs.js - Dynamic landing page FAQs
 * Loads FAQs from database and displays with accordion
 */

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    loadLandingFaqs();
});

/**
 * Load landing FAQs from API
 */
async function loadLandingFaqs() {
    const container = document.getElementById('faq-accordion');
    if (!container) {
        console.error('faq-accordion container not found');
        return;
    }

    try {
        // Build full API URL using AppUrls helper
        const apiUrl = window.AppUrls ?
            window.AppUrls.buildApiUrl(window.AppUrls.api.landingFaqs.list) :
            '/api/landingfaqs';

        console.log('Fetching landing FAQs from:', apiUrl);

        const response = await fetch(apiUrl);
        console.log('Response status:', response.status);

        if (!response.ok) {
            const errorText = await response.text();
            console.error('API error response:', errorText);
            throw new Error('Failed to load FAQs');
        }

        const result = await response.json();
        console.log('API result:', result);

        if (result.success && result.data && result.data.length > 0) {
            console.log('Rendering', result.data.length, 'FAQs');
            renderFaqs(result.data);
        } else {
            console.warn('No FAQs data:', result);
            container.innerHTML = `
                <div class="alert alert-info text-center">
                    <i class="bi bi-info-circle"></i> No FAQs available at this time.
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading FAQs:', error);
        container.innerHTML = `
            <div class="alert alert-warning text-center">
                <i class="bi bi-exclamation-triangle"></i> Unable to load FAQs at this time.
                <br><small class="text-muted">Error: ${error.message}</small>
            </div>
        `;
    }
}

/**
 * Render FAQs in accordion format
 * @param {Array} faqs - Array of FAQ objects
 */
function renderFaqs(faqs) {
    const container = document.getElementById('faq-accordion');
    console.log('renderFaqs called with:', faqs);

    let html = '';
    faqs.forEach((faq, index) => {
        console.log(`Rendering FAQ ${index + 1}:`, faq.question);

        const delay = (index % 4) * 100 + 100;
        const collapseId = `faq${faq.id}`;
        const isFirst = index === 0;

        html += `
            <div class="accordion-item border-0 shadow-sm mb-3 rounded-3" data-aos="fade-up" data-aos-delay="${delay}">
                <h2 class="accordion-header">
                    <button class="accordion-button ${isFirst ? '' : 'collapsed'} rounded-3 fw-semibold"
                            type="button"
                            data-bs-toggle="collapse"
                            data-bs-target="#${collapseId}"
                            aria-expanded="${isFirst ? 'true' : 'false'}"
                            aria-controls="${collapseId}">
                        <i class="bi ${escapeHtml(faq.iconClass)} text-${escapeHtml(faq.iconColor)} me-2"></i>
                        ${escapeHtml(faq.question)}
                    </button>
                </h2>
                <div id="${collapseId}"
                     class="accordion-collapse collapse ${isFirst ? 'show' : ''}"
                     data-bs-parent="#faqAccordion">
                    <div class="accordion-body">
                        ${faq.answer}
                    </div>
                </div>
            </div>
        `;
    });

    console.log('Setting container HTML, length:', html.length);
    container.innerHTML = html;
    console.log('FAQs rendered successfully!');

    // Re-initialize scroll animations
    reinitScrollAnimations();
}

/**
 * Re-initialize scroll animations for dynamically loaded FAQs
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

    // Observe FAQ elements with data-aos attribute
    const faqItems = document.querySelectorAll('#faq-accordion [data-aos]');
    faqItems.forEach(el => observer.observe(el));

    console.log('Reinitialized scroll animations for', faqItems.length, 'FAQ items');

    // Force immediate visibility as fallback
    setTimeout(() => {
        faqItems.forEach(item => {
            if (!item.classList.contains('aos-animate')) {
                item.classList.add('aos-animate');
            }
        });
        console.log('Forced animation on', faqItems.length, 'FAQ items');
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
