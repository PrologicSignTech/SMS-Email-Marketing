/**
 * Users Index Page - DataTables Implementation
 * SERVER-SIDE API INTEGRATION - Calls Web controller, not API directly
 */

// Global variables
let usersTable;
let currentRole = null;
let currentStatus = null;

// Initialize on document ready
$(document).ready(function() {
    initUsersTable();
    setupFilters();
});

/**
 * Initialize DataTable for users
 */
function initUsersTable() {
    usersTable = $('#usersTable').DataTable({
        // Server-side processing
        serverSide: true,
        processing: true,

        // AJAX configuration
        ajax: {
            url: '/Users/GetUsers', // SERVER-SIDE: Call Web controller, not API
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Send DataTables request as JSON
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    roleId: currentRole,
                    status: currentStatus
                });
            },
            dataSrc: function(json) {
                // Web controller returns DataTables format already
                return json.data || [];
            },
            error: function(xhr, error, code) {
                console.error('DataTables error:', error, code);
                handleAjaxError(xhr, 'Failed to load users');
            }
        },
        
        // Column definitions
        columns: [
            {
                data: 'firstName',
                name: 'Name',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const fullName = `${escapeHtml(row.firstName)} ${escapeHtml(row.lastName)}`;
                        return `<div><strong>${fullName}</strong><br/><small class="text-muted">${escapeHtml(row.email)}</small></div>`;
                    }
                    return data;
                }
            },
            {
                data: 'role',
                name: 'Role',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const roleName = data?.name || data || 'No Role';
                        const roleColors = {
                            'SuperAdmin': 'danger',
                            'Admin': 'warning',
                            'Manager': 'info',
                            'User': 'secondary'
                        };
                        const color = roleColors[roleName] || 'secondary';
                        return createBadge(roleName, color);
                    }
                    return data;
                }
            },
            {
                data: 'phoneNumber',
                name: 'Phone',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return data ? escapeHtml(data) : '<span class="text-muted">N/A</span>';
                    }
                    return data;
                }
            },
            {
                data: 'isActive',
                name: 'Status',
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
                data: 'lastLoginAt',
                name: 'Last Login',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return data ? formatDate(data) : '<span class="text-muted">Never</span>';
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
                                <button class="btn btn-outline-primary" onclick="viewUser(${data})" title="View">
                                    <i class="bi bi-eye"></i>
                                </button>
                                <button class="btn btn-outline-success" onclick="editUser(${data})" title="Edit">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="btn btn-outline-danger" onclick="deleteUser(${data})" title="Delete">
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
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No users found</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching users found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ users',
            infoEmpty: 'Showing 0 to 0 of 0 users',
            infoFiltered: '(filtered from _MAX_ total users)',
            lengthMenu: 'Show _MENU_ users',
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
 * Setup filter dropdowns
 */
function setupFilters() {
    $('#roleFilter').on('change', function() {
        const value = $(this).val();
        currentRole = value === '' ? null : parseInt(value);
        if (usersTable) {
            usersTable.ajax.reload();
        }
    });
    
    $('#statusFilter').on('change', function() {
        const value = $(this).val();
        currentStatus = value === '' ? null : (value === 'active');
        if (usersTable) {
            usersTable.ajax.reload();
        }
    });
}

// ============================================================================
// USER ACTION FUNCTIONS
// ============================================================================

/**
 * View user details
 */
function viewUser(id) {
    window.location.href = AppUrls.users?.details ? AppUrls.users.details(id) : `/Users/Details/${id}`;
}

/**
 * Edit user
 */
function editUser(id) {
    window.location.href = AppUrls.users?.edit ? AppUrls.users.edit(id) : `/Users/Edit/${id}`;
}

/**
 * Delete a user
 */
function deleteUser(id) {
    confirmAction('Are you sure you want to delete this user? This action cannot be undone.', function() {
        $.ajax({
            url: `/Users/Delete?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'User deleted successfully!', 'success');
                    usersTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete user', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to delete user');
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
        showNotification('User not found', 'error');
    } else {
        const errorMessage = xhr.responseJSON?.message || defaultMessage;
        showNotification(errorMessage, 'error');
    }
}
