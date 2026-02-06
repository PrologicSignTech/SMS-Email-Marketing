/**
 * Messages Index Page - DataTables Implementation
 * SERVER-SIDE: Calls Web controller endpoints instead of API directly
 */

// Global variables
let messagesTable;
let currentStatus = null;
let currentChannel = null;

// Message status enum
const MessageStatus = {
    All: null,
    Sent: 'Sent',
    Pending: 'Pending',
    Failed: 'Failed'
};

// Channel enum
const Channel = {
    SMS: 0,
    MMS: 1,
    Email: 2
};

// Initialize on document ready
$(document).ready(function() {
    initMessagesTable();
    setupTabHandlers();
    setupChannelFilter();
});

/**
 * Initialize DataTable for messages
 */
function initMessagesTable() {
    messagesTable = $('#messagesTable').DataTable({
        // Server-side processing
        serverSide: true,
        processing: true,

        // AJAX configuration
        ajax: {
            url: '/Messages/GetMessages', // SERVER-SIDE: Call Web controller
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Send DataTables request as JSON
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    status: currentStatus,
                    channel: currentChannel
                });
            },
            dataSrc: function(json) {
                // Web controller returns DataTables format already
                return json.data || [];
            },
            error: function(xhr, error, code) {
                console.error('DataTables error:', error, code);
                if (xhr.status === 401) {
                    showNotification('Session expired. Please log in again.', 'error');
                    setTimeout(() => {
                        window.location.href = AppUrls.auth.login;
                    }, 2000);
                }
            }
        },
        
        // Column definitions
        columns: [
            {
                data: 'subject',
                name: 'Message',
                title: 'Message',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const campaignName = row.campaignName || row.campaign?.name || '';
                        const subject = data || row.messageBody?.substring(0, 50) || 'No subject';
                        return `<strong>${escapeHtml(subject)}</strong><br/><small class="text-muted">${escapeHtml(campaignName)}</small>`;
                    }
                    return data;
                }
            },
            {
                data: 'channel',
                name: 'Channel',
                title: 'Channel',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const channelNames = ['SMS', 'MMS', 'Email'];
                        const channelColors = ['success', 'info', 'primary'];
                        const channelName = channelNames[data] || 'Unknown';
                        const channelColor = channelColors[data] || 'secondary';
                        return createBadge(channelName, channelColor);
                    }
                    return data;
                }
            },
            {
                data: 'recipients',
                name: 'Recipients',
                title: 'Recipients',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return formatNumber(data);
                    }
                    return data;
                }
            },
            {
                data: 'status',
                name: 'Status',
                title: 'Status',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const statusColors = { 
                            'Sent': 'success', 
                            'Pending': 'warning', 
                            'Failed': 'danger',
                            'Queued': 'info'
                        };
                        const statusColor = statusColors[data] || 'secondary';
                        return createBadge(data, statusColor);
                    }
                    return data;
                }
            },
            {
                data: 'delivered',
                name: 'Delivered',
                title: 'Delivered',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const deliveryRate = calculatePercentage(data, row.recipients, 1);
                        return `${formatNumber(data)}<br/><small class="text-muted">${deliveryRate}</small>`;
                    }
                    return data;
                }
            },
            {
                data: 'opened',
                name: 'Engagement',
                title: 'Engagement',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        // Email channels show open and click rates
                        if (row.channel === Channel.Email) {
                            const openRate = calculatePercentage(row.opened || 0, row.delivered, 1);
                            const clickRate = calculatePercentage(row.clicked || 0, row.opened || 1, 1);
                            return `
                                <small>Opened: ${formatNumber(row.opened || 0)} (${openRate})</small><br/>
                                <small>Clicked: ${formatNumber(row.clicked || 0)} (${clickRate})</small>
                            `;
                        } else {
                            // SMS/MMS show click count only
                            return `<small>Clicked: ${formatNumber(row.clicked || 0)}</small>`;
                        }
                    }
                    return data;
                }
            },
            {
                data: 'sentAt',
                name: 'SentAt',
                title: 'Sent Date',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return formatDate(data);
                    }
                    return data;
                }
            },
            {
                data: 'id',
                name: 'Actions',
                title: 'Actions',
                orderable: false,
                searchable: false,
                className: 'no-export',
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `
                            <div class="btn-group btn-group-sm" role="group">
                                <button class="btn btn-outline-primary" onclick="viewMessageDetails(${data})" title="View Details">
                                    <i class="bi bi-eye"></i>
                                </button>
                                <button class="btn btn-outline-secondary" onclick="duplicateMessage(${data})" title="Duplicate">
                                    <i class="bi bi-files"></i>
                                </button>
                            </div>
                        `;
                    }
                    return data;
                }
            }
        ],
        
        // Table configuration
        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[6, 'desc']], // Sort by sent date descending
        
        // DOM layout with export buttons
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
             '<"row"<"col-sm-12"Btr>>' +
             '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        
        // Export buttons
        buttons: [
            {
                extend: 'csv',
                className: 'btn btn-sm btn-outline-primary me-1',
                text: '<i class="bi bi-file-earmark-csv"></i> CSV',
                exportOptions: {
                    columns: ':visible:not(.no-export)'
                }
            },
            {
                extend: 'excel',
                className: 'btn btn-sm btn-outline-success me-1',
                text: '<i class="bi bi-file-earmark-excel"></i> Excel',
                exportOptions: {
                    columns: ':visible:not(.no-export)'
                }
            },
            {
                extend: 'pdf',
                className: 'btn btn-sm btn-outline-danger',
                text: '<i class="bi bi-file-earmark-pdf"></i> PDF',
                exportOptions: {
                    columns: ':visible:not(.no-export)'
                },
                orientation: 'landscape',
                pageSize: 'A4'
            }
        ],
        
        // Language customization
        language: {
            processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>',
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No messages found</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching messages found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ messages',
            infoEmpty: 'Showing 0 to 0 of 0 messages',
            infoFiltered: '(filtered from _MAX_ total messages)',
            lengthMenu: 'Show _MENU_ messages',
            search: 'Search:',
            paginate: {
                first: '<i class="bi bi-chevron-bar-left"></i>',
                previous: '<i class="bi bi-chevron-left"></i>',
                next: '<i class="bi bi-chevron-right"></i>',
                last: '<i class="bi bi-chevron-bar-right"></i>'
            }
        },
        
        // State saving
        stateSave: true,
        stateDuration: 60 * 60 * 24, // 24 hours
        
        // Search delay
        searchDelay: 300
    });
}

