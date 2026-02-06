/**
 * Journeys Index Page - DataTables Implementation
 * SERVER-SIDE API INTEGRATION
 */

let journeysTable;

$(document).ready(function() {
    initJourneysTable();
});

function initJourneysTable() {
    journeysTable = $('#journeysTable').DataTable({
        serverSide: true,
        processing: true,

        ajax: {
            url: '/Journeys/GetJourneys',
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    order: d.order
                });
            },
            dataSrc: function(json) {
                return json.data || [];
            },
            error: function(xhr, error, code) {
                console.error('DataTables error:', error, code);
                handleAjaxError(xhr, 'Failed to load journeys');
            }
        },

        columns: [
            {
                data: 'name',
                render: function(data, type, row) {
                    if (type === 'display') {
                        return `<strong>${escapeHtml(data)}</strong>`;
                    }
                    return data;
                }
            },
            {
                data: 'description',
                render: function(data) {
                    return escapeHtml(data || 'N/A');
                }
            },
            {
                data: 'status',
                render: function(data) {
                    const statusColors = { 0: 'secondary', 1: 'success', 2: 'warning' };
                    const statusNames = ['Draft', 'Active', 'Paused'];
                    return createBadge(statusNames[data] || 'Unknown', statusColors[data] || 'secondary');
                }
            },
            {
                data: 'stepsCount',
                render: function(data) {
                    return data || 0;
                }
            },
            {
                data: 'subscribersCount',
                render: function(data) {
                    return formatNumber(data || 0);
                }
            },
            {
                data: 'updatedAt',
                render: function(data) {
                    return formatShortDate(data);
                }
            },
            {
                data: 'id',
                orderable: false,
                searchable: false,
                className: 'text-end',
                render: function(data, type, row) {
                    let html = '<div class="btn-group btn-group-sm">';

                    if (row.status === 0) { // Draft
                        html += `<button class="btn btn-outline-success" onclick="activateJourney(${data})" title="Activate">
                                    <i class="bi bi-play-circle"></i>
                                 </button>`;
                    } else if (row.status === 1) { // Active
                        html += `<button class="btn btn-outline-warning" onclick="pauseJourney(${data})" title="Pause">
                                    <i class="bi bi-pause-circle"></i>
                                 </button>`;
                    } else if (row.status === 2) { // Paused
                        html += `<button class="btn btn-outline-success" onclick="activateJourney(${data})" title="Resume">
                                    <i class="bi bi-play-circle"></i>
                                 </button>`;
                    }

                    html += `<a href="/Journeys/Edit/${data}" class="btn btn-outline-primary" title="Edit">
                                <i class="bi bi-pencil"></i>
                             </a>
                             <a href="/Journeys/Details/${data}" class="btn btn-outline-info" title="Details">
                                <i class="bi bi-eye"></i>
                             </a>
                             <button class="btn btn-outline-danger" onclick="deleteJourney(${data})" title="Delete">
                                <i class="bi bi-trash"></i>
                             </button>
                          </div>`;

                    return html;
                }
            }
        ],

        order: [[5, 'desc']],
        pageLength: 25,
        language: {
            processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>',
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No journeys found</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching journeys found</div>'
        }
    });
}

function activateJourney(id) {
    confirmAction('Are you sure you want to activate this journey?', function() {
        $.ajax({
            url: `/Journeys/Activate?id=${id}`,
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Journey activated successfully!', 'success');
                    journeysTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to activate journey', 'error');
                }
            },
            error: function() {
                showNotification('An error occurred', 'error');
            }
        });
    });
}

function pauseJourney(id) {
    confirmAction('Are you sure you want to pause this journey?', function() {
        $.ajax({
            url: `/Journeys/Pause?id=${id}`,
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Journey paused successfully!', 'success');
                    journeysTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to pause journey', 'error');
                }
            },
            error: function() {
                showNotification('An error occurred', 'error');
            }
        });
    });
}

function deleteJourney(id) {
    confirmAction('Are you sure you want to delete this journey? This action cannot be undone.', function() {
        $.ajax({
            url: `/Journeys/Delete?id=${id}`,
            method: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Journey deleted successfully!', 'success');
                    journeysTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete journey', 'error');
                }
            },
            error: function() {
                showNotification('An error occurred', 'error');
            }
        });
    });
}
