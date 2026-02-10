/**
 * Contact Engagement Scoring - SERVER-SIDE API INTEGRATION
 * Dashboard for viewing and managing contact engagement scores
 */

var allEngagementData = [];

$(document).ready(function () {
    loadEngagementData();
});

/**
 * Load engagement data from server
 */
function loadEngagementData() {
    var tbody = document.getElementById('engagementTableBody');
    tbody.innerHTML = '<tr><td colspan="10" class="text-center py-4"><div class="spinner-border spinner-border-sm"></div> Loading engagement data...</td></tr>';

    $.ajax({
        url: '/Contacts/GetEngagementData',
        method: 'GET',
        success: function (response) {
            if (response.success && response.data) {
                allEngagementData = Array.isArray(response.data) ? response.data : [];
                renderEngagementTable(allEngagementData);
                updateSummaryCards(allEngagementData);
            } else {
                allEngagementData = [];
                tbody.innerHTML = '<tr><td colspan="10" class="text-center text-muted py-4"><i class="bi bi-inbox fs-1"></i><br>No engagement data available yet. Send campaigns to generate engagement scores.</td></tr>';
                updateSummaryCards([]);
            }
        },
        error: function () {
            tbody.innerHTML = '<tr><td colspan="10" class="text-center text-danger py-4"><i class="bi bi-exclamation-triangle"></i> Failed to load engagement data</td></tr>';
            updateSummaryCards([]);
        }
    });
}

/**
 * Update summary cards
 */
function updateSummaryCards(data) {
    var high = 0, med = 0, low = 0, none = 0;

    data.forEach(function (c) {
        var score = c.engagementScore || c.EngagementScore || 0;
        if (score >= 80) high++;
        else if (score >= 40) med++;
        else if (score > 0) low++;
        else none++;
    });

    document.getElementById('highEngagement').textContent = high;
    document.getElementById('medEngagement').textContent = med;
    document.getElementById('lowEngagement').textContent = low;
    document.getElementById('noEngagement').textContent = none;
}

/**
 * Render engagement table
 */
function renderEngagementTable(data) {
    var tbody = document.getElementById('engagementTableBody');

    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="10" class="text-center text-muted py-4"><i class="bi bi-inbox fs-1"></i><br>No engagement data available</td></tr>';
        return;
    }

    // Sort by score descending
    data.sort(function (a, b) {
        return (b.engagementScore || b.EngagementScore || 0) - (a.engagementScore || a.EngagementScore || 0);
    });

    var html = '';
    data.forEach(function (c) {
        var contactId = c.contactId || c.ContactId || 0;
        var firstName = c.firstName || c.FirstName || '';
        var lastName = c.lastName || c.LastName || '';
        var email = c.email || c.Email || '';
        var phone = c.phoneNumber || c.PhoneNumber || '';
        var score = c.engagementScore || c.EngagementScore || 0;
        var sent = c.totalMessagesSent || c.TotalMessagesSent || 0;
        var delivered = c.totalMessagesDelivered || c.TotalMessagesDelivered || 0;
        var clicks = c.totalClicks || c.TotalClicks || 0;
        var campaigns = c.campaignsParticipated || c.CampaignsParticipated || 0;
        var lastEngagement = c.lastEngagementDate || c.LastEngagementDate || '';

        // Score badge color
        var scoreBadge = getScoreBadge(score);

        html += '<tr>';
        html += '<td><strong>' + escapeHtml(firstName + ' ' + lastName).trim() + '</strong></td>';
        html += '<td>' + escapeHtml(email || '-') + '</td>';
        html += '<td>' + escapeHtml(phone || '-') + '</td>';
        html += '<td>' + scoreBadge + '</td>';
        html += '<td>' + formatNumber(sent) + '</td>';
        html += '<td>' + formatNumber(delivered) + '</td>';
        html += '<td>' + formatNumber(clicks) + '</td>';
        html += '<td>' + campaigns + '</td>';
        html += '<td>' + (lastEngagement ? formatShortDate(lastEngagement) : '<span class="text-muted">Never</span>') + '</td>';
        html += '<td class="text-end">';
        html += '<button class="btn btn-sm btn-outline-primary" onclick="viewContactDetail(' + contactId + ')" title="View Details">';
        html += '<i class="bi bi-eye"></i></button>';
        html += '</td>';
        html += '</tr>';
    });

    tbody.innerHTML = html;
}

/**
 * Get score badge HTML
 */
function getScoreBadge(score) {
    var color, icon, label;

    if (score >= 80) {
        color = 'bg-success';
        icon = 'emoji-smile';
        label = 'High';
    } else if (score >= 40) {
        color = 'bg-primary';
        icon = 'emoji-neutral';
        label = 'Medium';
    } else if (score > 0) {
        color = 'bg-warning text-dark';
        icon = 'emoji-frown';
        label = 'Low';
    } else {
        color = 'bg-secondary';
        icon = 'emoji-dizzy';
        label = 'None';
    }

    return '<span class="badge ' + color + '">' +
        '<i class="bi bi-' + icon + '"></i> ' + score +
        ' <small>(' + label + ')</small></span>';
}

/**
 * Filter engagement by score category
 */
function filterEngagement() {
    var filter = document.getElementById('engagementFilter').value;

    if (!filter) {
        renderEngagementTable(allEngagementData);
        return;
    }

    var filtered = allEngagementData.filter(function (c) {
        var score = c.engagementScore || c.EngagementScore || 0;
        switch (filter) {
            case 'high': return score >= 80;
            case 'medium': return score >= 40 && score < 80;
            case 'low': return score > 0 && score < 40;
            case 'none': return score === 0;
            default: return true;
        }
    });

    renderEngagementTable(filtered);
}

