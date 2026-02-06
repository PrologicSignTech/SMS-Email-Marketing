/**
 * Providers Index Page - DataTables Implementation
 * SERVER-SIDE API INTEGRATION - Calls Web controller endpoints
 */

let providersTable;
let currentType = null;

$(document).ready(function() {
    initProvidersTable();
    setupTabHandlers();
});

function initProvidersTable() {
    providersTable = $('#providersTable').DataTable({
        serverSide: true,
        processing: true,

        ajax: {
            url: '/Providers/GetProviders', // SERVER-SIDE: Call Web controller, not API
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Send DataTables request as JSON
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    type: currentType
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
                orderable: true,
                searchable: true,
                render: function(data) {
                    return `<strong>${escapeHtml(data)}</strong>`;
                }
            },
            {
                data: 'type',
                name: 'Type',
                orderable: true,
                searchable: false,
                render: function(data) {
                    const typeColors = { 0: 'success', 1: 'info', 2: 'primary' };
                    const typeNames = ['SMS', 'MMS', 'Email'];
                    return createBadge(typeNames[data] || 'Unknown', typeColors[data] || 'secondary');
                }
            },
            {
                data: 'isActive',
                name: 'Status',
                orderable: true,
                searchable: false,
                render: function(data) {
                    return data ? createBadge('Active', 'success') : createBadge('Inactive', 'secondary');
                }
            },
            {
                data: 'healthScore',
                name: 'Health',
                orderable: true,
                searchable: false,
                render: function(data) {
                    const color = data >= 99 ? 'success' : data >= 95 ? 'warning' : 'danger';
                    return createBadge(`${data}%`, color);
                }
            },
            {
                data: 'messagesCount',
                name: 'Messages',
                orderable: true,
                searchable: false,
                render: function(data) {
                    return formatNumber(data || 0);
                }
            },
            {
                data: 'id',
                name: 'Actions',
                orderable: false,
                searchable: false,
                className: 'no-export text-end',
                render: function(data) {
                    return `
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-primary" onclick="viewHealth(${data})"><i class="bi bi-activity"></i></button>
                            <button class="btn btn-outline-secondary" onclick="editProvider(${data})"><i class="bi bi-pencil"></i></button>
                            <button class="btn btn-outline-info" onclick="testProvider(${data})"><i class="bi bi-play"></i></button>
                            <button class="btn btn-outline-danger" onclick="deleteProvider(${data})"><i class="bi bi-trash"></i></button>
                        </div>
                    `;
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
            { extend: 'csv', className: 'btn btn-sm btn-outline-primary me-1', text: '<i class="bi bi-file-earmark-csv"></i> CSV', exportOptions: { columns: ':visible:not(.no-export)' } },
            { extend: 'excel', className: 'btn btn-sm btn-outline-success me-1', text: '<i class="bi bi-file-earmark-excel"></i> Excel', exportOptions: { columns: ':visible:not(.no-export)' } },
            { extend: 'pdf', className: 'btn btn-sm btn-outline-danger', text: '<i class="bi bi-file-earmark-pdf"></i> PDF', exportOptions: { columns: ':visible:not(.no-export)' } }
        ],
        
        language: {
            processing: '<div class="spinner-border text-primary"></div>',
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No providers found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ providers'
        },
        
        stateSave: true,
        searchDelay: 300
    });
}

function setupTabHandlers() {
    $('[data-bs-toggle="tab"]').on('shown.bs.tab', function(e) {
        const target = e.target.getAttribute('href').substring(1);
        const typeMap = { 'all': null, 'sms': 0, 'mms': 1, 'email': 2 };
        currentType = typeMap[target];
        providersTable.ajax.reload();
    });
}

function viewHealth(id) { window.location.href = AppUrls.providers?.health ? AppUrls.providers.health(id) : `/Providers/Health/${id}`; }
function editProvider(id) { window.location.href = AppUrls.providers?.edit ? AppUrls.providers.edit(id) : `/Providers/Edit/${id}`; }

function testProvider(id) {
    confirmAction('Send a test message through this provider?', function() {
        $.ajax({
            url: `/Providers/Test?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Test message sent successfully!', 'success');
                } else {
                    showNotification(response.message || 'Test failed', 'error');
                }
            },
            error: function() {
                showNotification('Failed to send test message', 'error');
            }
        });
    });
}

function deleteProvider(id) {
    confirmAction('Are you sure you want to delete this provider?', function() {
        $.ajax({
            url: `/Providers/Delete?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Provider deleted successfully!', 'success');
                    providersTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete provider', 'error');
                }
            },
            error: function() {
                showNotification('Failed to delete provider', 'error');
            }
        });
    });
}
