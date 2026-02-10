/**
 * Keywords Index Page - DataTables Implementation
 * SERVER-SIDE: Calls Web controller endpoints instead of API directly
 */

let keywordsTable;
let currentStatus = null;

// Initialize on document ready
$(document).ready(function() {
    initKeywordsTable();
    setupFilters();
});

/**
 * Initialize DataTable for keywords
 */
function initKeywordsTable() {
    keywordsTable = $('#keywordsTable').DataTable({
        // Server-side processing
        serverSide: true,
        processing: true,

        // AJAX configuration
        ajax: {
            url: '/Keywords/GetKeywords', // SERVER-SIDE: Call Web controller
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Send DataTables request as JSON
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    isActive: currentStatus
                });
            },
            dataSrc: function(json) {
                // Web controller returns DataTables format already
                return json.data || [];
            },
            error: function(xhr, error, code) {
                console.error('DataTables error:', error, code);
                handleAjaxError(xhr, 'Failed to load keywords');
            }
        },
        
        // Column definitions
        columns: [
            {
                data: 'keywordText',
                name: 'Keyword',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        var desc = row.description ? '<br/><small class="text-muted">' + escapeHtml(row.description) + '</small>' : '';
                        return '<strong class="text-primary">' + escapeHtml(data) + '</strong>' + desc;
                    }
                    return data;
                }
            },
            {
                data: 'responseMessage',
                name: 'Auto Response',
                orderable: false,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        var truncated = data && data.length > 50 ? data.substring(0, 50) + '...' : data;
                        return '<small>' + escapeHtml(truncated || 'N/A') + '</small>';
                    }
                    return data;
                }
            },
            {
                data: 'linkedCampaignName',
                name: 'Campaign',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return data ? escapeHtml(data) : '<span class="text-muted">None</span>';
                    }
                    return data;
                }
            },
            {
                data: 'activityCount',
                name: 'Activity',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return formatNumber(data || 0);
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
                data: 'id',
                name: 'Actions',
                orderable: false,
                searchable: false,
                className: 'no-export text-end',
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `
                            <div class="btn-group btn-group-sm" role="group">
                                <button class="btn btn-outline-primary" onclick="viewKeyword(${data})" title="View">
                                    <i class="bi bi-eye"></i>
                                </button>
                                <button class="btn btn-outline-success" onclick="editKeyword(${data})" title="Edit">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="btn btn-outline-danger" onclick="deleteKeyword(${data})" title="Delete">
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
        order: [[0, 'asc']], // Sort by keyword ascending
        
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
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No keywords found</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching keywords found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ keywords',
            infoEmpty: 'Showing 0 to 0 of 0 keywords',
            infoFiltered: '(filtered from _MAX_ total keywords)',
            lengthMenu: 'Show _MENU_ keywords',
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
 * Setup filter buttons
 */
function setupFilters() {
    $('.filter-btn').on('click', function() {
        $('.filter-btn').removeClass('active');
        $(this).addClass('active');
        
        const status = $(this).data('status');
        currentStatus = status === 'all' ? null : (status === 'active');
        
        if (keywordsTable) {
            keywordsTable.ajax.reload();
        }
    });
}

// ============================================================================
// KEYWORD ACTION FUNCTIONS
// ============================================================================

/**
 * View keyword details
 */
function viewKeyword(id) {
    window.location.href = window.AppUrls.keywords?.details ? window.AppUrls.keywords.details(id) : `/Keywords/Details/${id}`;
}

/**
 * Edit keyword
 */
function editKeyword(id) {
    window.location.href = window.AppUrls.keywords?.edit ? window.AppUrls.keywords.edit(id) : `/Keywords/Edit/${id}`;
}

/**
 * Delete a keyword
 */
function deleteKeyword(id) {
    confirmAction('Are you sure you want to delete this keyword?', function() {
        $.ajax({
            url: `/Keywords/Delete?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success || response.isSuccess) {
                    showNotification(response.message || 'Keyword deleted successfully!', 'success');
                    keywordsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete keyword', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to delete keyword');
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
            window.location.href = window.AppUrls.auth.login;
        }, 2000);
    } else if (xhr.status === 403) {
        showNotification('You do not have permission to perform this action', 'error');
    } else if (xhr.status === 404) {
        showNotification('Keyword not found', 'error');
    } else {
        const errorMessage = xhr.responseJSON?.message || defaultMessage;
        showNotification(errorMessage, 'error');
    }
}
