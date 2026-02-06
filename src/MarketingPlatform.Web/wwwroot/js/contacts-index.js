/**
 * Contacts Index Page - DataTables Implementation
 * Handles contact listing with server-side pagination, filtering, and actions
 */

let contactsTable;
let currentGroup = null;
let currentStatus = null;

// Initialize on document ready
$(document).ready(function() {
    initContactsTable();
    setupFilters();
    loadContactGroups();
});

/**
 * Initialize DataTable for contacts
 */
function initContactsTable() {
    contactsTable = $('#contactsTable').DataTable({
        // Server-side processing
        serverSide: true,
        processing: true,
        
        // AJAX configuration (SERVER-SIDE)
        ajax: {
            url: '/Contacts/GetContacts',
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    order: d.order,
                    status: currentStatus
                });
            },
            dataSrc: function(json) {
                return json.data || [];
            },
            error: function(xhr, error, code) {
                console.error('DataTables error:', error, code);
                handleAjaxError(xhr, 'Failed to load contacts');
            }
        },
        
        // Column definitions
        columns: [
            {
                data: null,
                orderable: false,
                searchable: false,
                className: 'select-checkbox',
                render: function(data, type, row) {
                    return `<input type="checkbox" class="contact-checkbox" value="${row.id}">`;
                }
            },
            {
                data: 'firstName',
                name: 'Name',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `<strong>${escapeHtml(row.firstName)} ${escapeHtml(row.lastName)}</strong>`;
                    }
                    return data;
                }
            },
            {
                data: 'email',
                name: 'Email',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return escapeHtml(data);
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
                        return data ? escapeHtml(data) : 'N/A';
                    }
                    return data;
                }
            },
            {
                data: 'groups',
                name: 'Groups',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        if (!data || data.length === 0) return '<span class="text-muted">None</span>';
                        return data.map(g => createBadge(escapeHtml(g.name || g), 'secondary')).join(' ');
                    }
                    return data;
                }
            },
            {
                data: 'status',
                name: 'Status',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const statusColors = {
                            'Active': 'success',
                            'Unsubscribed': 'warning',
                            'Bounced': 'danger'
                        };
                        const color = statusColors[data] || 'secondary';
                        return createBadge(data, color);
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
                                <a href="/Contacts/Details/${data}" class="btn btn-outline-primary" title="Details">
                                    <i class="bi bi-eye"></i>
                                </a>
                                <a href="/Contacts/Edit/${data}" class="btn btn-outline-success" title="Edit">
                                    <i class="bi bi-pencil"></i>
                                </a>
                                <button class="btn btn-outline-danger" onclick="deleteContact(${data})" title="Delete">
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
        order: [[1, 'asc']], // Sort by name ascending
        
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
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No contacts found</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching contacts found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ contacts',
            infoEmpty: 'Showing 0 to 0 of 0 contacts',
            infoFiltered: '(filtered from _MAX_ total contacts)',
            lengthMenu: 'Show _MENU_ contacts',
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
    
    // Setup select all checkbox
    const selectAllEl = document.getElementById('selectAll');
    if (selectAllEl) {
        selectAllEl.addEventListener('click', function() {
            document.querySelectorAll('.contact-checkbox').forEach(cb => {
                cb.checked = this.checked;
            });
        });
    }
}

/**
 * Setup filter dropdowns
 */
function setupFilters() {
    $('#groupFilter').on('change', function() {
        const value = $(this).val();
        currentGroup = value === '' ? null : parseInt(value);
        if (contactsTable) {
            contactsTable.ajax.reload();
        }
    });
    
    $('#statusFilter').on('change', function() {
        currentStatus = $(this).val() || null;
        if (contactsTable) {
            contactsTable.ajax.reload();
        }
    });
}

/**
 * Load contact groups for filter (SERVER-SIDE)
 */
function loadContactGroups() {
    $.ajax({
        url: '/Contacts/GetContactGroups',
        method: 'GET',
        success: function(response) {
            const groups = response.items || response.data || response || [];
            const select = $('#groupFilter');

            groups.forEach(group => {
                select.append(`<option value="${group.id}">${escapeHtml(group.name)}</option>`);
            });
        },
        error: function(xhr) {
            console.error('Failed to load contact groups:', xhr);
        }
    });
}

// ============================================================================
// CONTACT ACTION FUNCTIONS
// ============================================================================

/**
 * Delete a contact (SERVER-SIDE)
 */
function deleteContact(id) {
    confirmAction('Are you sure you want to delete this contact?', function() {
        $.ajax({
            url: `/Contacts/Delete?id=${id}`,
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Contact deleted successfully!', 'success');
                    contactsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete contact', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to delete contact');
            }
        });
    });
}

// Note: Import and Export functionality is provided by DataTables export buttons
// These are placeholder functions for future custom import/export features if needed

/**
 * Handle AJAX errors
 */
function handleAjaxError(xhr, defaultMessage) {
    if (xhr.status === 401) {
        showNotification('Session expired. Please log in again.', 'error');
        setTimeout(() => {
            window.location.href = window.AppUrls.auth.login;
        }, 2000);
    } else if (xhr.status === 403) {
        showNotification('You do not have permission to perform this action', 'error');
    } else if (xhr.status === 404) {
        showNotification('Contact not found', 'error');
    } else {
        const errorMessage = xhr.responseJSON?.message || defaultMessage;
        showNotification(errorMessage, 'error');
    }
}
