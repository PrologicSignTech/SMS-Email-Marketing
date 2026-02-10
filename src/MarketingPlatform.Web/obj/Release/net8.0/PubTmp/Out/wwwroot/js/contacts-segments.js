/**
 * Contacts Dynamic Segments - SERVER-SIDE API INTEGRATION
 * Segment builder with rule-based audience targeting
 */

var segmentRules = [];
var ruleCounter = 0;

// Available fields for segment rules
var segmentFields = [
    { value: 'Country', label: 'Country', type: 'text' },
    { value: 'City', label: 'City', type: 'text' },
    { value: 'PostalCode', label: 'Postal Code', type: 'text' },
    { value: 'Email', label: 'Email', type: 'text' },
    { value: 'PhoneNumber', label: 'Phone Number', type: 'text' },
    { value: 'FirstName', label: 'First Name', type: 'text' },
    { value: 'LastName', label: 'Last Name', type: 'text' },
    { value: 'IsActive', label: 'Active Status', type: 'boolean' },
    { value: 'CreatedAt', label: 'Created Date', type: 'date' }
];

// Operators by type
var operatorsByType = {
    text: [
        { value: 'Equals', label: 'equals' },
        { value: 'NotEquals', label: 'not equals' },
        { value: 'Contains', label: 'contains' },
        { value: 'StartsWith', label: 'starts with' },
        { value: 'EndsWith', label: 'ends with' },
        { value: 'IsEmpty', label: 'is empty' },
        { value: 'IsNotEmpty', label: 'is not empty' }
    ],
    boolean: [
        { value: 'Equals', label: 'equals' }
    ],
    date: [
        { value: 'Equals', label: 'equals' },
        { value: 'GreaterThan', label: 'after' },
        { value: 'LessThan', label: 'before' }
    ]
};

$(document).ready(function () {
    addRule(); // Start with one rule
    loadGroups();
});

/**
 * Add a new rule to the builder
 */
function addRule() {
    ruleCounter++;
    var ruleId = 'rule_' + ruleCounter;

    var fieldOptions = segmentFields.map(function (f) {
        return '<option value="' + f.value + '" data-type="' + f.type + '">' + f.label + '</option>';
    }).join('');

    var operatorOptions = operatorsByType.text.map(function (o) {
        return '<option value="' + o.value + '">' + o.label + '</option>';
    }).join('');

    var html = '<div class="rule-row border rounded p-3 mb-2 bg-light" id="' + ruleId + '">';
    html += '<div class="row align-items-center">';
    html += '<div class="col-md-3">';
    html += '<select class="form-select form-select-sm rule-field" data-rule-id="' + ruleId + '" onchange="updateOperators(this)">';
    html += fieldOptions;
    html += '</select>';
    html += '</div>';
    html += '<div class="col-md-3">';
    html += '<select class="form-select form-select-sm rule-operator" data-rule-id="' + ruleId + '">';
    html += operatorOptions;
    html += '</select>';
    html += '</div>';
    html += '<div class="col-md-4">';
    html += '<input type="text" class="form-control form-control-sm rule-value" data-rule-id="' + ruleId + '" placeholder="Value">';
    html += '</div>';
    html += '<div class="col-md-2 text-end">';
    html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="removeRule(\'' + ruleId + '\')">';
    html += '<i class="bi bi-x-lg"></i></button>';
    html += '</div>';
    html += '</div></div>';

    document.getElementById('rulesContainer').insertAdjacentHTML('beforeend', html);
}

/**
 * Remove a rule
 */
function removeRule(ruleId) {
    var el = document.getElementById(ruleId);
    if (el) el.remove();

    // Ensure at least one rule
    if (document.querySelectorAll('.rule-row').length === 0) {
        addRule();
    }
}

/**
 * Update operators when field changes
 */
function updateOperators(selectEl) {
    var selectedOption = selectEl.options[selectEl.selectedIndex];
    var fieldType = selectedOption.getAttribute('data-type') || 'text';
    var ruleId = selectEl.getAttribute('data-rule-id');
    var operatorSelect = document.querySelector('.rule-operator[data-rule-id="' + ruleId + '"]');
    var valueInput = document.querySelector('.rule-value[data-rule-id="' + ruleId + '"]');

    // Update operators
    var operators = operatorsByType[fieldType] || operatorsByType.text;
    operatorSelect.innerHTML = operators.map(function (o) {
        return '<option value="' + o.value + '">' + o.label + '</option>';
    }).join('');

    // Update value input type
    if (fieldType === 'boolean') {
        valueInput.outerHTML = '<select class="form-select form-select-sm rule-value" data-rule-id="' + ruleId + '">' +
            '<option value="true">Yes / Active</option>' +
            '<option value="false">No / Inactive</option></select>';
    } else if (fieldType === 'date') {
        valueInput.outerHTML = '<input type="date" class="form-control form-control-sm rule-value" data-rule-id="' + ruleId + '">';
    } else {
        if (valueInput.tagName === 'SELECT') {
            valueInput.outerHTML = '<input type="text" class="form-control form-control-sm rule-value" data-rule-id="' + ruleId + '" placeholder="Value">';
        }
    }
}

