/**
 * Webhooks Index Page - DataTables Implementation
 * SERVER-SIDE API INTEGRATION - Calls Web controller endpoints
 */

// Global variables
let webhooksTable;

// Initialize on document ready
$(document).ready(function() {
    initWebhooksTable();
});

/**
 * Initialize DataTable for webhooks
 */
function initWebhooksTable() {
    webhooksTable = $('#webhooksTable').DataTable({
        serverSide: true,
        processing: true,

        ajax: {
            url: '/Webhooks/GetWebhooks', // SERVER-SIDE: Call Web controller, not API
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Send DataTables request as JSON
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search
                });
            },
            dataSrc: function(json) {
                // Web controller returns DataTables format already
                return json.data || [];
            }
        },
        
        columns: [
            {
                data: 'name',
                name: 'Name',
                title: 'Name',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `<strong>${escapeHtml(data)}</strong>`;
                    }
                    return data;
                }
            },
            {
                data: 'url',
                name: 'URL',
                title: 'URL',
                orderable: false,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `<small class="text-muted text-break">${escapeHtml(data)}</small>`;
                    }
                    return data;
                }
            },
            {
                data: 'events',
                name: 'Events',
                title: 'Events',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const eventCount = Array.isArray(data) ? data.length : 0;
                        return createBadge(`${eventCount} events`, 'secondary');
                    }
                    return data;
                }
            },
            {
                data: 'isActive',
                name: 'Status',
                title: 'Status',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return data ? createBadge('Active', 'success') : createBadge('Inactive', 'secondary');
                    }
                    return data;
                }
            },
            {
                data: 'deliveryCount',
                name: 'SuccessRate',
                title: 'Success Rate',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const successRate = calculatePercentage(row.deliveryCount - (row.failureCount || 0), row.deliveryCount, 1);
                        return `${successRate}<br/><small class="text-muted">${formatNumber(data)} sent</small>`;
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
                className: 'no-export text-end',
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `
                            <div class="btn-group btn-group-sm" role="group">
                                <button class="btn btn-outline-primary" onclick="viewLogs(${data})" title="View Logs">
                                    <i class="bi bi-journal-text"></i>
                                </button>
                                <button class="btn btn-outline-secondary" onclick="editWebhook(${data})" title="Edit">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="btn btn-outline-info" onclick="testWebhook(${data})" title="Test">
                                    <i class="bi bi-play"></i>
                                </button>
                                <button class="btn btn-outline-danger" onclick="deleteWebhook(${data})" title="Delete">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        `;
                    }
                    return data;
                }
            }
        ],
        
        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[0, 'asc']],
        
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
             '<"row"<"col-sm-12"Btr>>' +
             '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        
        buttons: [
            {
                extend: 'csv',
                className: 'btn btn-sm btn-outline-primary me-1',
                text: '<i class="bi bi-file-earmark-csv"></i> CSV',
                exportOptions: { columns: ':visible:not(.no-export)' }
            },
            {
                extend: 'excel',
                className: 'btn btn-sm btn-outline-success me-1',
                text: '<i class="bi bi-file-earmark-excel"></i> Excel',
                exportOptions: { columns: ':visible:not(.no-export)' }
            },
            {
                extend: 'pdf',
                className: 'btn btn-sm btn-outline-danger',
                text: '<i class="bi bi-file-earmark-pdf"></i> PDF',
                exportOptions: { columns: ':visible:not(.no-export)' },
                orientation: 'landscape'
            }
        ],
        
        language: {
            processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>',
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No webhooks found</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching webhooks found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ webhooks',
            infoEmpty: 'Showing 0 to 0 of 0 webhooks',
            infoFiltered: '(filtered from _MAX_ total webhooks)',
            lengthMenu: 'Show _MENU_ webhooks',
            search: 'Search:',
            paginate: {
                first: '<i class="bi bi-chevron-bar-left"></i>',
                previous: '<i class="bi bi-chevron-left"></i>',
                next: '<i class="bi bi-chevron-right"></i>',
                last: '<i class="bi bi-chevron-bar-right"></i>'
            }
        },
        
        stateSave: true,
        stateDuration: 60 * 60 * 24,
        searchDelay: 300
    });
}

// Action functions
function viewLogs(id) {
    window.location.href = AppUrls.webhooks?.logs ? AppUrls.webhooks.logs(id) : `/Webhooks/Logs/${id}`;
}

function editWebhook(id) {
    window.location.href = AppUrls.webhooks?.edit ? AppUrls.webhooks.edit(id) : `/Webhooks/Edit/${id}`;
}

function testWebhook(id) {
    confirmAction('Send a test webhook request?', function() {
        $.ajax({
            url: `/Webhooks/Test?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Test webhook sent successfully!', 'success');
                } else {
                    showNotification(response.message || 'Test failed', 'error');
                }
            },
            error: function() {
                showNotification('Failed to send test webhook', 'error');
            }
        });
    });
}

function deleteWebhook(id) {
    confirmAction('Are you sure you want to delete this webhook?', function() {
        $.ajax({
            url: `/Webhooks/Delete?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Webhook deleted successfully!', 'success');
                    webhooksTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete webhook', 'error');
                }
            },
            error: function() {
                showNotification('Failed to delete webhook', 'error');
            }
        });
    });
}