/**
 * View contact engagement details
 */
function viewContactDetail(contactId) {
    var modalBody = document.getElementById('contactDetailBody');
    var modalTitle = document.getElementById('contactDetailTitle');

    modalBody.innerHTML = '<div class="text-center py-4"><div class="spinner-border"></div><p class="mt-2">Loading details...</p></div>';
    modalTitle.textContent = 'Contact Engagement Details';

    new bootstrap.Modal(document.getElementById('contactDetailModal')).show();

    $.ajax({
        url: '/Contacts/GetContactEngagement?contactId=' + contactId,
        method: 'GET',
        success: function (response) {
            if (response.success && response.data) {
                renderContactDetail(response.data);
            } else {
                modalBody.innerHTML = '<div class="text-center py-4 text-muted">No engagement details available</div>';
            }
        },
        error: function () {
            modalBody.innerHTML = '<div class="text-center py-4 text-danger">Failed to load engagement details</div>';
        }
    });
}

/**
 * Render contact engagement detail modal
 */
function renderContactDetail(data) {
    var modalBody = document.getElementById('contactDetailBody');
    var modalTitle = document.getElementById('contactDetailTitle');

    var firstName = data.firstName || data.FirstName || '';
    var lastName = data.lastName || data.LastName || '';
    modalTitle.textContent = (firstName + ' ' + lastName).trim() + ' - Engagement Details';

    var score = data.engagementScore || data.EngagementScore || 0;
    var sent = data.totalMessagesSent || data.TotalMessagesSent || 0;
    var delivered = data.totalMessagesDelivered || data.TotalMessagesDelivered || 0;
    var clicks = data.totalClicks || data.TotalClicks || 0;
    var campaigns = data.campaignsParticipated || data.CampaignsParticipated || 0;
    var lastEngagement = data.lastEngagementDate || data.LastEngagementDate || '';
    var campaignHistory = data.campaignHistory || data.CampaignHistory || [];
    var events = data.engagementEvents || data.EngagementEvents || [];

    var html = '';

    // Score overview
    html += '<div class="row mb-4">';
    html += '<div class="col-md-3 text-center"><div class="card bg-light"><div class="card-body">';
    html += '<h2>' + getScoreBadge(score) + '</h2><small>Engagement Score</small></div></div></div>';
    html += '<div class="col-md-3 text-center"><div class="card bg-light"><div class="card-body">';
    html += '<h3>' + sent + '</h3><small>Messages Sent</small></div></div></div>';
    html += '<div class="col-md-3 text-center"><div class="card bg-light"><div class="card-body">';
    html += '<h3>' + delivered + '</h3><small>Delivered</small></div></div></div>';
    html += '<div class="col-md-3 text-center"><div class="card bg-light"><div class="card-body">';
    html += '<h3>' + clicks + '</h3><small>Clicks</small></div></div></div>';
    html += '</div>';

    // Campaign participation
    if (campaignHistory.length > 0) {
        html += '<h6><i class="bi bi-megaphone"></i> Campaign Participation (' + campaigns + ')</h6>';
        html += '<div class="table-responsive"><table class="table table-sm">';
        html += '<thead><tr><th>Campaign</th><th>Participated</th><th>Messages</th><th>Clicks</th></tr></thead>';
        html += '<tbody>';
        campaignHistory.forEach(function (ch) {
            html += '<tr>';
            html += '<td>' + escapeHtml(ch.campaignName || ch.CampaignName || '-') + '</td>';
            html += '<td>' + formatShortDate(ch.participatedAt || ch.ParticipatedAt || '') + '</td>';
            html += '<td>' + (ch.messagesReceived || ch.MessagesReceived || 0) + '</td>';
            html += '<td>' + (ch.clicks || ch.Clicks || 0) + '</td>';
            html += '</tr>';
        });
        html += '</tbody></table></div>';
    }

    // Recent engagement events
    if (events.length > 0) {
        html += '<h6 class="mt-3"><i class="bi bi-clock-history"></i> Recent Engagement Events</h6>';
        html += '<div class="list-group" style="max-height: 200px; overflow-y: auto;">';
        events.slice(0, 20).forEach(function (ev) {
            var eventType = ev.eventType || ev.EventType || 'Unknown';
            var eventDate = ev.eventDate || ev.EventDate || '';
            var details = ev.details || ev.Details || '';
            var campaign = ev.campaignName || ev.CampaignName || '';

            var icon = eventType.toLowerCase().includes('click') ? 'cursor' :
                      eventType.toLowerCase().includes('open') ? 'envelope-open' :
                      eventType.toLowerCase().includes('delivered') ? 'check-circle' : 'circle';

            html += '<div class="list-group-item list-group-item-action d-flex justify-content-between">';
            html += '<div><i class="bi bi-' + icon + ' me-2"></i><strong>' + escapeHtml(eventType) + '</strong>';
            if (campaign) html += ' <span class="text-muted">- ' + escapeHtml(campaign) + '</span>';
            if (details) html += '<br><small class="text-muted">' + escapeHtml(details) + '</small>';
            html += '</div>';
            html += '<small class="text-muted">' + formatShortDate(eventDate) + '</small>';
            html += '</div>';
        });
        html += '</div>';
    }

    if (campaignHistory.length === 0 && events.length === 0) {
        html += '<div class="text-center py-3 text-muted"><i class="bi bi-inbox fs-1"></i><br>No campaign history or engagement events recorded</div>';
    }

    modalBody.innerHTML = html;
}
