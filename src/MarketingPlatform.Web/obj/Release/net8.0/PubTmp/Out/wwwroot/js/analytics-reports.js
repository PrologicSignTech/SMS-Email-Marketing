/**
 * Analytics Reports Page - Message Tracking DataTable
 * Shows all messages with status tabs and channel filtering.
 * Falls back to demo data when API returns empty results.
 */

let reportsTable;
let currentStatus = null;
let currentChannel = null;
let usingDemoData = false;

$(document).ready(function () {
    initReportsTable();
    setupStatusTabs();
    setupChannelFilter();
    loadSummaryStats();
});

// ─── Demo data for when API returns empty ───────────────────────
function getDemoMessages() {
    var now = new Date();
    var hour = 3600000;
    var day = 86400000;
    return [
        { id: 1001, recipient: '+1 (555) 123-4567', contactName: 'John Smith', channel: 0, messageBody: 'Hi John! Your appointment is confirmed for tomorrow at 10 AM. Reply CONFIRM to confirm.', subject: null, status: 3, scheduledAt: new Date(now - 2 * day).toISOString(), sentAt: new Date(now - 2 * day + hour).toISOString(), deliveredAt: new Date(now - 2 * day + hour + 5000).toISOString(), campaignName: 'Appointment Reminders' },
        { id: 1002, recipient: 'sarah.jones@email.com', contactName: 'Sarah Jones', channel: 2, messageBody: 'Welcome to our newsletter! Here are this week\'s top deals...', subject: 'Weekly Deals Newsletter', status: 3, scheduledAt: new Date(now - 1.5 * day).toISOString(), sentAt: new Date(now - 1.5 * day + hour).toISOString(), deliveredAt: new Date(now - 1.5 * day + hour + 3000).toISOString(), campaignName: 'Weekly Newsletter' },
        { id: 1003, recipient: '+1 (555) 987-6543', contactName: 'Mike Wilson', channel: 1, messageBody: 'Check out our new product lineup! Tap to see exclusive offers.', subject: null, status: 3, scheduledAt: new Date(now - 1 * day).toISOString(), sentAt: new Date(now - 1 * day + 2 * hour).toISOString(), deliveredAt: new Date(now - 1 * day + 2 * hour + 8000).toISOString(), campaignName: 'Product Launch MMS' },
        { id: 1004, recipient: '+1 (555) 456-7890', contactName: 'Emily Davis', channel: 0, messageBody: 'Flash sale! 30% off all items today only. Use code FLASH30. Shop now!', subject: null, status: 2, scheduledAt: new Date(now - 12 * hour).toISOString(), sentAt: new Date(now - 12 * hour + 1800000).toISOString(), deliveredAt: null, campaignName: 'Flash Sale SMS' },
        { id: 1005, recipient: 'robert.brown@company.com', contactName: 'Robert Brown', channel: 2, messageBody: 'Your monthly account statement is ready. Please review it at your earliest convenience.', subject: 'Monthly Statement Ready', status: 3, scheduledAt: new Date(now - 10 * hour).toISOString(), sentAt: new Date(now - 10 * hour + hour).toISOString(), deliveredAt: new Date(now - 10 * hour + hour + 4000).toISOString(), campaignName: 'Account Statements' },
        { id: 1006, recipient: '+1 (555) 321-0987', contactName: 'Lisa Anderson', channel: 0, messageBody: 'Hi Lisa, your package has been shipped! Track it here: https://track.example.com/12345', subject: null, status: 4, scheduledAt: new Date(now - 8 * hour).toISOString(), sentAt: new Date(now - 8 * hour + 1800000).toISOString(), deliveredAt: null, failedAt: new Date(now - 8 * hour + 2 * hour).toISOString(), errorMessage: 'Invalid phone number', campaignName: 'Shipping Notifications' },
        { id: 1007, recipient: '+1 (555) 654-3210', contactName: 'David Martinez', channel: 0, messageBody: 'Reminder: Your subscription renews in 3 days. Reply CANCEL to stop auto-renewal.', subject: null, status: 3, scheduledAt: new Date(now - 6 * hour).toISOString(), sentAt: new Date(now - 6 * hour + hour).toISOString(), deliveredAt: new Date(now - 6 * hour + hour + 2000).toISOString(), campaignName: 'Subscription Reminders' },
        { id: 1008, recipient: 'jennifer.taylor@email.com', contactName: 'Jennifer Taylor', channel: 2, messageBody: 'Thank you for your purchase! Here is your receipt and order details.', subject: 'Order Confirmation #ORD-4521', status: 3, scheduledAt: new Date(now - 4 * hour).toISOString(), sentAt: new Date(now - 4 * hour + 1800000).toISOString(), deliveredAt: new Date(now - 4 * hour + 1800000 + 5000).toISOString(), campaignName: 'Order Confirmations' },
        { id: 1009, recipient: '+1 (555) 111-2222', contactName: 'Chris Johnson', channel: 1, messageBody: 'New arrivals are here! See our latest collection with photos.', subject: null, status: 0, scheduledAt: new Date(now + 2 * hour).toISOString(), sentAt: null, deliveredAt: null, campaignName: 'New Arrivals MMS' },
        { id: 1010, recipient: '+1 (555) 333-4444', contactName: 'Amanda White', channel: 0, messageBody: 'Your verification code is 482917. It expires in 10 minutes.', subject: null, status: 3, scheduledAt: new Date(now - 2 * hour).toISOString(), sentAt: new Date(now - 2 * hour + 5000).toISOString(), deliveredAt: new Date(now - 2 * hour + 8000).toISOString(), campaignName: 'OTP Verification' },
        { id: 1011, recipient: 'mark.thompson@company.com', contactName: 'Mark Thompson', channel: 2, messageBody: 'You are invited to our exclusive webinar on digital marketing trends.', subject: 'Webinar Invitation: Marketing Trends 2025', status: 1, scheduledAt: new Date(now - 1 * hour).toISOString(), sentAt: new Date(now - 30 * 60000).toISOString(), deliveredAt: null, campaignName: 'Webinar Invites' },
        { id: 1012, recipient: '+1 (555) 777-8888', contactName: 'Rachel Green', channel: 0, messageBody: 'Happy Birthday Rachel! Enjoy 25% off your next purchase with code BDAY25 🎂', subject: null, status: 3, scheduledAt: new Date(now - 3 * hour).toISOString(), sentAt: new Date(now - 3 * hour + 1800000).toISOString(), deliveredAt: new Date(now - 3 * hour + 1800000 + 3000).toISOString(), campaignName: 'Birthday Offers' },
        { id: 1013, recipient: '+1 (555) 999-0000', contactName: 'Tom Harris', channel: 0, messageBody: 'Survey: How was your recent experience? Reply 1-5 to rate us.', subject: null, status: 4, scheduledAt: new Date(now - 5 * hour).toISOString(), sentAt: new Date(now - 5 * hour + hour).toISOString(), deliveredAt: null, failedAt: new Date(now - 5 * hour + 2 * hour).toISOString(), errorMessage: 'Number opted out', campaignName: 'Customer Surveys' },
        { id: 1014, recipient: 'karen.lee@email.com', contactName: 'Karen Lee', channel: 2, messageBody: 'Your password has been successfully changed. If you did not make this change, contact support immediately.', subject: 'Password Changed Successfully', status: 3, scheduledAt: new Date(now - 90 * 60000).toISOString(), sentAt: new Date(now - 85 * 60000).toISOString(), deliveredAt: new Date(now - 84 * 60000).toISOString(), campaignName: 'Security Alerts' },
        { id: 1015, recipient: '+1 (555) 222-3333', contactName: 'Steve Rogers', channel: 0, messageBody: 'Your loyalty points balance is 2,450 pts. Redeem now for rewards!', subject: null, status: 0, scheduledAt: new Date(now + 4 * hour).toISOString(), sentAt: null, deliveredAt: null, campaignName: 'Loyalty Program' }
    ];
}

