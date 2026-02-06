/**
 * Workflows Index Page - DataTables Implementation
 */

let workflowsTable;
let currentStatus = null;

const WorkflowStatus = { All: null, Active: 0, Draft: 1, Paused: 2 };

$(document).ready(function() {
    initWorkflowsTable();
    setupTabHandlers();
});

function initWorkflowsTable() {
    workflowsTable = $('#workflowsTable').DataTable({
        serverSide: true,
        processing: true,

        ajax: {
            url: '/Workflows/GetWorkflows', // SERVER-SIDE: Call Web controller, not API
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Send DataTables request as JSON
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    status: currentStatus
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
                        const description = row.description ? `<br/><small class="text-muted">${escapeHtml(row.description)}</small>` : '';
                        return `<strong>${escapeHtml(data)}</strong>${description}`;
                    }
                    return data;
                }
            },
            {
                data: 'trigger',
                name: 'Trigger',
                orderable: false,
                searchable: false,
                render: function(data) {
                    return `<small>${escapeHtml(data || 'Manual')}</small>`;
                }
            },
            {
                data: 'status',
                name: 'Status',
                orderable: true,
                searchable: false,
                render: function(data) {
                    const statusNames = ['Active', 'Draft', 'Paused'];
                    const statusColors = ['success', 'secondary', 'warning'];
                    return createBadge(statusNames[data] || 'Unknown', statusColors[data] || 'secondary');
                }
            },
            {
                data: 'executionsCount',
                name: 'Executions',
                orderable: true,
                searchable: false,
                render: function(data) {
                    return formatNumber(data || 0);
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
                render: function(data, type, row) {
                    return `
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-primary" onclick="viewWorkflow(${data})"><i class="bi bi-eye"></i></button>
                            <button class="btn btn-outline-secondary" onclick="editWorkflow(${data})"><i class="bi bi-pencil"></i></button>
                            ${row.status === 1 ? `<button class="btn btn-outline-success" onclick="activateWorkflow(${data})"><i class="bi bi-play"></i></button>` : ''}
                            ${row.status === 0 ? `<button class="btn btn-outline-warning" onclick="pauseWorkflow(${data})"><i class="bi bi-pause"></i></button>` : ''}
                            <button class="btn btn-outline-danger" onclick="deleteWorkflow(${data})"><i class="bi bi-trash"></i></button>
                        </div>
                    `;
                }
            }
        ],
        
        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[4, 'desc']],
        
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
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No workflows found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ workflows'
        },
        
        stateSave: true,
        searchDelay: 300
    });
}

function setupTabHandlers() {
    $('[data-bs-toggle="tab"]').on('shown.bs.tab', function(e) {
        const target = e.target.getAttribute('href');
        const statusMap = { '#all': null, '#active': WorkflowStatus.Active, '#draft': WorkflowStatus.Draft };
        currentStatus = statusMap[target];
        workflowsTable.ajax.reload();
    });
}

function viewWorkflow(id) { window.location.href = AppUrls.workflows?.details ? AppUrls.workflows.details(id) : `/Workflows/Details/${id}`; }
function editWorkflow(id) { window.location.href = AppUrls.workflows?.edit ? AppUrls.workflows.edit(id) : `/Workflows/Edit/${id}`; }

function activateWorkflow(id) {
    confirmAction('Activate this workflow?', function() {
        $.ajax({
            url: `/Workflows/Activate?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Workflow activated successfully!', 'success');
                    workflowsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to activate workflow', 'error');
                }
            },
            error: function() { showNotification('Failed to activate workflow', 'error'); }
        });
    });
}

function pauseWorkflow(id) {
    confirmAction('Pause this workflow?', function() {
        $.ajax({
            url: `/Workflows/Pause?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Workflow paused successfully!', 'success');
                    workflowsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to pause workflow', 'error');
                }
            },
            error: function() { showNotification('Failed to pause workflow', 'error'); }
        });
    });
}

function deleteWorkflow(id) {
    confirmAction('Delete this workflow?', function() {
        $.ajax({
            url: `/Workflows/Delete?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Workflow deleted successfully!', 'success');
                    workflowsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete workflow', 'error');
                }
            },
            error: function() { showNotification('Failed to delete workflow', 'error'); }
        });
    });
}