/**
 * Gather rules from the UI
 */
function gatherRules() {
    var rules = [];
    document.querySelectorAll('.rule-row').forEach(function (row) {
        var ruleId = row.id;
        var field = row.querySelector('.rule-field').value;
        var operator = row.querySelector('.rule-operator').value;
        var value = row.querySelector('.rule-value').value;

        if (field && operator) {
            rules.push({
                field: field,
                operator: operator,
                value: value || ''
            });
        }
    });
    return rules;
}

/**
 * Get criteria object
 */
function getCriteria() {
    var logicalOperator = document.querySelector('input[name="logicalOperator"]:checked').value;
    return {
        rules: gatherRules(),
        logicalOperator: logicalOperator
    };
}

/**
 * Calculate audience size
 */
function calculateSize() {
    var criteria = getCriteria();
    if (criteria.rules.length === 0) {
        showNotification('Please add at least one rule', 'warning');
        return;
    }

    $.ajax({
        url: '/Contacts/CalculateAudienceSize',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(criteria),
        success: function (response) {
            var size = 0;
            if (response.success) {
                size = response.data || 0;
            }
            document.getElementById('audienceSize').textContent = size + ' contacts';
            showNotification('Audience size: ' + size + ' contacts', 'info');
        },
        error: function () {
            showNotification('Failed to calculate audience size', 'error');
        }
    });
}

/**
 * Evaluate segment and show preview
 */
function evaluateSegment() {
    var criteria = getCriteria();
    if (criteria.rules.length === 0) {
        showNotification('Please add at least one rule', 'warning');
        return;
    }

    $.ajax({
        url: '/Contacts/EvaluateSegment',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(criteria),
        success: function (response) {
            if (response.success && response.data) {
                var data = response.data;
                var totalContacts = data.totalContacts || data.TotalContacts || 0;
                var contactIds = data.contactIds || data.ContactIds || [];

                document.getElementById('audienceSize').textContent = totalContacts + ' contacts';
                document.getElementById('previewCount').textContent = 'Showing ' + Math.min(contactIds.length, 50) + ' of ' + totalContacts + ' matching contacts';

                // Load actual contact details for the preview
                if (contactIds.length > 0) {
                    loadPreviewContacts(contactIds.slice(0, 50));
                } else {
                    document.getElementById('previewTableBody').innerHTML = '<tr><td colspan="5" class="text-center text-muted py-3">No matching contacts found</td></tr>';
                }

                document.getElementById('previewCard').classList.remove('d-none');
            } else {
                showNotification(response.message || 'No contacts match the criteria', 'info');
            }
        },
        error: function () {
            showNotification('Failed to evaluate segment', 'error');
        }
    });
}

/**
 * Load preview contacts by searching
 */
function loadPreviewContacts(contactIds) {
    // Use the contacts API to get details - we'll display what we have from IDs
    var tbody = document.getElementById('previewTableBody');
    tbody.innerHTML = '<tr><td colspan="5" class="text-center"><div class="spinner-border spinner-border-sm"></div> Loading...</td></tr>';

    // For preview, fetch a small page of contacts and match
    $.ajax({
        url: '/Contacts/GetContacts',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ draw: 1, start: 0, length: 100, search: { value: '' } }),
        success: function (response) {
            var allContacts = response.data || [];
            if (!Array.isArray(allContacts)) {
                // Try to convert JsonElement array
                try {
                    allContacts = JSON.parse(JSON.stringify(allContacts));
                } catch (e) { allContacts = []; }
            }

            // Filter to matching IDs
            var matched = allContacts.filter(function (c) {
                var cId = c.id || c.Id;
                return contactIds.includes(cId);
            });

            if (matched.length === 0) {
                // Just show the IDs if we can't resolve them
                var html = '';
                contactIds.forEach(function (id) {
                    html += '<tr><td>Contact #' + id + '</td><td colspan="4">-</td></tr>';
                });
                tbody.innerHTML = html;
                return;
            }

            var html = '';
            matched.forEach(function (c) {
                html += '<tr>';
                html += '<td>' + escapeHtml((c.firstName || '') + ' ' + (c.lastName || '')).trim() + '</td>';
                html += '<td>' + escapeHtml(c.email || '-') + '</td>';
                html += '<td>' + escapeHtml(c.phoneNumber || '-') + '</td>';
                html += '<td>' + escapeHtml(c.country || '-') + '</td>';
                html += '<td>' + escapeHtml(c.city || '-') + '</td>';
                html += '</tr>';
            });
            tbody.innerHTML = html;
        },
        error: function () {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Failed to load preview</td></tr>';
        }
    });
}

/**
 * Save segment as dynamic group
 */
