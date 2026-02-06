/**
 * landing-usecases.js - Dynamic landing page use cases
 * Loads use cases from database and displays them in tab-based interface
 */

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    loadUseCases();
});

/**
 * Load use cases from API
 */
async function loadUseCases() {
    const tabsContainer = document.getElementById('industryTabs');
    const contentContainer = document.getElementById('industryTabContent');
    
    if (!tabsContainer || !contentContainer) {
        console.error('Use cases containers not found');
        return;
    }

    try {
        // Build full API URL using AppUrls helper
        const apiUrl = window.AppUrls ?
            window.AppUrls.buildApiUrl(window.AppUrls.api.useCases.list) :
            '/api/usecases';

        console.log('Fetching use cases from:', apiUrl);

        const response = await fetch(apiUrl);
        console.log('Response status:', response.status);

        if (!response.ok) {
            const errorText = await response.text();
            console.error('API error response:', errorText);
            throw new Error('Failed to load use cases');
        }

        const result = await response.json();
        console.log('API result:', result);

        if (result.success && result.data && result.data.length > 0) {
            console.log('Rendering', result.data.length, 'use cases');
            renderUseCases(result.data);
        } else {
            console.warn('No use cases data:', result);
            tabsContainer.innerHTML = '<li class="nav-item"><span class="nav-link disabled">No use cases available</span></li>';
            contentContainer.innerHTML = `
                <div class="alert alert-info text-center">
                    <i class="bi bi-info-circle"></i> No use cases available at this time.
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading use cases:', error);
        tabsContainer.innerHTML = '<li class="nav-item"><span class="nav-link disabled">Error loading use cases</span></li>';
        contentContainer.innerHTML = `
            <div class="alert alert-warning text-center">
                <i class="bi bi-exclamation-triangle"></i> Unable to load use cases at this time.
                <br><small class="text-muted">Error: ${error.message}</small>
            </div>
        `;
    }
}

/**
 * Render use cases in tabs
 * @param {Array} useCases - Array of use case objects
 */
function renderUseCases(useCases) {
    const tabsContainer = document.getElementById('industryTabs');
    const contentContainer = document.getElementById('industryTabContent');
    
    console.log('renderUseCases called with:', useCases);

    // Group use cases by industry
    const industries = {};
    useCases.forEach(useCase => {
        const industry = useCase.industry || 'Other';
        if (!industries[industry]) {
            industries[industry] = [];
        }
        industries[industry].push(useCase);
    });

    // Generate tabs
    let tabsHtml = '';
    let isFirst = true;
    Object.keys(industries).forEach(industry => {
        const industryId = industry.toLowerCase().replace(/\s+/g, '-');
        const icon = getIndustryIcon(industry);
        
        tabsHtml += `
            <li class="nav-item" role="presentation">
                <button class="nav-link ${isFirst ? 'active' : ''}" 
                        id="${industryId}-tab" 
                        data-bs-toggle="pill" 
                        data-bs-target="#${industryId}" 
                        type="button" 
                        role="tab">
                    <i class="bi ${icon}"></i> ${escapeHtml(industry)}
                </button>
            </li>
        `;
        isFirst = false;
    });

    // Generate tab content
    let contentHtml = '';
    isFirst = true;
    Object.keys(industries).forEach(industry => {
        const industryId = industry.toLowerCase().replace(/\s+/g, '-');
        const useCases = industries[industry];
        
        contentHtml += `
            <div class="tab-pane fade ${isFirst ? 'show active' : ''}" id="${industryId}" role="tabpanel">
        `;
        
        // Render each use case in this industry
        useCases.forEach((useCase, index) => {
            const imageUrl = useCase.imageUrl || `/images/use-cases/${industryId}.svg`;
            const resultsText = useCase.resultsText || '';
            
            // Parse description as list items (assuming newline-separated benefits)
            const benefits = useCase.description.split('\n').filter(b => b.trim());
            
            contentHtml += `
                <div class="row align-items-center g-4 ${index > 0 ? 'mt-5' : ''}">
                    <div class="col-lg-6" data-aos="fade-right">
                        <div class="use-case-image-wrapper">
                            <img src="${escapeHtml(imageUrl)}" 
                                 alt="${escapeHtml(useCase.title)}" 
                                 class="img-fluid rounded-4 shadow-lg"
                                 onerror="this.src='/images/use-cases/default.svg';">
                        </div>
                    </div>
                    <div class="col-lg-6" data-aos="fade-left">
                        <h3 class="fw-bold mb-4">${escapeHtml(useCase.title)}</h3>
                        <ul class="list-unstyled use-case-list">
            `;
            
            benefits.forEach(benefit => {
                if (benefit.trim()) {
                    contentHtml += `
                        <li class="mb-3">
                            <i class="bi bi-check-circle-fill text-success me-2"></i>
                            ${escapeHtml(benefit.trim())}
                        </li>
                    `;
                }
            });
            
            contentHtml += `
                        </ul>
            `;
            
            if (resultsText) {
                contentHtml += `
                    <div class="alert alert-light border">
                        <strong class="text-primary">Case Study:</strong> ${escapeHtml(resultsText)}
                    </div>
                `;
            }
            
            contentHtml += `
                    </div>
                </div>
            `;
        });
        
        contentHtml += `</div>`;
        isFirst = false;
    });

    console.log('Setting tabs HTML, length:', tabsHtml.length);
    console.log('Setting content HTML, length:', contentHtml.length);
    
    tabsContainer.innerHTML = tabsHtml;
    contentContainer.innerHTML = contentHtml;
    
    console.log('Use cases rendered successfully!');

    // Re-initialize scroll animations
    reinitScrollAnimations();
}

/**
 * Get icon class for industry
 * @param {string} industry - Industry name
 * @returns {string} Bootstrap icon class
 */
function getIndustryIcon(industry) {
    const icons = {
        'E-commerce': 'bi-cart',
        'E-Commerce': 'bi-cart',
        'Healthcare': 'bi-hospital',
        'Real Estate': 'bi-house',
        'Retail': 'bi-shop',
        'Finance': 'bi-bank',
        'Education': 'bi-book',
        'Technology': 'bi-cpu',
        'Other': 'bi-briefcase'
    };
    return icons[industry] || 'bi-briefcase';
}

/**
 * Re-initialize scroll animations for dynamically loaded use cases
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

    // Observe use case elements with data-aos attribute
    const useCaseElements = document.querySelectorAll('#industryTabContent [data-aos]');
    useCaseElements.forEach(el => observer.observe(el));

    console.log('Reinitialized scroll animations for', useCaseElements.length, 'use case elements');

    // Force immediate visibility as fallback
    setTimeout(() => {
        useCaseElements.forEach(item => {
            if (!item.classList.contains('aos-animate')) {
                item.classList.add('aos-animate');
            }
        });
        console.log('Forced animation on', useCaseElements.length, 'use case elements');
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
