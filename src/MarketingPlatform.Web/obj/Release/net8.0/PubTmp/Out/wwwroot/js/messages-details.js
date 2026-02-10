/**
 * Messages Details Page - Server-Side API Integration
 * Calls Web controller endpoints, NOT API directly
 */

var chart = null;

document.addEventListener('DOMContentLoaded', function() {
    loadMessageDetails();
    setupEventListeners();
});

function setupEventListeners() {
    var resendBtn = document.querySelector('button[data-action="resend-message"]');
    if (resendBtn) resendBtn.addEventListener('click', resendMessage);

    var duplicateBtn = document.querySelector('button[data-action="duplicate-message"]');
    if (duplicateBtn) duplicateBtn.addEventListener('click', duplicateMessage);

    var exportBtn = document.querySelector('button[data-action="export-report"]');
    if (exportBtn) exportBtn.addEventListener('click', exportReport);
}

function loadMessageDetails() {
    var messageId = window.location.pathname.split('/').pop();

    $.get('/Messages/GetMessage?id=' + messageId, function(response) {
        if (response.success && response.data) {
            var msg = response.data;
            populateDetails(msg);
            renderDeliveryTable(msg.deliveryDetails || msg.recipients || []);

            var stats = {
                recipients: msg.recipientCount || msg.totalRecipients || 0,
                delivered: msg.deliveredCount || 0,
                opened: msg.openedCount || 0,
                clicked: msg.clickedCount || 0,
                failed: msg.failedCount || 0
            };
            createPerformanceChart(stats);

            document.getElementById('loading').style.display = 'none';
            document.getElementById('messageDetails').style.display = 'block';
        } else {
            document.getElementById('loading').innerHTML = '<div class="alert alert-danger">Message not found</div>';
        }
    }).fail(function() {
        document.getElementById('loading').innerHTML = '<div class="alert alert-danger">Failed to load message details</div>';
    });
}

function populateDetails(message) {
    var statRecipients = document.getElementById('statRecipients');
    if (statRecipients) statRecipients.textContent = (message.recipientCount || message.totalRecipients || 0).toLocaleString();

    var statDelivered = document.getElementById('statDelivered');
    if (statDelivered) statDelivered.textContent = (message.deliveredCount || 0).toLocaleString();

    var statOpened = document.getElementById('statOpened');
    if (statOpened) statOpened.textContent = message.openedCount !== undefined ? message.openedCount.toLocaleString() : 'N/A';

    var statClicked = document.getElementById('statClicked');
    if (statClicked) statClicked.textContent = (message.clickedCount || 0).toLocaleString();

    var subjectEl = document.getElementById('messageSubject');
    if (subjectEl) subjectEl.textContent = message.subject || message.name || 'No Subject';

    var channelNames = { 0: 'SMS', 1: 'MMS', 2: 'Email' };
    var channelColors = { 'SMS': 'success', 'MMS': 'info', 'Email': 'primary' };
    var channelName = message.channelName || channelNames[message.channel] || 'SMS';
    var channelEl = document.getElementById('messageChannel');
    if (channelEl) channelEl.innerHTML = '<span class="badge bg-' + (channelColors[channelName] || 'secondary') + '">' + channelName + '</span>';

    var statusColors = { 'Sent': 'success', 'Delivered': 'success', 'Pending': 'warning', 'Failed': 'danger', 'Scheduled': 'info', 'Draft': 'secondary' };
    var statusName = message.statusName || message.status || 'Unknown';
    var statusEl = document.getElementById('messageStatus');
    if (statusEl) statusEl.innerHTML = '<span class="badge bg-' + (statusColors[statusName] || 'secondary') + '">' + statusName + '</span>';

    var campaignEl = document.getElementById('messageCampaign');
    if (campaignEl) campaignEl.textContent = message.campaignName || 'N/A';

    var sentAtEl = document.getElementById('messageSentAt');
    if (sentAtEl) sentAtEl.textContent = message.sentAt ? new Date(message.sentAt).toLocaleString() : (message.createdAt ? new Date(message.createdAt).toLocaleString() : 'N/A');

    var contentEl = document.getElementById('messageContent');
    if (contentEl) contentEl.textContent = message.content || message.body || message.messageContent || 'No content';
}

function renderDeliveryTable(details) {
    var tbody = document.querySelector('#deliveryTable tbody');
    if (!tbody || !details || details.length === 0) {
        if (tbody) tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No delivery details available</td></tr>';
        return;
    }

    var html = '';
    details.forEach(function(detail) {
        var statusColors = { 'Delivered': 'success', 'Failed': 'danger', 'Pending': 'warning', 'Sent': 'info' };
        var status = detail.status || detail.statusName || 'Unknown';
        html += '<tr>' +
            '<td>' + escapeHtml(detail.recipient || detail.phoneNumber || detail.email || '') + '</td>' +
            '<td><span class="badge bg-' + (statusColors[status] || 'secondary') + '">' + escapeHtml(status) + '</span></td>' +
            '<td><small>' + (detail.deliveredAt ? new Date(detail.deliveredAt).toLocaleString() : 'N/A') + '</small></td>' +
            '<td><small>' + (detail.openedAt ? new Date(detail.openedAt).toLocaleString() : 'N/A') + '</small></td>' +
            '<td></td>' +
            '</tr>';
    });

    tbody.innerHTML = html;
}

function createPerformanceChart(stats) {
    var ctx = document.getElementById('performanceChart');
    if (!ctx) return;

    if (chart) chart.destroy();

    chart = new Chart(ctx.getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: ['Delivered', 'Opened', 'Clicked', 'Failed'],
            datasets: [{
                data: [stats.delivered, stats.opened || 0, stats.clicked, stats.failed],
                backgroundColor: ['#198754', '#0dcaf0', '#ffc107', '#dc3545']
            }]
        },
        options: {
            responsive: true,
            plugins: { legend: { position: 'bottom' } }
        }
    });
}

function resendMessage() {
    var messageId = window.location.pathname.split('/').pop();
    if (confirm('Resend this message to failed recipients?')) {
        showNotification('Resend feature not yet implemented for this message', 'info');
    }
}

function duplicateMessage() {
    var messageId = window.location.pathname.split('/').pop();
    if (confirm('Create a copy of this message?')) {
        $.post('/Messages/Duplicate?id=' + messageId, function(response) {
            if (response.success) {
                showNotification('Message duplicated!', 'success');
                window.location.href = '/Messages/Compose';
            } else {
                showNotification(response.message || 'Failed to duplicate', 'error');
            }
        });
    }
}

function exportReport() {
    showNotification('Export feature coming soon', 'info');
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
