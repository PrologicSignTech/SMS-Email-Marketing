/**
 * Campaigns Index Page - DataTables Implementation
 * Handles campaign listing with server-side pagination, filtering, and actions
 */

let campaignsTable;
let currentStatus = null;

// Campaign enums
const CampaignStatus = {
    Draft: 0,
    Scheduled: 1,
    Running: 2,
    Paused: 3,
    Completed: 4,
    Failed: 5
};

const CampaignType = {
    SMS: 0,
    MMS: 1,
    Email: 2,
    Multi: 3
};

// Initialize on document ready
$(document).ready(function() {
    initCampaignsTable();
    setupTabHandlers();
});

/**
 * Initialize DataTable for campaigns
 */
function initCampaignsTable() {
    campaignsTable = $('#campaignsTable').DataTable({
        // Server-side processing
        serverSide: true,
        processing: true,
        
        // AJAX configuration
        ajax: {
            url: window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.list),
            type: 'POST',
            headers: getAjaxHeaders(),
            data: function(d) {
                return {
                    pageNumber: Math.floor(d.start / d.length) + 1,
                    pageSize: d.length,
                    searchTerm: d.search.value,
                    sortColumn: d.columns[d.order[0]?.column]?.data || 'createdAt',
                    sortDirection: d.order[0]?.dir || 'desc',
                    status: currentStatus
                };
            },
            dataSrc: function(json) {
                json.recordsTotal = json.totalCount || 0;
                json.recordsFiltered = json.totalCount || 0;
                return json.items || json.data?.items || [];
            },
            error: function(xhr, error, code) {
                console.error('DataTables error:', error, code);
                if (xhr.status === 401) {
                    showNotification('Session expired. Please log in again.', 'error');
                    setTimeout(() => {
                        window.location.href = window.AppUrls.auth.login;
                    }, 2000);
                } else if (xhr.status === 0) {
                    showNotification('Network error. Please check your connection.', 'error');
                }
            }
        },
        
        // Column definitions
        columns: [
            {
                data: 'name',
                name: 'Name',
                title: 'Name',
                orderable: true,
                searchable: true,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const description = row.description ? 
                            `<br/><small class="text-muted">${escapeHtml(row.description)}</small>` : '';
                        return `<strong>${escapeHtml(data)}</strong>${description}`;
                    }
                    return data;
                }
            },
            {
                data: 'type',
                name: 'Type',
                title: 'Type',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const typeNames = ['SMS', 'MMS', 'Email', 'Multi-Channel'];
                        const typeColors = ['primary', 'info', 'success', 'warning'];
                        const typeName = typeNames[data] || 'Unknown';
                        const typeColor = typeColors[data] || 'secondary';
                        return createBadge(typeName, typeColor);
                    }
                    return data;
                }
            },
            {
                data: 'status',
                name: 'Status',
                title: 'Status',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        const statusNames = ['Draft', 'Scheduled', 'Running', 'Paused', 'Completed', 'Failed'];
                        const statusColors = ['secondary', 'warning', 'primary', 'info', 'success', 'danger'];
                        const statusName = statusNames[data] || 'Unknown';
                        const statusColor = statusColors[data] || 'secondary';
                        return createBadge(statusName, statusColor);
                    }
                    return data;
                }
            },
            {
                data: 'scheduledAt',
                name: 'Schedule',
                title: 'Schedule',
                orderable: true,
                searchable: false,
                render: function(data, type, row) {
                    if (type === 'display') {
                        if (row.scheduledAt) {
                            return `<small>Scheduled: ${formatShortDate(row.scheduledAt)}</small>`;
                        } else if (row.startedAt) {
                            return `<small>Started: ${formatShortDate(row.startedAt)}</small>`;
                        } else if (row.completedAt) {
                            return `<small>Completed: ${formatShortDate(row.completedAt)}</small>`;
                        }
                        return '<small class="text-muted">Not scheduled</small>';
                    }
                    return data || '';
                }
            },
            {
                data: 'createdAt',
                name: 'CreatedAt',
                title: 'Created',
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
                title: 'Actions',
                orderable: false,
                searchable: false,
                className: 'no-export',
                render: function(data, type, row) {
                    if (type === 'display') {
                        return renderActionButtons(data, row);
                    }
                    return data;
                }
            }
        ],
        
        // Table configuration
        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[4, 'desc']], // Sort by created date descending by default
        
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
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No campaigns found</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching campaigns found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ campaigns',
            infoEmpty: 'Showing 0 to 0 of 0 campaigns',
            infoFiltered: '(filtered from _MAX_ total campaigns)',
            lengthMenu: 'Show _MENU_ campaigns',
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
        
        // Search delay (debouncing)
        searchDelay: 300
    });
}

