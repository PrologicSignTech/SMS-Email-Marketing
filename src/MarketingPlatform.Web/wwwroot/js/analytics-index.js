/**
 * Analytics Index Page - Dashboard with Charts
 * Handles analytics dashboard with Chart.js visualizations
 */

let performanceChart;
let channelChart;

// Initialize on document ready
$(document).ready(function() {
    loadAnalyticsData();
});

/**
 * Load analytics data from API (SERVER-SIDE)
 */
function loadAnalyticsData() {
    $.ajax({
        url: '/Analytics/GetDashboard',
        method: 'GET',
        success: function(response) {
            const data = response.data || response;
            updateDashboardStats(data);
            initCharts(data);
        },
        error: function(xhr) {
            console.error('Failed to load analytics data:', xhr);
            // Initialize with default/empty data
            initCharts(null);
        }
    });
}

/**
 * Update dashboard statistics cards
 */
function updateDashboardStats(data) {
    if (!data) return;
    
    // Update stat cards if data is available
    const totalMessagesEl = document.getElementById('totalMessages');
    if (totalMessagesEl && data.totalMessages !== undefined) {
        totalMessagesEl.textContent = formatNumber(data.totalMessages);
    }
    const deliveryRateEl = document.getElementById('deliveryRate');
    if (deliveryRateEl && data.deliveryRate !== undefined) {
        deliveryRateEl.textContent = data.deliveryRate + '%';
    }
    const openRateEl = document.getElementById('openRate');
    if (openRateEl && data.openRate !== undefined) {
        openRateEl.textContent = data.openRate + '%';
    }
    const clickRateEl = document.getElementById('clickRate');
    if (clickRateEl && data.clickRate !== undefined) {
        clickRateEl.textContent = data.clickRate + '%';
    }
}

/**
 * Initialize charts
 */
function initCharts(data) {
    initPerformanceChart(data);
    initChannelChart(data);
}

/**
 * Initialize Performance Chart (Line Chart)
 */
function initPerformanceChart(data) {
    const ctx = document.getElementById('performanceChart');
    if (!ctx) return;
    
    const chartData = data?.performanceData || getDefaultPerformanceData();
    
    performanceChart = new Chart(ctx.getContext('2d'), {
        type: 'line',
        data: {
            labels: chartData.labels || ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
            datasets: [
                {
                    label: 'Sent',
                    data: chartData.sent || [1200, 1900, 1500, 2100],
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13, 110, 253, 0.1)',
                    tension: 0.4,
                    fill: true
                },
                {
                    label: 'Delivered',
                    data: chartData.delivered || [1180, 1850, 1480, 2050],
                    borderColor: '#198754',
                    backgroundColor: 'rgba(25, 135, 84, 0.1)',
                    tension: 0.4,
                    fill: true
                },
                {
                    label: 'Opened',
                    data: chartData.opened || [620, 890, 750, 1100],
                    borderColor: '#ffc107',
                    backgroundColor: 'rgba(255, 193, 7, 0.1)',
                    tension: 0.4,
                    fill: true
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false
            },
            plugins: {
                title: {
                    display: true,
                    text: 'Campaign Performance Over Time'
                },
                legend: {
                    position: 'bottom'
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return formatNumber(value);
                        }
                    }
                }
            }
        }
    });
}

/**
 * Initialize Channel Distribution Chart (Doughnut Chart)
 */
function initChannelChart(data) {
    const ctx = document.getElementById('channelChart');
    if (!ctx) return;
    
    const chartData = data?.channelData || getDefaultChannelData();
    
    channelChart = new Chart(ctx.getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: ['SMS', 'Email', 'MMS'],
            datasets: [{
                data: chartData || [45, 40, 15],
                backgroundColor: [
                    '#198754',  // Green for SMS
                    '#0d6efd',  // Blue for Email
                    '#0dcaf0'   // Cyan for MMS
                ],
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: 'Channel Distribution'
                },
                legend: {
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((value / total) * 100).toFixed(1);
                            return `${label}: ${percentage}%`;
                        }
                    }
                }
            }
        }
    });
}

/**
 * Get default performance data
 */
function getDefaultPerformanceData() {
    return {
        labels: ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
        sent: [1200, 1900, 1500, 2100],
        delivered: [1180, 1850, 1480, 2050],
        opened: [620, 890, 750, 1100]
    };
}

/**
 * Get default channel data
 */
function getDefaultChannelData() {
    return [45, 40, 15]; // SMS, Email, MMS percentages
}

/**
 * Refresh analytics data
 */
function refreshAnalytics() {
    showNotification('Refreshing analytics data...', 'info', 2000);
    
    // Destroy existing charts
    if (performanceChart) {
        performanceChart.destroy();
    }
    if (channelChart) {
        channelChart.destroy();
    }
    
    // Reload data
    loadAnalyticsData();
}

/**
 * Export analytics data
 */
function exportAnalytics(format) {
    // Select the appropriate export endpoint based on format
    let exportEndpoint;
    if (format === 'csv') {
        exportEndpoint = window.AppUrls.api.analytics.export.campaignPerformanceCsv;
    } else if (format === 'excel') {
        exportEndpoint = window.AppUrls.api.analytics.export.campaignPerformanceExcel;
    } else {
        exportEndpoint = window.AppUrls.api.analytics.export.campaignPerformanceCsv; // default
    }
    
    $.ajax({
        url: window.AppUrls.buildApiUrl(exportEndpoint),
        method: 'GET',
        headers: getAjaxHeaders(),
        success: function(response) {
            if (response.success || response.isSuccess) {
                const downloadUrl = response.data?.url || response.url;
                if (downloadUrl) {
                    window.location.href = downloadUrl;
                    showNotification('Export started! Your download will begin shortly.', 'success');
                } else {
                    showNotification('Export completed!', 'success');
                }
            } else {
                showNotification(response.message || 'Failed to export analytics', 'error');
            }
        },
        error: function(xhr) {
            const errorMessage = xhr.responseJSON?.message || 'Failed to export analytics';
            showNotification(errorMessage, 'error');
        }
    });
}

/**
 * Handle AJAX errors
 */
function handleAjaxError(xhr, defaultMessage) {
    if (xhr.status === 401) {
        showNotification('Session expired. Please log in again.', 'error');
        setTimeout(() => {
            window.location.href = window.AppUrls.auth.login;
        }, 2000);
    } else if (xhr.status === 403) {
        showNotification('You do not have permission to view analytics', 'error');
    } else {
        const errorMessage = xhr.responseJSON?.message || defaultMessage;
        showNotification(errorMessage, 'error');
    }
}
