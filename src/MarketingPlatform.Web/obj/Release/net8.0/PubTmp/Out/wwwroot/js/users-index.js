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
        serverSide: true,
        processing: true,

        ajax: {
            url: '/Users/GetUsers',
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
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
                return json.data || [];
            },
            error: function(xhr, error, code) {
                console.error('DataTables error:', error, code);
                handleAjaxError(xhr, 'Failed to load users');
            }
        },

        columns: [
            {
                data: 'firstName',
                name: 'Name',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        var fullName = escapeHtml((row.firstName || '') + ' ' + (row.lastName || ''));
                        return '<div><strong>' + fullName + '</strong><br/><small class="text-muted">' + escapeHtml(row.email || '') + '</small></div>';
                    }
                    return data;
                }
            },
            {
                data: 'roles',
                name: 'Role',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        if (!data || data.length === 0) return '<span class="text-muted">No Role</span>';
                        var roleColors = { 'SuperAdmin': 'danger', 'Admin': 'warning', 'Manager': 'info', 'User': 'secondary' };
                        return data.map(function(r) {
                            return createBadge(r, roleColors[r] || 'secondary');
                        }).join(' ');
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
                        var activeBtn = row.isActive
                            ? '<button class="btn btn-outline-warning" onclick="toggleUserStatus(\'' + data + '\', true)" title="Deactivate"><i class="bi bi-lock"></i></button>'
                            : '<button class="btn btn-outline-success" onclick="toggleUserStatus(\'' + data + '\', false)" title="Activate"><i class="bi bi-unlock"></i></button>';
                        return '<div class="btn-group btn-group-sm" role="group">' +
                            '<button class="btn btn-outline-primary" onclick="viewUser(\'' + data + '\')" title="View"><i class="bi bi-eye"></i></button>' +
                            '<button class="btn btn-outline-success" onclick="editUser(\'' + data + '\')" title="Edit"><i class="bi bi-pencil"></i></button>' +
                            activeBtn +
                            '<button class="btn btn-outline-danger" onclick="deleteUser(\'' + data + '\')" title="Delete"><i class="bi bi-trash"></i></button>' +
                            '</div>';
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
            { extend: 'csv', className: 'btn btn-sm btn-outline-primary me-1', text: '<i class="bi bi-file-earmark-csv"></i> CSV', exportOptions: { columns: ':visible:not(.no-export)' } },
            { extend: 'excel', className: 'btn btn-sm btn-outline-success me-1', text: '<i class="bi bi-file-earmark-excel"></i> Excel', exportOptions: { columns: ':visible:not(.no-export)' } },
            { extend: 'pdf', className: 'btn btn-sm btn-outline-danger', text: '<i class="bi bi-file-earmark-pdf"></i> PDF', exportOptions: { columns: ':visible:not(.no-export)' }, orientation: 'landscape', pageSize: 'A4' }
        ],

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

        stateSave: true,
        stateDuration: 60 * 60 * 24,
        searchDelay: 300
    });
}

function setupFilters() {
    $('#roleFilter').on('change', function() {
        var value = $(this).val();
        currentRole = value === '' ? null : parseInt(value);
        if (usersTable) usersTable.ajax.reload();
    });

    $('#statusFilter').on('change', function() {
        var value = $(this).val();
        currentStatus = value === '' ? null : (value === 'active');
        if (usersTable) usersTable.ajax.reload();
    });
}