/**
 * Render action buttons based on campaign status
 */
function renderActionButtons(id, row) {
    let buttons = `<div class="btn-group btn-group-sm" role="group">`;
    
    // View button (always available)
    buttons += `
        <button class="btn btn-outline-primary" onclick="viewCampaign(${id})" title="View Details">
            <i class="bi bi-eye"></i>
        </button>`;
    
    // Duplicate button (always available)
    buttons += `
        <button class="btn btn-outline-secondary" onclick="duplicateCampaign(${id})" title="Duplicate">
            <i class="bi bi-files"></i>
        </button>`;
    
    // Status-specific action buttons
    switch(row.status) {
        case CampaignStatus.Draft:
            buttons += `
                <button class="btn btn-outline-success" onclick="startCampaign(${id})" title="Start Campaign">
                    <i class="bi bi-play-fill"></i>
                </button>
                <button class="btn btn-outline-danger" onclick="deleteCampaign(${id})" title="Delete">
                    <i class="bi bi-trash"></i>
                </button>`;
            break;
            
        case CampaignStatus.Scheduled:
            buttons += `
                <button class="btn btn-outline-success" onclick="startCampaign(${id})" title="Start Now">
                    <i class="bi bi-play-fill"></i>
                </button>
                <button class="btn btn-outline-danger" onclick="cancelCampaign(${id})" title="Cancel">
                    <i class="bi bi-x-circle"></i>
                </button>`;
            break;
            
        case CampaignStatus.Running:
            buttons += `
                <button class="btn btn-outline-warning" onclick="pauseCampaign(${id})" title="Pause">
                    <i class="bi bi-pause-fill"></i>
                </button>
                <button class="btn btn-outline-danger" onclick="cancelCampaign(${id})" title="Cancel">
                    <i class="bi bi-x-circle"></i>
                </button>`;
            break;
            
        case CampaignStatus.Paused:
            buttons += `
                <button class="btn btn-outline-success" onclick="resumeCampaign(${id})" title="Resume">
                    <i class="bi bi-play-fill"></i>
                </button>
                <button class="btn btn-outline-danger" onclick="cancelCampaign(${id})" title="Cancel">
                    <i class="bi bi-x-circle"></i>
                </button>`;
            break;
            
        case CampaignStatus.Completed:
        case CampaignStatus.Failed:
            buttons += `
                <button class="btn btn-outline-danger" onclick="deleteCampaign(${id})" title="Delete">
                    <i class="bi bi-trash"></i>
                </button>`;
            break;
    }
    
    buttons += `</div>`;
    return buttons;
}

/**
 * Setup tab click handlers for status filtering
 */
function setupTabHandlers() {
    $('[data-bs-toggle="tab"]').on('shown.bs.tab', function(e) {
        const target = e.target.getAttribute('href');
        const statusMap = {
            '#all': null,
            '#draft': CampaignStatus.Draft,
            '#scheduled': CampaignStatus.Scheduled,
            '#running': CampaignStatus.Running,
            '#paused': CampaignStatus.Paused,
            '#completed': CampaignStatus.Completed,
            '#failed': CampaignStatus.Failed
        };
        currentStatus = statusMap[target];
        if (campaignsTable) {
            campaignsTable.ajax.reload();
        }
    });
}

