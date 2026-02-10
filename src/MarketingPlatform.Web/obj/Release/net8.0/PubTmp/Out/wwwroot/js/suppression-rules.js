/**
 * Suppression Rules Index Page - DataTables Implementation
 * SERVER-SIDE API INTEGRATION
 */

let rulesTable;

const triggerNames = ['Unsubscribe', 'Hard Bounce', 'Soft Bounce', 'Spam Complaint', 'SMS Opt-Out', 'WhatsApp Opt-Out', 'Invalid Email', 'Invalid Phone', 'Manual Upload', 'Inactivity'];
const triggerIcons = ['bi-link-45deg', 'bi-exclamation-triangle', 'bi-arrow-repeat', 'bi-flag', 'bi-phone-vibrate', 'bi-whatsapp', 'bi-envelope-x', 'bi-phone-x', 'bi-upload', 'bi-clock-history'];
const scopeNames = ['Global', 'Channel Specific'];
const channelNames = ['All', 'Email', 'SMS', 'MMS', 'WhatsApp'];
const channelColors = { 0: 'secondary', 1: 'primary', 2: 'success', 3: 'info', 4: 'success' };
const suppressionTypeNames = ['Opt-Out', 'Bounce', 'Complaint', 'Manual'];
const suppressionTypeColors = { 0: 'danger', 1: 'warning', 2: 'danger', 3: 'info' };

$(document).ready(function () {
    initRulesTable();
});

function initRulesTable() {
    rulesTable = $('#rulesTable').DataTable({
        serverSide: true,
        processing: true,
        ajax: {
            url: '/SuppressionRules/GetRules',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                return JSON.stringify({ draw: d.draw, start: d.start, length: d.length, search: d.search });
            },
            dataSrc: function (json) { return json.data || []; }
        },
        columns: [
            {
                data: 'name', render: function (data, type, row) {
                    if (type !== 'display') return data;
                    var icon = row.isSystemRule ? '<i class="bi bi-lock-fill text-muted me-1" title="System Rule"></i>' : '';
                    return icon + '<strong>' + escapeHtml(data) + '</strong>' +
                        (row.description ? '<br><small class="text-muted">' + escapeHtml(row.description.substring(0, 60)) + '</small>' : '');
                }
            },
            {
                data: 'trigger', render: function (data) {
                    var name = triggerNames[data] || 'Unknown';
                    var icon = triggerIcons[data] || 'bi-question';
                    return '<i class="bi ' + icon + ' me-1"></i>' + name;
                }
            },
            {
                data: 'scope', render: function (data) {
                    return createBadge(scopeNames[data] || 'Unknown', data === 0 ? 'dark' : 'outline-primary');
                }
            },
            {
                data: 'channel', render: function (data) {
                    return createBadge(channelNames[data] || 'Unknown', channelColors[data] || 'secondary');
                }
            },
            {
                data: 'suppressionType', render: function (data) {
                    return createBadge(suppressionTypeNames[data] || 'Unknown', suppressionTypeColors[data] || 'secondary');
                }
            },
            {
                data: 'isActive', render: function (data, type, row) {
                    var color = data ? 'success' : 'secondary';
                    var label = data ? 'Active' : 'Inactive';
                    return '<span class="badge bg-' + color + ' cursor-pointer" onclick="toggleRule(' + row.id + ')" style="cursor:pointer" title="Click to toggle">' + label + '</span>';
                }
            },
            {
                data: 'triggerCount', render: function (data, type, row) {
                    var text = data > 0 ? data.toLocaleString() + ' times' : 'Never';
                    if (row.lastTriggeredAt) {
                        text += '<br><small class="text-muted">' + formatShortDate(row.lastTriggeredAt) + '</small>';
                    }
                    return text;
                }
            },
            {
                data: 'id', orderable: false, className: 'text-end no-export',
                render: function (data, type, row) {
                    var btns = '<div class="btn-group btn-group-sm">';
                    btns += '<button class="btn btn-outline-primary" onclick="editRule(' + data + ')" title="Edit"><i class="bi bi-pencil"></i></button>';
                    if (!row.isSystemRule) {
                        btns += '<button class="btn btn-outline-danger" onclick="deleteRule(' + data + ')" title="Delete"><i class="bi bi-trash"></i></button>';
                    }
                    btns += '</div>';
                    return btns;
                }
            }
        ],
        order: [[5, 'desc']],
        pageLength: 50,
        responsive: true,
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>><"row"<"col-sm-12"tr>><"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        language: {
            processing: '<div class="spinner-border text-primary"></div>',
            emptyTable: '<div class="text-center py-4"><i class="bi bi-gear" style="font-size:2rem"></i><p class="mt-2">No suppression rules configured.<br>Click "Load Defaults" to set up recommended rules.</p></div>'
        }
    });
}

