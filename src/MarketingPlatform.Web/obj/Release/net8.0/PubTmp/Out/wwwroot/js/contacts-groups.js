/**
 * Contact Groups Management
 * Loads groups from API and handles CRUD operations via Web controller proxy
 */

let allGroups = [];

document.addEventListener('DOMContentLoaded', function () {
    loadGroups();

    // Create group button handler
    const createBtn = document.querySelector('[data-action="create-group"]');
    if (createBtn) {
        createBtn.addEventListener('click', showCreateGroupModal);
    }
});

async function loadGroups() {
    const listContainer = document.getElementById('groups-list');
    listContainer.innerHTML = '<div class="text-center py-5"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>';

    try {
        const response = await $.ajax({
            url: '/Contacts/GetContactGroups',
            method: 'GET',
            headers: typeof getAjaxHeaders === 'function' ? getAjaxHeaders() : {}
        });

        let groups = [];
        if (response?.success && response.items) {
            // Handle paginated (items.items) or flat array
            if (Array.isArray(response.items)) {
                groups = response.items;
            } else if (response.items.items && Array.isArray(response.items.items)) {
                groups = response.items.items;
            } else {
                // Try to parse JsonElement array
                groups = response.items;
            }
        }

        allGroups = groups;
        renderGroups(groups);
    } catch (err) {
        console.error('Failed to load groups:', err);
        listContainer.innerHTML = '<div class="text-center py-5 text-muted"><i class="bi bi-exclamation-triangle fs-1"></i><p class="mt-2">Failed to load groups. Please try again.</p><button class="btn btn-primary btn-sm" onclick="loadGroups()">Retry</button></div>';
    }
}