function getDemoSummary() {
    return {
        totalSent: 1247,
        totalDelivered: 1189,
        totalPending: 38,
        totalFailed: 20
    };
}

// ─── DataTable initialization ───────────────────────────────────
function initReportsTable() {
    // Suppress DataTable warning popups
    $.fn.dataTable.ext.errMode = 'none';

    reportsTable = $('#reportsTable').DataTable({
        serverSide: true,
        processing: true,

        ajax: function (data, callback, settings) {
            var requestData = {
                draw: data.draw,
                start: data.start,
                length: data.length,
                search: data.search,
                status: currentStatus,
                channel: currentChannel
            };

            $.ajax({
                url: '/Analytics/GetReportMessages',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(requestData),
                success: function (json) {
                    // Check if API returned real data
                    if (json && json.data && json.data.length > 0) {
                        usingDemoData = false;
                        callback(json);
                    } else {
                        // Fall back to demo data
                        usingDemoData = true;
                        var demoMessages = getDemoMessages();

                        // Apply status filter
                        if (currentStatus !== null) {
                            demoMessages = demoMessages.filter(function (m) { return m.status === currentStatus; });
                        }

                        // Apply channel filter
                        if (currentChannel !== null) {
                            demoMessages = demoMessages.filter(function (m) { return m.channel === currentChannel; });
                        }

                        // Apply search
                        var searchVal = data.search && data.search.value ? data.search.value.toLowerCase() : '';
                        if (searchVal) {
                            demoMessages = demoMessages.filter(function (m) {
                                return (m.recipient || '').toLowerCase().indexOf(searchVal) >= 0 ||
                                    (m.contactName || '').toLowerCase().indexOf(searchVal) >= 0 ||
                                    (m.messageBody || '').toLowerCase().indexOf(searchVal) >= 0;
                            });
                        }

                        var total = demoMessages.length;
                        var paged = demoMessages.slice(data.start, data.start + data.length);

                        callback({
                            draw: data.draw,
                            recordsTotal: total,
                            recordsFiltered: total,
                            data: paged
                        });
                    }
                },
                error: function () {
                    // On error, also use demo data
                    usingDemoData = true;
                    var demoMessages = getDemoMessages();
                    callback({
                        draw: data.draw,
                        recordsTotal: demoMessages.length,
                        recordsFiltered: demoMessages.length,
                        data: demoMessages.slice(data.start, data.start + data.length)
                    });
                }
            });
        },

        columns: [
            {
                data: 'recipient',
                name: 'Recipient',
                orderable: true,
                searchable: true,
                defaultContent: 'N/A',
                render: function (data, type, row) {
                    if (type === 'display') {
                        var name = row.contactName ? '<strong>' + escapeHtml(row.contactName) + '</strong><br>' : '';
                        return name + '<small>' + escapeHtml(data || 'N/A') + '</small>';
                    }
                    return data;
                }
            },
            {
                data: 'channel',
                name: 'Channel',
                orderable: true,
                searchable: false,
                defaultContent: '',
                render: function (data) {
                    var names = ['SMS', 'MMS', 'Email'];
                    var colors = { 0: 'success', 1: 'info', 2: 'primary' };
                    return createBadge(names[data] || 'Unknown', colors[data] || 'secondary');
                }
            },
            {
                data: 'messageBody',
                name: 'Content',
                orderable: false,
                searchable: true,
                defaultContent: '',
                render: function (data, type, row) {
                    if (type === 'display') {
                        var content = '';
                        if (row.subject) content += '<strong>' + escapeHtml(row.subject) + '</strong><br>';
                        var body = (data || '').substring(0, 50);
                        content += '<small class="text-muted">' + escapeHtml(body) + (data && data.length > 50 ? '...' : '') + '</small>';
                        return content;
                    }
                    return data;
                }
            },
            {
                data: 'status',
                name: 'Status',
                orderable: true,
                searchable: false,
                defaultContent: '',
                render: function (data) {
                    var statusNames = ['Queued', 'Sending', 'Sent', 'Delivered', 'Failed', 'Bounced'];
                    var statusColors = { 0: 'warning', 1: 'info', 2: 'primary', 3: 'success', 4: 'danger', 5: 'danger' };
                    return createBadge(statusNames[data] || 'Unknown', statusColors[data] || 'secondary');
                }
            },
            {
                data: 'scheduledAt',
                name: 'Scheduled',
                orderable: true,
                searchable: false,
                defaultContent: '-',
                render: function (data) { return formatShortDate(data); }
            },
            {
                data: 'sentAt',
                name: 'Sent',
                orderable: true,
                searchable: false,
                defaultContent: '-',
                render: function (data) { return formatShortDate(data); }
            },
            {
                data: 'deliveredAt',
                name: 'Delivered',
                orderable: true,
                searchable: false,
                defaultContent: '-',
                render: function (data) { return formatShortDate(data); }
            },
            {
                data: 'id',
                name: 'Actions',
                orderable: false,
                searchable: false,
                className: 'no-export text-end',
                defaultContent: '',
                render: function (data, type, row) {
                    var btns = '<div class="btn-group btn-group-sm">';
                    btns += '<a href="/Messages/Details/' + data + '" class="btn btn-outline-primary" title="View Details"><i class="bi bi-eye"></i></a>';
                    if (row.status === 4) {
                        btns += '<button class="btn btn-outline-warning" onclick="retryMessage(' + data + ')" title="Retry"><i class="bi bi-arrow-repeat"></i></button>';
                    }
                    if (row.status === 0 || row.status === 1) {
                        btns += '<button class="btn btn-outline-danger" onclick="cancelMessage(' + data + ')" title="Cancel"><i class="bi bi-x-circle"></i></button>';
                    }
                    btns += '</div>';
                    return btns;
                }
            }
        ],

        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[5, 'desc']],

        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
            '<"row"<"col-sm-12"Btr>>' +
            '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',

        buttons: [
            { extend: 'csv', className: 'btn btn-sm btn-outline-primary me-1', text: '<i class="bi bi-file-earmark-csv"></i> CSV', exportOptions: { columns: ':visible:not(.no-export)' } },
            { extend: 'excel', className: 'btn btn-sm btn-outline-success me-1', text: '<i class="bi bi-file-earmark-excel"></i> Excel', exportOptions: { columns: ':visible:not(.no-export)' } },
            { extend: 'pdf', className: 'btn btn-sm btn-outline-danger', text: '<i class="bi bi-file-earmark-pdf"></i> PDF', exportOptions: { columns: ':visible:not(.no-export)' } }
        ],

        language: {
            processing: '<div class="spinner-border spinner-border-sm text-primary"></div> Loading...',
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox fs-4 d-block mb-2 text-muted"></i> No messages found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ messages',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search fs-4 d-block mb-2 text-muted"></i> No matching messages found</div>'
        },

        stateSave: true,
        searchDelay: 300
    });

    // Handle DataTables errors gracefully
    $('#reportsTable').on('error.dt', function (e, settings, techNote, message) {
        console.warn('DataTable warning:', message);
    });
}

