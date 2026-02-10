/**
 * landing-companies.js - Load trusted companies dynamically
 */

document.addEventListener('DOMContentLoaded', function() {
    loadTrustedCompanies();
});

async function loadTrustedCompanies() {
    const container = document.getElementById('trusted-companies-container');
    if (!container) return;

    try {
        const apiUrl = window.AppUrls ?
            window.AppUrls.buildApiUrl('/api/trustedcompanies') :
            '/api/trustedcompanies';

        const response = await fetch(apiUrl);
        const result = await response.json();

        if (result.success && result.data && result.data.length > 0) {
            // Show all companies (limit to 12 for display)
            const companies = result.data.slice(0, 12);

            container.innerHTML = companies.map(company => {
                const websiteAttr = company.websiteUrl ? `data-website="${company.websiteUrl}"` : '';

                return `
                    <div class="col-6 col-md-2 text-center">
                        <div class="company-logo-wrapper" ${websiteAttr}>
                            <img src="${company.logoUrl}"
                                 alt="${escapeHtml(company.companyName)}"
                                 class="company-logo img-fluid grayscale-logo"
                                 onerror="this.style.display='none'; this.nextElementSibling.style.display='block';"
                                 title="${escapeHtml(company.companyName)}">
                            <div style="display:none; padding: 20px 10px; border: 1px dashed #ccc; border-radius: 8px; color: #999; font-size: 0.875rem;">
                                ${escapeHtml(company.companyName)}
                            </div>
                        </div>
                    </div>
                `;
            }).join('');
        } else {
            container.innerHTML = '<div class="col-12"><p class="text-center text-muted">No company logos available</p></div>';
        }
    } catch (error) {
        console.error('Error loading trusted companies:', error);
        container.innerHTML = '<div class="col-12"><p class="text-center text-muted">Failed to load companies</p></div>';
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