function renderGroups(groups) {
    const listContainer = document.getElementById('groups-list');

    if (!groups || groups.length === 0) {
        listContainer.innerHTML = '<div class="text-center py-5 text-muted"><i class="bi bi-collection fs-1"></i><p class="mt-2">No contact groups yet. Create your first group!</p></div>';
        return;
    }

    let html = '<div class="row">';
    groups.forEach(group => {
        const name = group.name || 'Unnamed Group';
        const desc = group.description || 'No description';
        const count = (group.contactCount || 0).toLocaleString();
        const isStatic = group.isStatic !== false;
        const typeBadge = isStatic
            ? '<span class="badge bg-info ms-2">Static</span>'
            : '<span class="badge bg-warning ms-2">Dynamic</span>';

        html += `
            <div class="col-md-6 col-lg-4 mb-3" id="group-card-${group.id}">
                <div class="card h-100 shadow-sm">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start">
                            <div class="flex-grow-1">
                                <h5 class="card-title fw-bold">${escapeHtml(name)}</h5>
                                <p class="card-text text-muted small">${escapeHtml(desc)}</p>
                                <p class="mb-0">
                                    <i class="bi bi-people-fill me-1 text-primary"></i>
                                    <strong>${count}</strong> contacts
                                    ${typeBadge}
                                </p>
                                <small class="text-muted">${group.createdAt ? 'Created: ' + formatDate(group.createdAt) : ''}</small>
                            </div>
                            <div class="btn-group-vertical btn-group-sm ms-2">
                                <button class="btn btn-outline-primary btn-sm" data-action="import" data-group-id="${group.id}" title="Import CSV">
                                    <i class="bi bi-upload"></i>
                                </button>
                                <button class="btn btn-outline-info btn-sm" data-action="export" data-group-id="${group.id}" title="Export Contacts">
                                    <i class="bi bi-download"></i>
                                </button>
                                <button class="btn btn-outline-success btn-sm" data-action="edit" data-group-id="${group.id}" title="Edit">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="btn btn-outline-danger btn-sm" data-action="delete" data-group-id="${group.id}" title="Delete">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });
    html += '</div>';

    listContainer.innerHTML = html;

    // Attach event listeners via delegation
    listContainer.onclick = function (e) {
        const button = e.target.closest('[data-action]');
        if (!button) return;

        const action = button.dataset.action;
        const groupId = parseInt(button.dataset.groupId);

        if (action === 'edit') {
            showEditGroupModal(groupId);
        } else if (action === 'delete') {
            deleteGroup(groupId);
        } else if (action === 'import') {
            showGroupImportModalForGroup(groupId);
        } else if (action === 'export') {
            exportGroupContacts(groupId);
        }
    };
}

// ========== CREATE GROUP ==========
function showCreateGroupModal() {
    // Remove existing modal if any
    const existingModal = document.getElementById('groupModal');
    if (existingModal) existingModal.remove();

    const modalHtml = `
    <div class="modal fade" id="groupModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-plus-circle me-2"></i>Create Contact Group</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <form id="groupForm">
                        <div class="mb-3">
                            <label for="groupName" class="form-label">Group Name <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="groupName" required>
                        </div>
                        <div class="mb-3">
                            <label for="groupDescription" class="form-label">Description</label>
                            <textarea class="form-control" id="groupDescription" rows="3"></textarea>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Group Type</label>
                            <div>
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="groupType" id="groupTypeStatic" value="static" checked onchange="toggleRuleCriteria()">
                                    <label class="form-check-label" for="groupTypeStatic">Static</label>
                                </div>
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="groupType" id="groupTypeDynamic" value="dynamic" onchange="toggleRuleCriteria()">
                                    <label class="form-check-label" for="groupTypeDynamic">Dynamic</label>
                                </div>
                            </div>
                        </div>
                        <div class="mb-3" id="ruleCriteriaSection" style="display:none">
                            <label class="form-label">Rule Criteria</label>
                            <div class="mb-2">
                                <label class="form-label small text-muted">Logic</label>
                                <select class="form-select form-select-sm" id="ruleLogic">
                                    <option value="0">AND (all rules must match)</option>
                                    <option value="1">OR (any rule must match)</option>
                                </select>
                            </div>
                            <div id="rulesContainer">
                                <div class="rule-row input-group input-group-sm mb-2">
                                    <select class="form-select" id="ruleField0">
                                        <option value="0">Email</option>
                                        <option value="1">Phone</option>
                                        <option value="2">FirstName</option>
                                        <option value="3">LastName</option>
                                        <option value="4">City</option>
                                        <option value="5">Country</option>
                                    </select>
                                    <select class="form-select" id="ruleOp0">
                                        <option value="0">Contains</option>
                                        <option value="1">Equals</option>
                                        <option value="2">StartsWith</option>
                                        <option value="3">EndsWith</option>
                                    </select>
                                    <input type="text" class="form-control" id="ruleVal0" placeholder="Value">
                                </div>
                            </div>
                            <button type="button" class="btn btn-sm btn-outline-secondary" onclick="addRuleRow()">
                                <i class="bi bi-plus"></i> Add Rule
                            </button>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-primary" id="saveGroupBtn" onclick="saveGroup()">
                        <i class="bi bi-check-circle me-1"></i>Create Group
                    </button>
                </div>
            </div>
        </div>
    </div>`;

    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modal = new bootstrap.Modal(document.getElementById('groupModal'));
    modal.show();
}

let ruleCount = 1;

function toggleRuleCriteria() {
    const isDynamic = document.querySelector('input[name="groupType"]:checked')?.value === 'dynamic';
    const section = document.getElementById('ruleCriteriaSection');
    if (section) section.style.display = isDynamic ? '' : 'none';
}

function addRuleRow() {
    const container = document.getElementById('rulesContainer');
    const idx = ruleCount++;
    const html = `<div class="rule-row input-group input-group-sm mb-2">
        <select class="form-select" id="ruleField${idx}">
            <option value="0">Email</option><option value="1">Phone</option>
            <option value="2">FirstName</option><option value="3">LastName</option>
            <option value="4">City</option><option value="5">Country</option>
        </select>
        <select class="form-select" id="ruleOp${idx}">
            <option value="0">Contains</option><option value="1">Equals</option>
            <option value="2">StartsWith</option><option value="3">EndsWith</option>
        </select>
        <input type="text" class="form-control" id="ruleVal${idx}" placeholder="Value">
        <button class="btn btn-outline-danger" type="button" onclick="this.parentElement.remove()"><i class="bi bi-x"></i></button>
    </div>`;
    container.insertAdjacentHTML('beforeend', html);
}

function buildRuleCriteria() {
    const rows = document.querySelectorAll('#rulesContainer .rule-row');
    const rules = [];
    rows.forEach((row, idx) => {
        const fieldSelect = row.querySelector('select[id^="ruleField"]');
        const opSelect = row.querySelector('select[id^="ruleOp"]');
        const valInput = row.querySelector('input[id^="ruleVal"]');
        if (fieldSelect && opSelect && valInput && valInput.value.trim()) {
            rules.push({
                field: parseInt(fieldSelect.value),
                operator: parseInt(opSelect.value),
                value: valInput.value.trim()
            });
        }
    });

    if (rules.length === 0) return null;

    return {
        logic: parseInt(document.getElementById('ruleLogic')?.value || '0'),
        rules: rules
    };
}

async function saveGroup() {
    const name = document.getElementById('groupName').value.trim();
    const description = document.getElementById('groupDescription').value.trim();
    const isDynamic = document.querySelector('input[name="groupType"]:checked').value === 'dynamic';

    if (!name) {
        showNotification('Group name is required', 'error');
        return;
    }

    // Validate dynamic group has rule criteria
    if (isDynamic) {
        const criteria = buildRuleCriteria();
        if (!criteria || criteria.rules.length === 0) {
            showNotification('Dynamic groups must have at least one rule defined', 'error');
            return;
        }
    }

    const btn = document.getElementById('saveGroupBtn');
    btn.disabled = true;
    btn.innerHTML = '<i class="spinner-border spinner-border-sm me-1"></i>Saving...';

    const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');
    const apiUrl = window.AppUrls ? window.AppUrls.buildApiUrl('/api/contactgroups') : '/api/contactgroups';

    try {
        const response = await $.ajax({
            url: apiUrl,
            method: 'POST',
            contentType: 'application/json',
            headers: {
                'Authorization': token ? 'Bearer ' + token : ''
            },
            data: JSON.stringify({
                name: name,
                description: description,
                isStatic: !isDynamic,
                isDynamic: isDynamic,
                ruleCriteria: isDynamic ? buildRuleCriteria() : null
            })
        });

        if (response?.success || response?.id) {
            showNotification('Group created successfully!', 'success');
            const modal = bootstrap.Modal.getInstance(document.getElementById('groupModal'));
            if (modal) modal.hide();
            loadGroups();
        } else {
            showNotification(response?.message || 'Failed to create group', 'error');
        }
    } catch (err) {
        console.error('Failed to create group:', err);
        const errorMsg = err.responseJSON?.message || err.responseJSON?.errors?.join(', ') || 'Failed to create group';
        showNotification(errorMsg, 'error');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-check-circle me-1"></i>Create Group';
    }
}

// ========== EDIT GROUP ==========
function showEditGroupModal(groupId) {
    const group = allGroups.find(g => g.id === groupId);
    if (!group) {
        showNotification('Group not found', 'error');
        return;
    }

    // Remove existing modal if any
    const existingModal = document.getElementById('groupModal');
    if (existingModal) existingModal.remove();

    const modalHtml = `
    <div class="modal fade" id="groupModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-pencil me-2"></i>Edit Contact Group</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <form id="groupForm">
                        <input type="hidden" id="editGroupId" value="${groupId}">
                        <div class="mb-3">
                            <label for="groupName" class="form-label">Group Name <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="groupName" value="${escapeHtml(group.name || '')}" required>
                        </div>
                        <div class="mb-3">
                            <label for="groupDescription" class="form-label">Description</label>
                            <textarea class="form-control" id="groupDescription" rows="3">${escapeHtml(group.description || '')}</textarea>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Group Type</label>
                            <div>
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="groupType" id="groupTypeStatic" value="static" ${group.isStatic !== false ? 'checked' : ''}>
                                    <label class="form-check-label" for="groupTypeStatic">Static</label>
                                </div>
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="groupType" id="groupTypeDynamic" value="dynamic" ${group.isDynamic ? 'checked' : ''}>
                                    <label class="form-check-label" for="groupTypeDynamic">Dynamic</label>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-primary" id="updateGroupBtn" onclick="updateGroup()">
                        <i class="bi bi-check-circle me-1"></i>Update Group
                    </button>
                </div>
            </div>
        </div>
    </div>`;

    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modal = new bootstrap.Modal(document.getElementById('groupModal'));
    modal.show();
}

async function updateGroup() {
    const groupId = document.getElementById('editGroupId').value;
    const name = document.getElementById('groupName').value.trim();
    const description = document.getElementById('groupDescription').value.trim();
    const isDynamic = document.querySelector('input[name="groupType"]:checked').value === 'dynamic';

    if (!name) {
        showNotification('Group name is required', 'error');
        return;
    }

    const btn = document.getElementById('updateGroupBtn');
    btn.disabled = true;
    btn.innerHTML = '<i class="spinner-border spinner-border-sm me-1"></i>Updating...';

    const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');
    const apiUrl = window.AppUrls ? window.AppUrls.buildApiUrl(`/api/contactgroups/${groupId}`) : `/api/contactgroups/${groupId}`;

    try {
        const response = await $.ajax({
            url: apiUrl,
            method: 'PUT',
            contentType: 'application/json',
            headers: {
                'Authorization': token ? 'Bearer ' + token : ''
            },
            data: JSON.stringify({
                name: name,
                description: description,
                isStatic: !isDynamic,
                isDynamic: isDynamic
            })
        });

        if (response?.success !== false) {
            showNotification('Group updated successfully!', 'success');
            const modal = bootstrap.Modal.getInstance(document.getElementById('groupModal'));
            if (modal) modal.hide();
            loadGroups();
        } else {
            showNotification(response?.message || 'Failed to update group', 'error');
        }
    } catch (err) {
        console.error('Failed to update group:', err);
        const errorMsg = err.responseJSON?.message || 'Failed to update group';
        showNotification(errorMsg, 'error');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-check-circle me-1"></i>Update Group';
    }
}

// ========== DELETE GROUP ==========
async function deleteGroup(groupId) {
    if (!confirm('Are you sure you want to delete this group? This action cannot be undone.')) return;

    const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');
    const apiUrl = window.AppUrls ? window.AppUrls.buildApiUrl(`/api/contactgroups/${groupId}`) : `/api/contactgroups/${groupId}`;

    try {
        const response = await $.ajax({
            url: apiUrl,
            method: 'DELETE',
            headers: {
                'Authorization': token ? 'Bearer ' + token : ''
            }
        });

        if (response?.success !== false) {
            showNotification('Group deleted successfully!', 'success');
            // Remove card with animation
            const card = document.getElementById(`group-card-${groupId}`);
            if (card) {
                card.style.transition = 'opacity 0.3s';
                card.style.opacity = '0';
                setTimeout(() => {
                    card.remove();
                    // Check if any groups left
                    const remaining = document.querySelectorAll('[id^="group-card-"]');
                    if (remaining.length === 0) {
                        loadGroups(); // Reload to show empty state
                    }
                }, 300);
            } else {
                loadGroups();
            }
        } else {
            showNotification(response?.message || 'Failed to delete group', 'error');
        }
    } catch (err) {
        console.error('Failed to delete group:', err);
        showNotification('Failed to delete group', 'error');
    }
}

// ========== HELPERS ==========
function escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

// ========== IMPORT / EXPORT ==========
function showGroupImportModal() {
    showGroupImportModalForGroup(null);
}

function showGroupImportModalForGroup(groupId) {
    const existing = document.getElementById('groupImportModal');
    if (existing) existing.remove();

    const groupName = groupId ? (allGroups.find(g => g.id === groupId)?.name || `Group #${groupId}`) : '';
    const title = groupId ? `Import Contacts to "${escapeHtml(groupName)}"` : 'Import Contacts to Group';

    let groupSelect = '';
    if (!groupId) {
        let options = '<option value="">Select a group...</option>';
        allGroups.forEach(g => {
            options += `<option value="${g.id}">${escapeHtml(g.name)}</option>`;
        });
        groupSelect = `<div class="mb-3">
            <label for="importTargetGroup" class="form-label">Target Group <span class="text-danger">*</span></label>
            <select class="form-select" id="importTargetGroup">${options}</select>
        </div>`;
    }

    const modalHtml = `
    <div class="modal fade" id="groupImportModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-upload me-2"></i>${title}</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    ${groupSelect}
                    <div class="mb-3">
                        <label for="groupCsvFile" class="form-label">CSV File <span class="text-danger">*</span></label>
                        <input type="file" class="form-control" id="groupCsvFile" accept=".csv">
                        <div class="form-text">CSV should have: FirstName, LastName, Email, PhoneNumber, City, Country</div>
                        <a href="javascript:void(0)" class="small text-primary mt-1 d-inline-block" onclick="downloadSampleCsvForGroups()">
                            <i class="bi bi-download me-1"></i>Download Sample CSV
                        </a>
                    </div>
                    <div id="groupImportProgress" style="display:none">
                        <div class="progress"><div class="progress-bar progress-bar-striped progress-bar-animated" style="width:100%">Importing...</div></div>
                    </div>
                    <div id="groupImportResult" style="display:none"></div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-primary" id="groupImportBtn" onclick="doGroupImport(${groupId || 'null'})">
                        <i class="bi bi-upload me-1"></i>Import
                    </button>
                </div>
            </div>
        </div>
    </div>`;

    document.body.insertAdjacentHTML('beforeend', modalHtml);
    new bootstrap.Modal(document.getElementById('groupImportModal')).show();
}

function doGroupImport(presetGroupId) {
    const fileInput = document.getElementById('groupCsvFile');
    if (!fileInput.files || !fileInput.files[0]) {
        showNotification('Please select a CSV file', 'error');
        return;
    }

    const groupId = presetGroupId || document.getElementById('importTargetGroup')?.value;
    if (!groupId) {
        showNotification('Please select a target group', 'error');
        return;
    }

    const formData = new FormData();
    formData.append('file', fileInput.files[0]);

    const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');
    const apiUrl = (window.AppUrls && window.AppUrls.buildApiUrl)
        ? window.AppUrls.buildApiUrl(`/api/contacts/import/csv?groupId=${groupId}`)
        : `/api/contacts/import/csv?groupId=${groupId}`;

    document.getElementById('groupImportProgress').style.display = '';
    document.getElementById('groupImportBtn').disabled = true;

    $.ajax({
        url: apiUrl,
        method: 'POST',
        headers: { 'Authorization': token ? 'Bearer ' + token : '' },
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            document.getElementById('groupImportProgress').style.display = 'none';
            const resultDiv = document.getElementById('groupImportResult');
            resultDiv.style.display = '';
            if (response?.success !== false) {
                resultDiv.innerHTML = '<div class="alert alert-success">' + (response?.message || 'Import completed!') + '</div>';
                showNotification('Contacts imported successfully!', 'success');
                loadGroups();
            } else {
                resultDiv.innerHTML = '<div class="alert alert-danger">' + (response?.message || 'Import failed') + '</div>';
            }
            document.getElementById('groupImportBtn').disabled = false;
        },
        error: function(xhr) {
            document.getElementById('groupImportProgress').style.display = 'none';
            document.getElementById('groupImportBtn').disabled = false;
            showNotification(xhr.responseJSON?.message || 'Import failed', 'error');
        }
    });
}

function exportGroupContacts(groupId) {
    const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');
    const apiUrl = (window.AppUrls && window.AppUrls.buildApiUrl)
        ? window.AppUrls.buildApiUrl(`/api/contactgroups/${groupId}/contacts`)
        : `/api/contactgroups/${groupId}/contacts`;

    showNotification('Exporting contacts...', 'info');

    $.ajax({
        url: apiUrl,
        method: 'GET',
        headers: { 'Authorization': token ? 'Bearer ' + token : '' },
        success: function(response) {
            let contacts = [];
            if (response?.success && response.data) {
                contacts = response.data.items || response.data || [];
            } else if (Array.isArray(response)) {
                contacts = response;
            }

            if (contacts.length === 0) {
                showNotification('No contacts in this group to export', 'info');
                return;
            }

            // Build CSV
            const headers = ['FirstName', 'LastName', 'Email', 'PhoneNumber', 'City', 'Country'];
            let csv = headers.join(',') + '\\n';
            contacts.forEach(c => {
                const contact = c.contact || c;
                csv += [
                    csvEscape(contact.firstName),
                    csvEscape(contact.lastName),
                    csvEscape(contact.email),
                    csvEscape(contact.phoneNumber),
                    csvEscape(contact.city),
                    csvEscape(contact.country)
                ].join(',') + '\\n';
            });

            // Download
            const blob = new Blob([csv], { type: 'text/csv' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            const group = allGroups.find(g => g.id === groupId);
            a.download = `${(group?.name || 'group')}_contacts.csv`;
            a.click();
            URL.revokeObjectURL(url);
            showNotification('Export completed!', 'success');
        },
        error: function() {
            showNotification('Failed to export contacts', 'error');
        }
    });
}

function csvEscape(val) {
    if (!val) return '';
    val = String(val);
    if (val.includes(',') || val.includes('"') || val.includes('\\n')) {
        return '"' + val.replace(/"/g, '""') + '"';
    }
    return val;
}

function formatDate(dateStr) {
    if (!dateStr) return '';
    try {
        return new Date(dateStr).toLocaleDateString();
    } catch {
        return dateStr;
    }
}

function downloadSampleCsvForGroups() {
    const csv = 'FirstName,LastName,Email,PhoneNumber,City,Country\n' +
        'John,Doe,john.doe@example.com,+1234567890,New York,US\n' +
        'Jane,Smith,jane.smith@example.com,+0987654321,London,UK\n' +
        'Bob,Johnson,bob.johnson@example.com,+1122334455,Toronto,CA\n';
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'sample_contacts.csv';
    a.click();
    URL.revokeObjectURL(url);
}