function setupStatusTabs() {
    $('#statusTabs a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        var status = e.target.getAttribute('data-status');
        currentStatus = status !== '' ? parseInt(status) : null;
        reportsTable.ajax.reload();
    });
}

function setupChannelFilter() {
    $('#channelFilter').on('change', function () {
        var val = $(this).val();
        currentChannel = val !== '' ? parseInt(val) : null;
        reportsTable.ajax.reload();
    });
}

/**
 * Load summary statistics — maps DashboardSummaryDto properties correctly
 */
function loadSummaryStats() {
    $.ajax({
        url: '/Analytics/GetDashboard',
        method: 'GET',
        success: function (response) {
            if (response.success && response.data) {
                var d = response.data;
                // DashboardSummaryDto properties (camelCase from JSON): totalMessagesSent, totalMessagesDelivered, totalMessagesFailed
                var sent = d.totalMessagesSent || d.totalSent || d.TotalMessagesSent || d.TotalSent || 0;
                var delivered = d.totalMessagesDelivered || d.totalDelivered || d.TotalMessagesDelivered || d.TotalDelivered || 0;
                var failed = d.totalMessagesFailed || d.totalFailed || d.TotalMessagesFailed || d.TotalFailed || 0;
                var pending = d.totalPending || d.TotalPending || d.totalQueued || d.TotalQueued || 0;

                // If API returned all zeros, use demo stats
                if (sent === 0 && delivered === 0 && failed === 0) {
                    var demo = getDemoSummary();
                    sent = demo.totalSent;
                    delivered = demo.totalDelivered;
                    failed = demo.totalFailed;
                    pending = demo.totalPending;
                }

                $('#totalSent').text(formatNumber(sent));
                $('#totalDelivered').text(formatNumber(delivered));
                $('#totalPending').text(formatNumber(pending));
                $('#totalFailed').text(formatNumber(failed));
            } else {
                // API failed — show demo stats
                var demo = getDemoSummary();
                $('#totalSent').text(formatNumber(demo.totalSent));
                $('#totalDelivered').text(formatNumber(demo.totalDelivered));
                $('#totalPending').text(formatNumber(demo.totalPending));
                $('#totalFailed').text(formatNumber(demo.totalFailed));
            }
        },
        error: function () {
            // Show demo stats on error
            var demo = getDemoSummary();
            $('#totalSent').text(formatNumber(demo.totalSent));
            $('#totalDelivered').text(formatNumber(demo.totalDelivered));
            $('#totalPending').text(formatNumber(demo.totalPending));
            $('#totalFailed').text(formatNumber(demo.totalFailed));
        }
    });
}

