/**
 * Roles Index Page - DataTables Implementation
 * SERVER-SIDE API INTEGRATION - Calls Web controller, not API directly
 */

// Global variables
let rolesTable;

// Initialize on document ready
$(document).ready(function() {
    initRolesTable();
});

/**
 * Initialize DataTable for roles
 */
function initRolesTable() {
    rolesTable = $('#rolesTable').DataTable({
        // Server-side processing
        serverSide: true,
        processing: true,

        // AJAX configuration
        ajax: {
            url: '/Roles/GetRoles', // SERVER-SIDE: Call Web controller, not API
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
            },
            error: function(xhr, error, code) {
                console.error('DataTables error:', error, code);
                handleAjaxError(xhr, 'Failed to load roles');
            }
        },
        
        // Column definitions
        columns: [
            {
                data: 'name',
                name: 'Name',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const roleColors = {
                            'SuperAdmin': 'danger',
                            'Admin': 'warning',
                            'Manager': 'info',
                            'User': 'secondary'
                        };
                        const color = roleColors[data] || 'primary';
                        return createBadge(data, color);
                    }
                    return data;
                }
            },
            {
                data: 'description',
                name: 'Description',
                orderable: false,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return data ? `<small class="text-muted">${escapeHtml(data)}</small>` : '<span class="text-muted">No description</span>';
                    }
                    return data;
                }
            },
            {
                data: 'userCount',
                name: 'Users',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `<span class="badge bg-info">${formatNumber(data || 0)} users</span>`;
                    }
                    return data;
                }
            },
            {
                data: 'permissions',
                name: 'Permissions',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const count = Array.isArray(data) ? data.length : (data || 0);
                        return `<small class="text-muted">${count} permissions</small>`;
                    }
                    return data;
                }
            },
            {
                data: 'createdAt',
                name: 'Created',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return formatShortDate(data);
                    }
                    return data;
                }
            },
            {
                data: 'id',
                name: 'Actions',
                orderable: false,
                searchable: false,
                className: 'no-export text-end',
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `
                            <div class="btn-group btn-group-sm" role="group">
                                <button class="btn btn-outline-primary" onclick="viewRole(${data})" title="View">
                                    <i class="bi bi-eye"></i>
                                </button>
                                <button class="btn btn-outline-success" onclick="editRole(${data})" title="Edit">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="btn btn-outline-danger" onclick="deleteRole(${data})" title="Delete">
                                    <i class="bi bi-trash"></i>
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
        order: [[0, 'asc']], // Sort by name ascending
        
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
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No roles found</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching roles found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ roles',
            infoEmpty: 'Showing 0 to 0 of 0 roles',
            infoFiltered: '(filtered from _MAX_ total roles)',
            lengthMenu: 'Show _MENU_ roles',
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

// ============================================================================
// ROLE ACTION FUNCTIONS
// ============================================================================

/**
 * Create new role
 */
function createRole() {
    window.location.href = AppUrls.roles?.create || '/Roles/Create';
}

/**
 * View role details
 */
function viewRole(id) {
    window.location.href = AppUrls.roles?.details ? AppUrls.roles.details(id) : `/Roles/Details/${id}`;
}

/**
 * Edit role
 */
function editRole(id) {
    window.location.href = AppUrls.roles?.edit ? AppUrls.roles.edit(id) : `/Roles/Edit/${id}`;
}

/**
 * Delete a role
 */
function deleteRole(id) {
    confirmAction('Are you sure you want to delete this role? Users with this role will need to be reassigned.', function() {
        $.ajax({
            url: `/Roles/Delete?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Role deleted successfully!', 'success');
                    rolesTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete role', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to delete role');
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
        showNotification('Role not found', 'error');
    } else {
        const errorMessage = xhr.responseJSON?.message || defaultMessage;
        showNotification(errorMessage, 'error');
    }
}