function viewUser(id) {
    document.getElementById('viewUserLoading').style.display = 'block';
    document.getElementById('viewUserContent').style.display = 'none';
    var modal = new bootstrap.Modal(document.getElementById('viewUserModal'));
    modal.show();

    $.get('/Users/GetUser?id=' + id, function(response) {
        if (response.success && response.data) {
            var u = response.data;
            document.getElementById('viewFirstName').textContent = u.firstName || '-';
            document.getElementById('viewLastName').textContent = u.lastName || '-';
            document.getElementById('viewEmail').textContent = u.email || '-';
            document.getElementById('viewPhone').textContent = u.phoneNumber || 'N/A';
            document.getElementById('viewStatus').innerHTML = u.isActive
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-secondary">Inactive</span>';
            var roles = u.roles || [];
            document.getElementById('viewRoles').innerHTML = roles.length > 0
                ? roles.map(function(r) { return '<span class="badge bg-info me-1">' + r + '</span>'; }).join('')
                : '<span class="text-muted">No roles</span>';
            document.getElementById('viewLastLogin').textContent = u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleString() : 'Never';
            document.getElementById('viewCreated').textContent = u.createdAt ? new Date(u.createdAt).toLocaleDateString() : '-';
            document.getElementById('viewCompany').textContent = u.companyName || u.company || '-';
        }
        document.getElementById('viewUserLoading').style.display = 'none';
        document.getElementById('viewUserContent').style.display = 'block';
    }).fail(function() {
        document.getElementById('viewUserLoading').innerHTML = '<div class="alert alert-danger">Failed to load user details</div>';
    });
}

function editUser(id) {
    document.getElementById('editUserLoading').style.display = 'block';
    document.getElementById('editUserForm').style.display = 'none';
    var modal = new bootstrap.Modal(document.getElementById('editUserModal'));
    modal.show();

    $.get('/Users/GetUser?id=' + id, function(response) {
        if (response.success && response.data) {
            var u = response.data;
            document.getElementById('editUserId').value = u.id || id;
            document.getElementById('editFirstName').value = u.firstName || '';
            document.getElementById('editLastName').value = u.lastName || '';
            document.getElementById('editEmail').value = u.email || '';
            document.getElementById('editPhoneNumber').value = u.phoneNumber || '';
            document.getElementById('editCompany').value = u.companyName || u.company || '';
            document.getElementById('editIsActive').checked = u.isActive !== false;
        }
        document.getElementById('editUserLoading').style.display = 'none';
        document.getElementById('editUserForm').style.display = 'block';
    }).fail(function() {
        document.getElementById('editUserLoading').innerHTML = '<div class="alert alert-danger">Failed to load user details</div>';
    });
}

function saveUser() {
    var btn = document.getElementById('saveUserBtn');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Saving...';

    var id = document.getElementById('editUserId').value;
    var data = {
        firstName: document.getElementById('editFirstName').value.trim(),
        lastName: document.getElementById('editLastName').value.trim(),
        email: document.getElementById('editEmail').value.trim(),
        phoneNumber: document.getElementById('editPhoneNumber').value.trim(),
        companyName: document.getElementById('editCompany').value.trim(),
        isActive: document.getElementById('editIsActive').checked
    };

    $.ajax({
        url: '/Users/UpdateUser?id=' + id,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function(response) {
            if (response.success) {
                showNotification('User updated successfully!', 'success');
                bootstrap.Modal.getInstance(document.getElementById('editUserModal')).hide();
                usersTable.ajax.reload(null, false);
            } else {
                showNotification(response.message || 'Failed to update user', 'error');
            }
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-check-circle"></i> Save Changes';
        },
        error: function() {
            showNotification('An error occurred', 'error');
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-check-circle"></i> Save Changes';
        }
    });
}

function toggleUserStatus(id, currentlyActive) {
    var action = currentlyActive ? 'deactivate' : 'activate';
    confirmAction('Are you sure you want to ' + action + ' this user?', function() {
        var url = currentlyActive ? '/Users/Deactivate?id=' + id : '/Users/Activate?id=' + id;
        $.ajax({
            url: url,
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification('User ' + action + 'd successfully!', 'success');
                    usersTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to ' + action + ' user', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to ' + action + ' user');
            }
        });
    });
}

function deleteUser(id) {
    confirmAction('Are you sure you want to delete this user? This action cannot be undone.', function() {
        $.ajax({
            url: '/Users/Delete?id=' + id,
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

function handleAjaxError(xhr, defaultMessage) {
    if (xhr.status === 401) {
        showNotification('Session expired. Please log in again.', 'error');
        setTimeout(function() { window.location.href = '/Account/Login'; }, 2000);
    } else if (xhr.status === 403) {
        showNotification('You do not have permission to perform this action', 'error');
    } else if (xhr.status === 404) {
        showNotification('User not found', 'error');
    } else {
        var errorMessage = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : defaultMessage;
        showNotification(errorMessage, 'error');
    }
}
