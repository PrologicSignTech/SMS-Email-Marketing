/**
 * Suppression Lists Index Page - DataTables Implementation
 * SERVER-SIDE API INTEGRATION - Calls Web controller, not API directly
 */

let suppressionTable;
let currentFilter = null;

$(document).ready(function() {
    initSuppressionTable();
    setupTypeFilter();
});

function initSuppressionTable() {
    suppressionTable = $('#suppressionTable').DataTable({
        serverSide: true,
        processing: true,

        ajax: {
            url: '/Suppression/GetSuppressionList', // SERVER-SIDE: Call Web controller, not API
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Send DataTables request as JSON
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    type: currentFilter
                });
            },
            dataSrc: function(json) {
                // Web controller returns DataTables format already
                return json.data || [];
            }
        },
        
        columns: [
            {
                data: 'phoneOrEmail',
                name: 'PhoneOrEmail',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return '<strong>' + escapeHtml(data || 'N/A') + '</strong>';
                    }
                    return data;
                }
            },
            {
                data: 'type',
                name: 'Type',
                orderable: true,
                searchable: false,
                render: function(data) {
                    var typeNames = ['Opt-Out', 'Bounce', 'Complaint', 'Manual'];
                    var typeColors = { 0: 'danger', 1: 'warning', 2: 'danger', 3: 'info' };
                    var typeName = typeNames[data] || 'Unknown';
                    var color = typeColors[data] || 'secondary';
                    return createBadge(typeName, color);
                }
            },
            {
                data: 'reason',
                name: 'Reason',
                orderable: false,
                searchable: true,
                render: function(data) {
                    if (!data) return '<span class="text-muted">N/A</span>';
                    var truncated = data.length > 50 ? data.substring(0, 50) + '...' : data;
                    return '<small>' + escapeHtml(truncated) + '</small>';
                }
            },
            {
                data: 'createdAt',
                name: 'Created',
                orderable: true,
                searchable: false,
                render: function(data) {
                    return formatShortDate(data);
                }
            },
            {
                data: 'id',
                name: 'Actions',
                orderable: false,
                searchable: false,
                className: 'no-export text-end',
                render: function(data) {
                    return '<div class="btn-group btn-group-sm">' +
                        '<button class="btn btn-outline-danger" onclick="deleteList(' + data + ')"><i class="bi bi-trash"></i></button>' +
                        '</div>';
                }
            }
        ],
        
        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[3, 'desc']],
        
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
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No suppression lists found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ lists'
        },
        
        stateSave: true,
        searchDelay: 300
    });
}

function setupTypeFilter() {
    $('#typeFilter').on('change', function() {
        currentFilter = $(this).val() || null;
        suppressionTable.ajax.reload();
    });
}

function viewEntries(id) { window.location.href = (typeof AppUrls !== 'undefined' && AppUrls?.suppression?.entries) ? AppUrls.suppression.entries(id) : `/Suppression/Entries/${id}`; }
function editList(id) { window.location.href = (typeof AppUrls !== 'undefined' && AppUrls?.suppression?.edit) ? AppUrls.suppression.edit(id) : `/Suppression/Edit/${id}`; }

function exportList(id) {
    showNotification('Exporting suppression list...', 'info');
    // Export still goes direct to API for file download
    const apiBaseUrl = window.location.origin;
    window.location.href = `${apiBaseUrl}/api/suppression/${id}/export`;
}

function deleteList(id) {
    confirmAction('Are you sure you want to delete this suppression list?', function() {
        $.ajax({
            url: `/Suppression/Delete?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Suppression list deleted successfully!', 'success');
                    suppressionTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete suppression list', 'error');
                }
            },
            error: function() {
                showNotification('Failed to delete suppression list', 'error');
            }
        });
    });
}