/**
 * Setup tab click handlers for status filtering
 */
function setupTabHandlers() {
    $('[data-bs-toggle="tab"]').on('shown.bs.tab', function(e) {
        const target = e.target.getAttribute('href').substring(1);
        const statusMap = {
            'all': null,
            'sent': MessageStatus.Sent,
            'pending': MessageStatus.Pending,
            'failed': MessageStatus.Failed
        };
        currentStatus = statusMap[target];
        if (messagesTable) {
            messagesTable.ajax.reload();
        }
    });
}

/**
 * Setup channel filter dropdown
 */
function setupChannelFilter() {
    $('#channelFilter').on('change', function() {
        const value = $(this).val();
        currentChannel = value === '' ? null : parseInt(value);
        if (messagesTable) {
            messagesTable.ajax.reload();
        }
    });
}

// ============================================================================
// MESSAGE ACTION FUNCTIONS
// ============================================================================

/**
 * View message details
 */
function viewMessageDetails(id) {
    window.location.href = AppUrls.messages?.details ? AppUrls.messages.details(id) : `/Messages/Details/${id}`;
}

/**
 * Duplicate a message
 */
function duplicateMessage(id) {
    confirmAction('Are you sure you want to duplicate this message?', function() {
        $.ajax({
            url: `/Messages/Duplicate?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success || response.isSuccess) {
                    showNotification(response.message || 'Message duplicated successfully!', 'success');
                    messagesTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to duplicate message', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to duplicate message');
            }
        });
    });
}

/**
 * Handle AJAX errors
 */
function handleAjaxError(xhr, defaultMessage) {
    if (xhr.status === 401) {
        showNotification('Session expired. Please log in again.', 'error');
        setTimeout(() => {
            window.location.href = AppUrls.auth.login;
        }, 2000);
    } else if (xhr.status === 403) {
        showNotification('You do not have permission to perform this action', 'error');
    } else if (xhr.status === 404) {
        showNotification('Message not found', 'error');
    } else {
        const errorMessage = xhr.responseJSON?.message || defaultMessage;
        showNotification(errorMessage, 'error');
    }
}
