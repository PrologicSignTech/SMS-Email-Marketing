/**
 * URLs Index Page - DataTables Implementation
 * SERVER-SIDE API INTEGRATION - Calls Web controller endpoints
 */

// Global variables
let urlsTable;

// Initialize on document ready
$(document).ready(function() {
    initUrlsTable();
});

/**
 * Initialize DataTable for URLs
 */
function initUrlsTable() {
    urlsTable = $('#urlsTable').DataTable({
        // Server-side processing
        serverSide: true,
        processing: true,

        // AJAX configuration
        ajax: {
            url: '/Urls/GetUrls', // SERVER-SIDE: Call Web controller, not API
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
        
        // Column definitions
        columns: [
            {
                data: 'shortCode',
                name: 'Short URL',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const shortUrl = `${window.location.origin}/s/${escapeHtml(data)}`;
                        return `
                            <div>
                                <code class="text-primary">${escapeHtml(data)}</code>
                                <button class="btn btn-sm btn-link p-0 ms-2" onclick="copyUrl('${shortUrl}')" title="Copy URL">
                                    <i class="bi bi-clipboard"></i>
                                </button>
                            </div>
                        `;
                    }
                    return data;
                }
            },
            {
                data: 'originalUrl',
                name: 'Original URL',
                orderable: false,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const truncated = data && data.length > 50 ? data.substring(0, 50) + '...' : data;
                        return `<small class="text-muted" title="${escapeHtml(data)}">${escapeHtml(truncated)}</small>`;
                    }
                    return data;
                }
            },
            {
                data: 'shortUrl',
                name: 'Full URL',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return data ? '<small class="text-primary">' + escapeHtml(data) + '</small>' : '<span class="text-muted">N/A</span>';
                    }
                    return data;
                }
            },
            {
                data: 'clickCount',
                name: 'Clicks',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        return '<span class="badge bg-primary">' + formatNumber(data || 0) + '</span>';
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
                                <button class="btn btn-outline-primary" onclick="viewAnalytics(${data})" title="View Analytics">
                                    <i class="bi bi-graph-up"></i>
                                </button>
                                <button class="btn btn-outline-success" onclick="editUrl(${data})" title="Edit">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="btn btn-outline-danger" onclick="deleteUrl(${data})" title="Delete">
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
        order: [[5, 'desc']], // Sort by created date descending
        
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
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No tracked URLs found</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching URLs found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ URLs',
            infoEmpty: 'Showing 0 to 0 of 0 URLs',
            infoFiltered: '(filtered from _MAX_ total URLs)',
            lengthMenu: 'Show _MENU_ URLs',
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
// URL ACTION FUNCTIONS
// ============================================================================

/**
 * Copy URL to clipboard with modern API and legacy fallback
 */
function copyUrl(url) {
    // Modern Clipboard API (preferred)
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(url).then(function() {
            showNotification('URL copied to clipboard!', 'success', 2000);
        }).catch(function(err) {
            console.error('Failed to copy: ', err);
            // Fallback on error
            legacyCopyToClipboard(url);
        });
    } else {
        // Legacy fallback for older browsers
        legacyCopyToClipboard(url);
    }
}

/**
 * Legacy clipboard copy method
 */
function legacyCopyToClipboard(text) {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.top = '-999999px';
    textArea.style.left = '-999999px';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    
    try {
        const successful = document.execCommand('copy');
        if (successful) {
            showNotification('URL copied to clipboard!', 'success', 2000);
        } else {
            showNotification('Failed to copy URL. Please copy manually.', 'error');
        }
    } catch (err) {
        console.error('Failed to copy: ', err);
        showNotification('Failed to copy URL. Please copy manually.', 'error');
    }
    
    document.body.removeChild(textArea);
}

/**
 * View URL analytics
 */
function viewAnalytics(id) {
    window.location.href = AppUrls.urls?.analytics ? AppUrls.urls.analytics(id) : `/Urls/Analytics/${id}`;
}

/**
 * Edit URL
 */
function editUrl(id) {
    window.location.href = AppUrls.urls?.edit ? AppUrls.urls.edit(id) : `/Urls/Edit/${id}`;
}

/**
 * Delete a URL
 */
function deleteUrl(id) {
    confirmAction('Are you sure you want to delete this tracked URL? Click statistics will be lost.', function() {
        $.ajax({
            url: `/Urls/Delete?id=${id}`, // SERVER-SIDE: Call Web controller
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'URL deleted successfully!', 'success');
                    urlsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete URL', 'error');
                }
            },
            error: function() {
                showNotification('Failed to delete URL', 'error');
            }
        });
    });
}