function saveAsGroup() {
    var criteria = getCriteria();
    if (criteria.rules.length === 0) {
        showNotification('Please add at least one rule', 'warning');
        return;
    }
    new bootstrap.Modal(document.getElementById('saveGroupModal')).show();
}

/**
 * Confirm save group
 */
function confirmSaveGroup() {
    var name = document.getElementById('groupName').value.trim();
    var description = document.getElementById('groupDescription').value.trim();

    if (!name) {
        showNotification('Please enter a group name', 'error');
        return;
    }

    var criteria = getCriteria();
    var groupData = {
        name: name,
        description: description,
        isDynamic: true,
        segmentCriteria: JSON.stringify(criteria)
    };

    $.ajax({
        url: '/Contacts/CreateGroup',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(groupData),
        success: function (response) {
            if (response.success) {
                showNotification('Dynamic group "' + name + '" created!', 'success');
                bootstrap.Modal.getInstance(document.getElementById('saveGroupModal')).hide();
                document.getElementById('groupName').value = '';
                document.getElementById('groupDescription').value = '';
                loadGroups();
            } else {
                showNotification(response.message || 'Failed to create group', 'error');
            }
        },
        error: function () {
            showNotification('An error occurred', 'error');
        }
    });
}

/**
 * Load existing groups
 */
function loadGroups() {
    var container = document.getElementById('groupsList');
    container.innerHTML = '<div class="text-center py-3"><div class="spinner-border spinner-border-sm"></div> Loading groups...</div>';

    $.get('/Contacts/GetContactGroups', function (response) {
        var items = [];
        if (response.success && response.items) {
            items = Array.isArray(response.items) ? response.items : [];
        }

        if (items.length === 0) {
            container.innerHTML = '<div class="text-center py-4 text-muted"><i class="bi bi-inbox fs-1"></i><br>No groups found. Create a segment above and save it as a group.</div>';
            return;
        }

        var html = '<div class="table-responsive"><table class="table table-hover table-sm">';
        html += '<thead><tr><th>Name</th><th>Description</th><th>Contacts</th><th>Type</th><th class="text-end">Actions</th></tr></thead>';
        html += '<tbody>';

        items.forEach(function (g) {
            var isDynamic = g.isDynamic || g.IsDynamic || false;
            var typeBadge = isDynamic
                ? '<span class="badge bg-info"><i class="bi bi-lightning"></i> Dynamic</span>'
                : '<span class="badge bg-secondary">Static</span>';

            html += '<tr>';
            html += '<td><strong>' + escapeHtml(g.name || g.Name || '') + '</strong></td>';
            html += '<td>' + escapeHtml(g.description || g.Description || '-') + '</td>';
            html += '<td>' + (g.contactCount || g.ContactCount || 0) + '</td>';
            html += '<td>' + typeBadge + '</td>';
            html += '<td class="text-end">';
            if (isDynamic) {
                html += '<button class="btn btn-sm btn-outline-info me-1" onclick="refreshGroup(' + (g.id || g.Id) + ')" title="Refresh"><i class="bi bi-arrow-clockwise"></i></button>';
            }
            html += '<button class="btn btn-sm btn-outline-danger" onclick="deleteGroup(' + (g.id || g.Id) + ', \'' + escapeHtml(g.name || g.Name || '') + '\')" title="Delete"><i class="bi bi-trash"></i></button>';
            html += '</td>';
            html += '</tr>';
        });

        html += '</tbody></table></div>';
        container.innerHTML = html;
    }).fail(function () {
        container.innerHTML = '<div class="text-center py-3 text-danger">Failed to load groups</div>';
    });
}

/**
 * Refresh a dynamic group
 */
function refreshGroup(groupId) {
    $.ajax({
        url: '/Contacts/RefreshGroup?groupId=' + groupId,
        method: 'POST',
        success: function (response) {
            if (response.success) {
                showNotification('Group refreshed!', 'success');
                loadGroups();
            } else {
                showNotification(response.message || 'Failed to refresh group', 'error');
            }
        },
        error: function () {
            showNotification('An error occurred', 'error');
        }
    });
}

/**
 * Delete a group
 */
function deleteGroup(groupId, name) {
    confirmAction('Are you sure you want to delete the group "' + name + '"?', function () {
        $.ajax({
            url: '/Contacts/DeleteGroup?id=' + groupId,
            method: 'POST',
            success: function (response) {
                if (response.success) {
                    showNotification('Group deleted', 'success');
                    loadGroups();
                } else {
                    showNotification(response.message || 'Failed to delete group', 'error');
                }
            },
            error: function () {
                showNotification('An error occurred', 'error');
            }
        });
    });
}

/**
 * Show create segment modal (just scrolls to builder)
 */
function showCreateSegmentModal() {
    document.getElementById('segmentBuilderCard').scrollIntoView({ behavior: 'smooth' });
    // Reset rules
    document.getElementById('rulesContainer').innerHTML = '';
    ruleCounter = 0;
    addRule();
    document.getElementById('audienceSize').textContent = '0 contacts';
}
