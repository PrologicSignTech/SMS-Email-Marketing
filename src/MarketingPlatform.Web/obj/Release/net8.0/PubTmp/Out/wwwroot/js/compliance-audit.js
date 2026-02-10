/**
 * Compliance Audit Log - SERVER-SIDE API INTEGRATION
 */

var auditTable;

$(document).ready(function () {
    initAuditTable();
});

/**
 * Initialize audit log DataTable
 */
function initAuditTable() {
    auditTable = initDataTable('#auditTable', {
        ajax: {
            url: '/Compliance/GetAuditLogs',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        columns: [
            {
                data: null,
                render: function (data) {
                    var date = data.actionDate || data.ActionDate || '';
                    return formatDate(date);
                }
            },
            {
                data: null,
                render: function (data) {
                    var actionType = data.actionType || data.ActionType || 'Unknown';
                    var actionStr = typeof actionType === 'number' ? getActionTypeName(actionType) : actionType;
                    var color = getActionBadgeColor(actionStr);
                    return '<span class="badge ' + color + '">' + escapeHtml(actionStr) + '</span>';
                }
            },
            {
                data: null,
                render: function (data) {
                    var channel = data.channel || data.Channel;
                    if (channel === undefined || channel === null) return '<span class="text-muted">-</span>';
                    var channelStr = typeof channel === 'number' ? getChannelName(channel) : channel;
                    return '<span class="badge bg-outline-secondary border">' + escapeHtml(channelStr) + '</span>';
                }
            },
            {
                data: null,
                render: function (data) {
                    var contactId = data.contactId || data.ContactId;
                    if (!contactId) return '<span class="text-muted">-</span>';
                    return '<a href="/Contacts/Details/' + contactId + '" class="text-decoration-none">#' + contactId + '</a>';
                }
            },
            {
                data: null,
                render: function (data) {
                    var campaignId = data.campaignId || data.CampaignId;
                    if (!campaignId) return '<span class="text-muted">-</span>';
                    return '<a href="/Campaigns/Details/' + campaignId + '" class="text-decoration-none">#' + campaignId + '</a>';
                }
            },
            {
                data: null,
                render: function (data) {
                    var details = data.details || data.Details || data.description || data.Description || '';
                    if (!details) return '<span class="text-muted">-</span>';
                    var truncated = details.length > 60 ? details.substring(0, 60) + '...' : details;
                    return '<span title="' + escapeHtml(details) + '">' + escapeHtml(truncated) + '</span>';
                }
            },
            {
                data: null,
                render: function (data) {
                    var ip = data.ipAddress || data.IpAddress || '';
                    if (!ip) return '<span class="text-muted">-</span>';
                    return '<code class="small">' + escapeHtml(ip) + '</code>';
                }
            }
        ],
        order: [[0, 'desc']],
        pageLength: 50
    });
}

/**
 * Map action type number to name
 */
function getActionTypeName(type) {
    var names = {
        0: 'ConsentGranted',
        1: 'ConsentRevoked',
        2: 'Suppressed',
        3: 'Unsuppressed',
        4: 'OptIn',
        5: 'OptOut',
        6: 'ComplianceCheck',
        7: 'SettingsUpdated',
        8: 'RuleCreated',
        9: 'RuleUpdated',
        10: 'RuleDeleted'
    };
    return names[type] || 'Action ' + type;
}

/**
 * Get badge color for action type
 */
function getActionBadgeColor(action) {
    if (!action) return 'bg-secondary';
    var a = action.toLowerCase();
    if (a.includes('granted') || a.includes('optin') || a.includes('opt-in')) return 'bg-success';
    if (a.includes('revoked') || a.includes('optout') || a.includes('opt-out')) return 'bg-danger';
    if (a.includes('suppressed')) return 'bg-warning text-dark';
    if (a.includes('check')) return 'bg-info';
    if (a.includes('settings') || a.includes('rule')) return 'bg-primary';
    return 'bg-secondary';
}

/**
 * Map channel number to name
 */
function getChannelName(channel) {
    var names = { 0: 'SMS', 1: 'MMS', 2: 'Email', 3: 'Voice', 4: 'Push' };
    return names[channel] || 'Channel ' + channel;
}
