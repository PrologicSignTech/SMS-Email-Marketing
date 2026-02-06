/**
 * landing-security.js - Dynamic landing page security badges
 * Loads security badges from database and displays them
 */

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    loadSecurityBadges();
});

/**
 * Load security badges from API
 */
async function loadSecurityBadges() {
    const container = document.getElementById('security-badges-container');
    if (!container) {
        console.error('security-badges-container not found');
        return;
    }

    try {
        // Build full API URL using AppUrls helper
        const apiUrl = window.AppUrls ?
            window.AppUrls.buildApiUrl(window.AppUrls.api.securityBadges.list) :
            '/api/securitybadges';

        console.log('Fetching security badges from:', apiUrl);

        const response = await fetch(apiUrl);
        console.log('Response status:', response.status);

        if (!response.ok) {
            const errorText = await response.text();
            console.error('API error response:', errorText);
            throw new Error('Failed to load security badges');
        }

        const result = await response.json();
        console.log('API result:', result);

        if (result.success && result.data && result.data.length > 0) {
            console.log('Rendering', result.data.length, 'security badges');
            renderSecurityBadges(result.data);
        } else {
            console.warn('No security badges data:', result);
            container.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-info text-center">
                        <i class="bi bi-info-circle"></i> No security badges available at this time.
                    </div>
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading security badges:', error);
        container.innerHTML = `
            <div class="col-12">
                <div class="alert alert-warning text-center">
                    <i class="bi bi-exclamation-triangle"></i> Unable to load security badges at this time.
                    <br><small class="text-muted">Error: ${error.message}</small>
                </div>
            </div>
        `;
    }
}

/**
 * Render security badges in the container
 * @param {Array} badges - Array of security badge objects
 */
function renderSecurityBadges(badges) {
    const container = document.getElementById('security-badges-container');
    console.log('renderSecurityBadges called with:', badges);

    let html = '';
    badges.forEach((badge, index) => {
        console.log(`Rendering badge ${index + 1}:`, badge.title);

        // Calculate delay for staggered animation
        const delay = (index + 1) * 100;

        html += `
            <div class="col-6 col-md-3 col-lg-2 text-center" data-aos="zoom-in" data-aos-delay="${delay}">
                <div class="trust-badge-card p-4 h-100">
                    <img src="${escapeHtml(badge.iconUrl)}"
                         alt="${escapeHtml(badge.title)}"
                         class="trust-badge-img mb-3"
                         onerror="this.style.display='none';"
                         title="${escapeHtml(badge.description)}">
                    <div class="fw-semibold text-dark">${escapeHtml(badge.title)}</div>
                    <small class="text-muted">${escapeHtml(badge.subtitle)}</small>
                </div>
            </div>
        `;
    });

    console.log('Setting container HTML, length:', html.length);
    container.innerHTML = html;
    console.log('Security badges rendered successfully!');

    // Re-initialize scroll animations
    reinitScrollAnimations();
}

/**
 * Re-initialize scroll animations for dynamically loaded security badges
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

    // Observe security badge elements with data-aos attribute
    const badgeElements = document.querySelectorAll('#security-badges-container [data-aos]');
    badgeElements.forEach(el => observer.observe(el));

    console.log('Reinitialized scroll animations for', badgeElements.length, 'security badges');

    // Force immediate visibility as fallback
    setTimeout(() => {
        badgeElements.forEach(item => {
            if (!item.classList.contains('aos-animate')) {
                item.classList.add('aos-animate');
            }
        });
        console.log('Forced animation on', badgeElements.length, 'security badges');
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
