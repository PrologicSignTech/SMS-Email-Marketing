/**
 * Analytics Conversions - SERVER-SIDE API INTEGRATION
 * Conversion tracking for campaigns
 */

$(document).ready(function () {
    loadConversions();
});

/**
 * Load conversions data
 */
function loadConversions() {
    var tbody = document.getElementById('conversionsTableBody');
    tbody.innerHTML = '<tr><td colspan="7" class="text-center py-4"><div class="spinner-border spinner-border-sm"></div> Loading conversion data...</td></tr>';

    var startDate = document.getElementById('dateFrom').value || '';
    var endDate = document.getElementById('dateTo').value || '';

    var url = '/Analytics/GetConversions?';
    if (startDate) url += 'startDate=' + startDate + '&';
    if (endDate) url += 'endDate=' + endDate + '&';

    $.ajax({
        url: url.replace(/[&?]$/, ''),
        method: 'GET',
        success: function (response) {
            if (response.success && response.data) {
                var data = Array.isArray(response.data) ? response.data : [response.data];
                renderConversionsTable(data);
                updateSummaryCards(data);
            } else {
                tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted py-4"><i class="bi bi-inbox fs-1"></i><br>No conversion data available. Run campaigns with tracking enabled to see conversions.</td></tr>';
                updateSummaryCards([]);
            }
        },
        error: function () {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-danger py-4"><i class="bi bi-exclamation-triangle"></i> Failed to load conversion data</td></tr>';
            updateSummaryCards([]);
        }
    });
}

/**
 * Render conversions table
 */
function renderConversionsTable(data) {
    var tbody = document.getElementById('conversionsTableBody');

    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted py-4"><i class="bi bi-inbox fs-1"></i><br>No conversion data available</td></tr>';
        return;
    }

    var html = '';
    data.forEach(function (c) {
        var campaign = c.campaignName || c.CampaignName || 'Campaign #' + (c.campaignId || c.CampaignId || '-');
        var sent = c.totalSent || c.TotalSent || c.sent || 0;
        var delivered = c.totalDelivered || c.TotalDelivered || c.delivered || 0;
        var clicks = c.totalClicks || c.TotalClicks || c.clicks || 0;
        var conversions = c.totalConversions || c.TotalConversions || c.conversions || 0;
        var revenue = c.totalRevenue || c.TotalRevenue || c.revenue || 0;
        var convRate = sent > 0 ? ((conversions / sent) * 100).toFixed(1) : '0.0';

        var rateColor = parseFloat(convRate) >= 5 ? 'text-success' :
                       parseFloat(convRate) >= 2 ? 'text-warning' : 'text-danger';

        html += '<tr>';
        html += '<td><strong>' + escapeHtml(campaign) + '</strong></td>';
        html += '<td>' + formatNumber(sent) + '</td>';
        html += '<td>' + formatNumber(delivered) + '</td>';
        html += '<td>' + formatNumber(clicks) + '</td>';
        html += '<td><span class="badge bg-primary">' + formatNumber(conversions) + '</span></td>';
        html += '<td><span class="' + rateColor + ' fw-bold">' + convRate + '%</span></td>';
        html += '<td>$' + (typeof revenue === 'number' ? revenue.toFixed(2) : '0.00') + '</td>';
        html += '</tr>';
    });

    tbody.innerHTML = html;
}

/**
 * Update summary cards
 */
function updateSummaryCards(data) {
    var totalConv = 0, totalRev = 0, totalSent = 0;

    data.forEach(function (c) {
        totalConv += (c.totalConversions || c.TotalConversions || c.conversions || 0);
        totalRev += (c.totalRevenue || c.TotalRevenue || c.revenue || 0);
        totalSent += (c.totalSent || c.TotalSent || c.sent || 0);
    });

    var avgRate = totalSent > 0 ? ((totalConv / totalSent) * 100).toFixed(1) : '0.0';

    document.getElementById('totalConversions').textContent = formatNumber(totalConv);
    document.getElementById('avgConversionRate').textContent = avgRate + '%';
    document.getElementById('totalRevenue').textContent = '$' + totalRev.toFixed(2);
    document.getElementById('campaignsTracked').textContent = data.length;
}

/**
 * Export conversions
 */
function exportConversions(format) {
    var startDate = document.getElementById('dateFrom').value || '';
    var endDate = document.getElementById('dateTo').value || '';

    var url = format === 'csv'
        ? '/Analytics/ExportConversionsCsv?'
        : '/Analytics/ExportConversionsExcel?';

    if (startDate) url += 'startDate=' + startDate + '&';
    if (endDate) url += 'endDate=' + endDate + '&';

    window.location.href = url.replace(/[&?]$/, '');
}