// ============================================================================
// CAMPAIGN ACTION FUNCTIONS
// ============================================================================

/**
 * Navigate to campaign details page
 */
function viewCampaign(id) {
    window.location.href = window.AppUrls.campaigns.details ? window.AppUrls.campaigns.details(id) : `/Campaigns/Details/${id}`;
}

/**
 * Duplicate a campaign
 */
function duplicateCampaign(id) {
    confirmAction('Are you sure you want to duplicate this campaign?', function() {
        $.ajax({
            url: window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.duplicate(id)),
            method: 'POST',
            headers: getAjaxHeaders(),
            success: function(response) {
                if (response.success || response.isSuccess) {
                    showNotification('Campaign duplicated successfully!', 'success');
                    campaignsTable.ajax.reload(null, false); // Stay on current page
                } else {
                    showNotification(response.message || 'Failed to duplicate campaign', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to duplicate campaign');
            }
        });
    });
}

/**
 * Start a campaign
 */
function startCampaign(id) {
    confirmAction('Are you sure you want to start this campaign?', function() {
        $.ajax({
            url: window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.start(id)),
            method: 'POST',
            headers: getAjaxHeaders(),
            success: function(response) {
                if (response.success || response.isSuccess) {
                    showNotification('Campaign started successfully!', 'success');
                    campaignsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to start campaign', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to start campaign');
            }
        });
    });
}

/**
 * Pause a running campaign
 */
function pauseCampaign(id) {
    confirmAction('Are you sure you want to pause this campaign?', function() {
        $.ajax({
            url: window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.pause(id)),
            method: 'POST',
            headers: getAjaxHeaders(),
            success: function(response) {
                if (response.success || response.isSuccess) {
                    showNotification('Campaign paused successfully!', 'success');
                    campaignsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to pause campaign', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to pause campaign');
            }
        });
    });
}

/**
 * Resume a paused campaign
 */
function resumeCampaign(id) {
    confirmAction('Are you sure you want to resume this campaign?', function() {
        $.ajax({
            url: window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.resume(id)),
            method: 'POST',
            headers: getAjaxHeaders(),
            success: function(response) {
                if (response.success || response.isSuccess) {
                    showNotification('Campaign resumed successfully!', 'success');
                    campaignsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to resume campaign', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to resume campaign');
            }
        });
    });
}

/**
 * Cancel a scheduled or running campaign
 */
function cancelCampaign(id) {
    confirmAction('Are you sure you want to cancel this campaign? This action cannot be undone.', function() {
        $.ajax({
            url: window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.cancel(id)),
            method: 'POST',
            headers: getAjaxHeaders(),
            success: function(response) {
                if (response.success || response.isSuccess) {
                    showNotification('Campaign cancelled successfully!', 'success');
                    campaignsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to cancel campaign', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to cancel campaign');
            }
        });
    });
}

/**
 * Delete a campaign
 */
function deleteCampaign(id) {
    confirmAction('Are you sure you want to delete this campaign? This action cannot be undone.', function() {
        $.ajax({
            url: window.AppUrls.buildApiUrl(window.AppUrls.api.campaigns.delete(id)),
            method: 'DELETE',
            headers: getAjaxHeaders(),
            success: function(response) {
                if (response.success || response.isSuccess) {
                    showNotification('Campaign deleted successfully!', 'success');
                    campaignsTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete campaign', 'error');
                }
            },
            error: function(xhr) {
                handleAjaxError(xhr, 'Failed to delete campaign');
            }
        });
    });
}

/**
 * Handle AJAX errors consistently
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
        showNotification('Campaign not found', 'error');
    } else {
        const errorMessage = xhr.responseJSON?.message || defaultMessage;
        showNotification(errorMessage, 'error');
    }
}
