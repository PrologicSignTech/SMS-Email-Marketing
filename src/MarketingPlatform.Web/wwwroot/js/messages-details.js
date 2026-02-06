/**
 * Messages Details Page JavaScript
 * Handles message details loading, rendering delivery table, and performance metrics
 */

let chart = null;

document.addEventListener('DOMContentLoaded', function() {
    loadMessageDetails();
    setupEventListeners();
});

/**
 * Setup event listeners for action buttons
 */
function setupEventListeners() {
    const resendBtn = document.querySelector('button[data-action="resend-message"]');
    if (resendBtn) {
        resendBtn.addEventListener('click', resendMessage);
    }

    const duplicateBtn = document.querySelector('button[data-action="duplicate-message"]');
    if (duplicateBtn) {
        duplicateBtn.addEventListener('click', duplicateMessage);
    }

    const exportBtn = document.querySelector('button[data-action="export-report"]');
    if (exportBtn) {
        exportBtn.addEventListener('click', exportReport);
    }

    // Setup event delegation for recipient view buttons
    document.addEventListener('click', function(e) {
        if (e.target.closest('button[data-action="view-recipient"]')) {
            const recipientPhone = e.target.closest('button[data-action="view-recipient"]').dataset.recipient;
            viewRecipient(recipientPhone);
        }
    });
}

/**
 * Load message details from API
 */
async function loadMessageDetails() {
    try {
        const messageId = window.location.pathname.split('/').pop();
        
        const mockMessage = {
            id: messageId,
            subject: 'Flash Sale Alert',
            channel: 'SMS',
            status: 'Sent',
            campaignName: 'Flash Deals',
            sentAt: '2024-03-20 09:15:00',
            content: 'FLASH SALE! Get 50% off today only. Shop now: https://example.com/sale',
            stats: {
                recipients: 5230,
                delivered: 5227,
                opened: 4156,
                clicked: 1456,
                bounced: 3,
                failed: 0
            },
            deliveryDetails: [
                { recipient: '+1234567890', status: 'Delivered', deliveredAt: '2024-03-20 09:15:23', openedAt: '2024-03-20 09:16:45' },
                { recipient: '+1234567891', status: 'Delivered', deliveredAt: '2024-03-20 09:15:24', openedAt: '2024-03-20 09:18:12' },
                { recipient: '+1234567892', status: 'Delivered', deliveredAt: '2024-03-20 09:15:25', openedAt: null },
                { recipient: '+1234567893', status: 'Failed', deliveredAt: null, openedAt: null },
                { recipient: '+1234567894', status: 'Delivered', deliveredAt: '2024-03-20 09:15:27', openedAt: '2024-03-20 09:20:34' }
            ]
        };
        
        populateDetails(mockMessage);
        renderDeliveryTable(mockMessage.deliveryDetails);
        createPerformanceChart(mockMessage.stats);
        
        document.getElementById('loading').style.display = 'none';
        document.getElementById('messageDetails').style.display = 'block';
    } catch (error) {
        document.getElementById('loading').innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
    }
}

/**
 * Populate message details into the UI
 */
function populateDetails(message) {
    document.getElementById('statRecipients').textContent = message.stats.recipients.toLocaleString();
    document.getElementById('statDelivered').textContent = message.stats.delivered.toLocaleString();
    document.getElementById('statOpened').textContent = message.stats.opened ? message.stats.opened.toLocaleString() : 'N/A';
    document.getElementById('statClicked').textContent = message.stats.clicked.toLocaleString();
    
    document.getElementById('messageSubject').textContent = message.subject;
    
    const channelColors = { SMS: 'success', MMS: 'info', Email: 'primary' };
    document.getElementById('messageChannel').innerHTML = 
        `<span class="badge bg-${channelColors[message.channel]}">${message.channel}</span>`;
    
    const statusColors = { Sent: 'success', Pending: 'warning', Failed: 'danger' };
    document.getElementById('messageStatus').innerHTML = 
        `<span class="badge bg-${statusColors[message.status]}">${message.status}</span>`;
    
    document.getElementById('messageCampaign').textContent = message.campaignName;
    document.getElementById('messageSentAt').textContent = message.sentAt;
    document.getElementById('messageContent').textContent = message.content;
}

/**
 * Render delivery details table
 */
function renderDeliveryTable(details) {
    const tbody = document.querySelector('#deliveryTable tbody');
    let html = '';
    
    details.forEach(detail => {
        const statusColors = { Delivered: 'success', Failed: 'danger', Pending: 'warning' };
        html += `
            <tr>
                <td>${detail.recipient}</td>
                <td><span class="badge bg-${statusColors[detail.status]}">${detail.status}</span></td>
                <td><small>${detail.deliveredAt || 'N/A'}</small></td>
                <td><small>${detail.openedAt || 'N/A'}</small></td>
                <td>
                    <button class="btn btn-sm btn-outline-primary" data-action="view-recipient" data-recipient="${detail.recipient}">
                        <i class="bi bi-eye"></i>
                    </button>
                </td>
            </tr>
        `;
    });
    
    tbody.innerHTML = html;
}

/**
 * Create performance metrics chart
 */
function createPerformanceChart(stats) {
    const ctx = document.getElementById('performanceChart').getContext('2d');
    
    // Destroy existing chart if it exists
    if (chart) {
        chart.destroy();
    }
    
    chart = new Chart(ctx, {
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
            plugins: {
                legend: { position: 'bottom' }
            }
        }
    });
}

/**
 * View recipient details
 */
function viewRecipient(phone) {
    showNotification(`View details for ${phone} (Demo)`, 'info');
}

/**
 * Resend message to failed recipients
 */
function resendMessage() {
    if (confirm('Resend this message to failed recipients?')) {
        showNotification('Message resent! (Demo)', 'success');
    }
}

/**
 * Duplicate current message
 */
function duplicateMessage() {
    if (confirm('Create a copy of this message?')) {
        window.location.href = '/Messages/Compose';
    }
}

/**
 * Export message report
 */
function exportReport() {
    showNotification('Exporting report... (Demo)', 'info');
}