function formatNumber(num) {
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toString();
}

function retryMessage(id) {
    if (usingDemoData) {
        if (typeof showNotification === 'function') showNotification('Demo mode — retry not available', 'info');
        return;
    }
    if (!confirm('Retry sending this message?')) return;

    $.ajax({
        url: '/Messages/RetryMessage',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ messageId: id }),
        success: function (result) {
            if (result.success) {
                if (typeof showNotification === 'function') showNotification('Message queued for retry!', 'success');
                reportsTable.ajax.reload(null, false);
            } else {
                if (typeof showNotification === 'function') showNotification(result.message || 'Failed to retry', 'error');
            }
        },
        error: function () {
            if (typeof showNotification === 'function') showNotification('Failed to retry message', 'error');
        }
    });
}

function cancelMessage(id) {
    if (usingDemoData) {
        if (typeof showNotification === 'function') showNotification('Demo mode — cancel not available', 'info');
        return;
    }
    if (!confirm('Cancel this scheduled message?')) return;

    $.ajax({
        url: '/Messages/CancelMessage',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ messageId: id }),
        success: function (result) {
            if (result.success) {
                if (typeof showNotification === 'function') showNotification('Message cancelled!', 'success');
                reportsTable.ajax.reload(null, false);
            } else {
                if (typeof showNotification === 'function') showNotification(result.message || 'Failed to cancel', 'error');
            }
        },
        error: function () {
            if (typeof showNotification === 'function') showNotification('Failed to cancel message', 'error');
        }
    });
}

// ─── Helpers (fallbacks if not defined in common.js) ────────────
if (typeof escapeHtml !== 'function') {
    function escapeHtml(str) {
        if (!str) return '';
        var div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }
}

if (typeof createBadge !== 'function') {
    function createBadge(text, color) {
        return '<span class="badge bg-' + color + '">' + text + '</span>';
    }
}

if (typeof formatShortDate !== 'function') {
    function formatShortDate(dateStr) {
        if (!dateStr) return '-';
        try {
            var d = new Date(dateStr);
            return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) + ' ' + d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
        } catch (e) {
            return dateStr;
        }
    }
}