function createRule() {
    var name = document.getElementById('ruleName').value.trim();
    var trigger = document.getElementById('ruleTrigger').value;

    if (!name || trigger === '') {
        showNotification('Name and trigger are required', 'error');
        return;
    }

    fetch('/SuppressionRules/CreateRule', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            name: name,
            description: document.getElementById('ruleDescription').value.trim() || null,
            trigger: parseInt(trigger),
            scope: parseInt(document.getElementById('ruleScope').value),
            channel: parseInt(document.getElementById('ruleChannel').value),
            suppressionType: parseInt(document.getElementById('ruleSuppressionType').value),
            priority: parseInt(document.getElementById('rulePriority').value) || 0,
            autoReason: document.getElementById('ruleAutoReason').value.trim() || null
        })
    })
        .then(r => r.json())
        .then(result => {
            if (result.success) {
                showNotification('Rule created successfully!', 'success');
                bootstrap.Modal.getInstance(document.getElementById('createRuleModal')).hide();
                document.getElementById('createRuleForm').reset();
                rulesTable.ajax.reload(null, false);
            } else {
                showNotification(result.message || 'Failed to create rule', 'error');
            }
        })
        .catch(() => showNotification('An error occurred', 'error'));
}

function editRule(id) {
    // Find row data from DataTable
    var data = rulesTable.rows().data().toArray().find(r => r.id === id);
    if (!data) return;

    document.getElementById('editRuleId').value = id;
    document.getElementById('editRuleName').value = data.name || '';
    document.getElementById('editRuleDescription').value = data.description || '';
    document.getElementById('editRuleScope').value = data.scope;
    document.getElementById('editRuleChannel').value = data.channel;
    document.getElementById('editRuleSuppressionType').value = data.suppressionType;
    document.getElementById('editRulePriority').value = data.priority;
    document.getElementById('editRuleAutoReason').value = data.autoReason || '';
    document.getElementById('editRuleActive').checked = data.isActive;

    new bootstrap.Modal(document.getElementById('editRuleModal')).show();
}

function saveEditRule() {
    var id = document.getElementById('editRuleId').value;

    fetch('/SuppressionRules/UpdateRule?id=' + id, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            name: document.getElementById('editRuleName').value.trim(),
            description: document.getElementById('editRuleDescription').value.trim() || null,
            scope: parseInt(document.getElementById('editRuleScope').value),
            channel: parseInt(document.getElementById('editRuleChannel').value),
            suppressionType: parseInt(document.getElementById('editRuleSuppressionType').value),
            isActive: document.getElementById('editRuleActive').checked,
            priority: parseInt(document.getElementById('editRulePriority').value) || 0,
            autoReason: document.getElementById('editRuleAutoReason').value.trim() || null
        })
    })
        .then(r => r.json())
        .then(result => {
            if (result.success) {
                showNotification('Rule updated!', 'success');
                bootstrap.Modal.getInstance(document.getElementById('editRuleModal')).hide();
                rulesTable.ajax.reload(null, false);
            } else {
                showNotification(result.message || 'Failed to update rule', 'error');
            }
        })
        .catch(() => showNotification('An error occurred', 'error'));
}

function toggleRule(id) {
    fetch('/SuppressionRules/ToggleRule?id=' + id, { method: 'POST' })
        .then(r => r.json())
        .then(result => {
            if (result.success) {
                showNotification('Rule toggled!', 'success');
                rulesTable.ajax.reload(null, false);
            } else {
                showNotification(result.message || 'Failed to toggle rule', 'error');
            }
        })
        .catch(() => showNotification('An error occurred', 'error'));
}

function deleteRule(id) {
    confirmAction('Are you sure you want to delete this rule?', function () {
        fetch('/SuppressionRules/DeleteRule?id=' + id, { method: 'POST' })
            .then(r => r.json())
            .then(result => {
                if (result.success) {
                    showNotification('Rule deleted!', 'success');
                    rulesTable.ajax.reload(null, false);
                } else {
                    showNotification(result.message || 'Failed to delete rule', 'error');
                }
            })
            .catch(() => showNotification('An error occurred', 'error'));
    });
}

function seedDefaults() {
    confirmAction('This will create default auto-suppression rules. Continue?', function () {
        fetch('/SuppressionRules/SeedDefaults', { method: 'POST' })
            .then(r => r.json())
            .then(result => {
                if (result.success) {
                    showNotification('Default rules loaded successfully!', 'success');
                    rulesTable.ajax.reload(null, false);
                } else {
                    showNotification(result.message || 'Failed to load defaults', 'error');
                }
            })
            .catch(() => showNotification('An error occurred', 'error'));
    });
}
