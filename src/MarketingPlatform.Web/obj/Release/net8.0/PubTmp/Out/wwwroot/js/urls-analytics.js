/**
 * urls-analytics.js - URL analytics page functionality
 * Handles analytics data loading, display, and copy to clipboard
 */

// Configuration
let urlId = null;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Get URL ID from the path
    urlId = window.location.pathname.split('/').pop();
    
    // Load analytics data
    loadAnalytics();
});

/**
 * Load analytics data for the URL
 */
async function loadAnalytics() {
    try {
        // For now, use mock data
        // TODO: Replace with actual API call when available
        const mockData = {
            shortCode: 'abc123',
            shortUrl: window.location.origin + '/s/abc123',
            originalUrl: 'https://example.com/summer-sale-2024',
            totalClicks: 1245,
            uniqueClicks: 892,
            clickRate: 71.6,
            avgPerDay: 89,
            clicksOverTime: [
                { date: '2024-03-14', clicks: 85 },
                { date: '2024-03-15', clicks: 120 },
                { date: '2024-03-16', clicks: 145 },
                { date: '2024-03-17', clicks: 132 },
                { date: '2024-03-18', clicks: 156 },
                { date: '2024-03-19', clicks: 178 },
                { date: '2024-03-20', clicks: 192 }
            ],
            referrers: [
                { source: 'Email Campaign', clicks: 456 },
                { source: 'Facebook', clicks: 234 },
                { source: 'Twitter', clicks: 189 },
                { source: 'Direct', clicks: 156 }
            ],
            devices: { Desktop: 567, Mobile: 556, Tablet: 122 },
            locations: [
                { country: 'United States', clicks: 456 },
                { country: 'United Kingdom', clicks: 234 },
                { country: 'Canada', clicks: 189 },
                { country: 'Australia', clicks: 123 }
            ]
        };
        
        populateData(mockData);
        createCharts(mockData);
    } catch (error) {
        console.error('Error loading analytics:', error);
        if (typeof showNotification === 'function') {
            showNotification('Error loading analytics: ' + error.message, 'error');
        }
    }
}

/**
 * Populate analytics data in the page
 * @param {Object} data - Analytics data object
 */
function populateData(data) {
    document.getElementById('shortUrl').textContent = data.shortUrl;
    document.getElementById('originalUrl').textContent = data.originalUrl;
    document.getElementById('totalClicks').textContent = data.totalClicks.toLocaleString();
    document.getElementById('uniqueClicks').textContent = data.uniqueClicks.toLocaleString();
    document.getElementById('clickRate').textContent = data.clickRate.toFixed(1) + '%';
    document.getElementById('avgPerDay').textContent = data.avgPerDay;
    
    // Render referrers list
    let refHtml = '<div class="list-group list-group-flush">';
    data.referrers.forEach(ref => {
        refHtml += `<div class="list-group-item px-0 d-flex justify-content-between">
            <span>${ref.source}</span>
            <strong>${ref.clicks}</strong>
        </div>`;
    });
    refHtml += '</div>';
    document.getElementById('referrersList').innerHTML = refHtml;
    
    // Render locations list
    let locHtml = '<div class="list-group list-group-flush">';
    data.locations.forEach(loc => {
        locHtml += `<div class="list-group-item px-0 d-flex justify-content-between">
            <span>${loc.country}</span>
            <strong>${loc.clicks}</strong>
        </div>`;
    });
    locHtml += '</div>';
    document.getElementById('locationsList').innerHTML = locHtml;
}

/**
 * Create charts for analytics visualization
 * @param {Object} data - Analytics data object
 */
function createCharts(data) {
    // Clicks over time chart
    const clicksCtx = document.getElementById('clicksChart').getContext('2d');
    new Chart(clicksCtx, {
        type: 'line',
        data: {
            labels: data.clicksOverTime.map(d => d.date),
            datasets: [{
                label: 'Clicks',
                data: data.clicksOverTime.map(d => d.clicks),
                borderColor: 'rgb(13, 110, 253)',
                backgroundColor: 'rgba(13, 110, 253, 0.1)',
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } }
        }
    });

    // Devices chart
    const devicesCtx = document.getElementById('devicesChart').getContext('2d');
    new Chart(devicesCtx, {
        type: 'doughnut',
        data: {
            labels: Object.keys(data.devices),
            datasets: [{
                data: Object.values(data.devices),
                backgroundColor: ['#0d6efd', '#198754', '#ffc107']
            }]
        },
        options: {
            responsive: true,
            plugins: { legend: { position: 'bottom' } }
        }
    });
}

/**
 * Copy URL to clipboard
 */
function copyUrl() {
    const url = document.getElementById('shortUrl').textContent;
    
    navigator.clipboard.writeText(url).then(function() {
        if (typeof showNotification === 'function') {
            showNotification('URL copied to clipboard!', 'success');
        } else {
            alert('URL copied to clipboard!');
        }
    }).catch(function(error) {
        console.error('Failed to copy URL:', error);
        if (typeof showNotification === 'function') {
            showNotification('Failed to copy URL', 'error');
        } else {
            alert('Failed to copy URL');
        }
    });
}

// Make function available globally for event handlers
if (typeof window !== 'undefined') {
    window.copyUrl = copyUrl;
}
