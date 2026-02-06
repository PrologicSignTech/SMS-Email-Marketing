/**
 * Landing Stats - Dynamic Stats Loading
 * Loads stats from the API and renders them with counter animations
 */

(function () {
    'use strict';

    // Wait for DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        loadLandingStats();
    });

    /**
     * Load stats from API
     */
    async function loadLandingStats() {
        const container = document.getElementById('stats-container');
        if (!container) {
            console.log('Stats container not found');
            return;
        }

        try {
            const apiBaseUrl = window.appConfig?.apiBaseUrl || '';
            const apiUrl = apiBaseUrl ? `${apiBaseUrl}/api/landingstats` : '/api/landingstats';

            console.log('Loading stats from:', apiUrl);

            const response = await fetch(apiUrl, { credentials: 'include' });
            const result = await response.json();

            if (result.success && result.data && result.data.length > 0) {
                renderStats(container, result.data);
                // Initialize counter animations
                setTimeout(() => initCounterAnimations(), 500);
            } else {
                console.log('No stats data available, using fallback');
                renderFallbackStats(container);
            }
        } catch (error) {
            console.error('Error loading stats:', error);
            renderFallbackStats(container);
        }
    }

    /**
     * Render stats from API data
     */
    function renderStats(container, stats) {
        let html = '';

        stats.forEach((stat, index) => {
            const colorClass = stat.colorClass || 'primary';
            const iconClass = stat.iconClass || 'bi-graph-up';

            html += `
                <div class="col-md-3 col-6" data-aos="zoom-in" data-aos-delay="${100 + (index * 100)}">
                    <div class="stat-item-enhanced">
                        <div class="stat-icon mb-3">
                            <i class="bi ${escapeHtml(iconClass)} text-${escapeHtml(colorClass)} display-5"></i>
                        </div>
                        <h2 class="display-3 fw-bold text-${escapeHtml(colorClass)} mb-2">
                            ${stat.counterTarget ? `<span class="counter" data-target="${stat.counterTarget}" data-prefix="${stat.counterPrefix || ''}" data-suffix="${stat.counterSuffix || ''}">${stat.counterPrefix || ''}0${stat.counterSuffix || ''}</span>` : escapeHtml(stat.value)}
                        </h2>
                        <p class="text-muted mb-0 fw-semibold">${escapeHtml(stat.label)}</p>
                    </div>
                </div>
            `;
        });

        container.innerHTML = html;

        // Re-initialize AOS for dynamic content
        if (typeof AOS !== 'undefined') {
            AOS.refresh();
        }
    }

    /**
     * Render fallback stats if API fails
     */
    function renderFallbackStats(container) {
        const fallbackStats = [
            { value: '10M+', label: 'Messages Delivered', icon: 'bi-envelope-paper', color: 'primary', target: 10000000, suffix: '+' },
            { value: '98%', label: 'Delivery Success Rate', icon: 'bi-check-circle', color: 'success', target: 98, suffix: '%' },
            { value: '5,000+', label: 'Happy Customers', icon: 'bi-people', color: 'info', target: 5000, suffix: '+' },
            { value: '24/7', label: 'Customer Support', icon: 'bi-headset', color: 'warning', target: 24, suffix: '/7' }
        ];

        let html = '';
        fallbackStats.forEach((stat, index) => {
            html += `
                <div class="col-md-3 col-6" data-aos="zoom-in" data-aos-delay="${100 + (index * 100)}">
                    <div class="stat-item-enhanced">
                        <div class="stat-icon mb-3">
                            <i class="bi ${stat.icon} text-${stat.color} display-5"></i>
                        </div>
                        <h2 class="display-3 fw-bold text-${stat.color} mb-2">
                            <span class="counter" data-target="${stat.target}" data-suffix="${stat.suffix}">0${stat.suffix}</span>
                        </h2>
                        <p class="text-muted mb-0 fw-semibold">${stat.label}</p>
                    </div>
                </div>
            `;
        });

        container.innerHTML = html;

        // Re-initialize AOS for dynamic content
        if (typeof AOS !== 'undefined') {
            AOS.refresh();
        }

        // Initialize counter animations
        setTimeout(() => initCounterAnimations(), 500);
    }

    /**
     * Initialize counter animations
     */
    function initCounterAnimations() {
        const counters = document.querySelectorAll('.counter');

        const observerOptions = {
            threshold: 0.5
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    animateCounter(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        counters.forEach(counter => {
            observer.observe(counter);
        });
    }

    /**
     * Animate a single counter
     */
    function animateCounter(counter) {
        const target = parseInt(counter.dataset.target) || 0;
        const prefix = counter.dataset.prefix || '';
        const suffix = counter.dataset.suffix || '';
        const duration = 2000;
        const startTime = performance.now();

        function updateCounter(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function for smooth animation
            const easeOutQuart = 1 - Math.pow(1 - progress, 4);
            const currentValue = Math.floor(easeOutQuart * target);

            // Format the number
            let displayValue;
            if (target >= 1000000) {
                displayValue = (currentValue / 1000000).toFixed(1) + 'M';
            } else if (target >= 1000) {
                displayValue = currentValue.toLocaleString();
            } else {
                displayValue = currentValue.toString();
            }

            counter.textContent = prefix + displayValue + suffix;

            if (progress < 1) {
                requestAnimationFrame(updateCounter);
            }
        }

        requestAnimationFrame(updateCounter);
    }

    /**
     * Escape HTML to prevent XSS
     */
    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

})();
