/**
 * Templates Index Page - DataTables Implementation
 * SERVER-SIDE: Calls Web controller endpoints instead of API directly
 */

let templatesTable;
let currentType = null;

$(document).ready(function() {
    initTemplatesTable();
    setupTabHandlers();
});

function initTemplatesTable() {
    templatesTable = $('#templatesTable').DataTable({
        serverSide: true,
        processing: true,

        ajax: {
            url: '/Templates/GetTemplates', // SERVER-SIDE: Call Web controller
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Send DataTables request as JSON
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    status: currentType
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
                render: function(data, type, row) {
                    if (type === 'display') {
                        const subject = row.subject ? `<br/><small class="text-muted">${escapeHtml(row.subject)}</small>` : '';
                        return `<strong>${escapeHtml(data)}</strong>${subject}`;
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
                data: 'lastModified',
                name: 'LastModified',
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
                    return `
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-primary" onclick="previewTemplate(${data})"><i class="bi bi-eye"></i></button>
                            <button class="btn btn-outline-secondary" onclick="editTemplate(${data})"><i class="bi bi-pencil"></i></button>
                            <button class="btn btn-outline-info" onclick="duplicateTemplate(${data})"><i class="bi bi-files"></i></button>
                            <button class="btn btn-outline-danger" onclick="deleteTemplate(${data})"><i class="bi bi-trash"></i></button>
                        </div>
                    `;
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
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No templates found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ templates'
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
        templatesTable.ajax.reload();
    });
}

function previewTemplate(id) { window.location.href = AppUrls.templates?.preview ? AppUrls.templates.preview(id) : `/Templates/Preview/${id}`; }
function editTemplate(id) { window.location.href = AppUrls.templates?.edit ? AppUrls.templates.edit(id) : `/Templates/Edit/${id}`; }

function duplicateTemplate(id) {
    confirmAction('Duplicate this template?', function() {
        $.ajax({
            url: `/Templates/Duplicate?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Template duplicated successfully!', 'success');
                    templatesTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to duplicate template', 'error');
                }
            },
            error: function() { showNotification('Failed to duplicate template', 'error'); }
        });
    });
}

function deleteTemplate(id) {
    confirmAction('Delete this template?', function() {
        $.ajax({
            url: `/Templates/Delete?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Template deleted successfully!', 'success');
                    templatesTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete template', 'error');
                }
            },
            error: function() { showNotification('Failed to delete template', 'error'); }
        });
    });
}
