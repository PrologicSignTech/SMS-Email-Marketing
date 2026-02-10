/**
 * Contacts Duplicate Detection & Merge - SERVER-SIDE API INTEGRATION
 * All calls go through Web Controller proxy
 */

var duplicateReport = null;
var currentResolveGroup = null;

/**
 * Scan for duplicates
 */
function scanForDuplicates() {
    var scanBtn = document.getElementById('scanBtn');
    scanBtn.disabled = true;
    scanBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Scanning...';

    document.getElementById('initialState').classList.add('d-none');
    document.getElementById('noDuplicatesSection').classList.add('d-none');
    document.getElementById('duplicatesSection').classList.add('d-none');
    document.getElementById('loadingSection').classList.remove('d-none');

    $.ajax({
        url: '/Contacts/GetDuplicatesReport',
        method: 'GET',
        success: function (response) {
            document.getElementById('loadingSection').classList.add('d-none');

            if (response.success && response.data) {
                duplicateReport = response.data;
                renderDuplicateReport(response.data);
            } else {
                document.getElementById('noDuplicatesSection').classList.remove('d-none');
                updateDuplicateStats(0, 0, 0, 0);
            }
        },
        error: function () {
            document.getElementById('loadingSection').classList.add('d-none');
            showNotification('Failed to scan for duplicates', 'error');
            document.getElementById('initialState').classList.remove('d-none');
        },
        complete: function () {
            scanBtn.disabled = false;
            scanBtn.innerHTML = '<i class="bi bi-search"></i> Scan for Duplicates';
        }
    });
}

/**
 * Update stats cards
 */
function updateDuplicateStats(groups, contacts, phone, email) {
    document.getElementById('totalDuplicates').textContent = groups;
    document.getElementById('totalDuplicateContacts').textContent = contacts;
    document.getElementById('phoneDuplicates').textContent = phone;
    document.getElementById('emailDuplicates').textContent = email;
}

/**
 * Render duplicate report
 */
function renderDuplicateReport(data) {
    var groups = data.duplicateGroups || data.DuplicateGroups || [];
    var totalDups = data.totalDuplicates || data.TotalDuplicates || groups.length;

    if (groups.length === 0) {
        document.getElementById('noDuplicatesSection').classList.remove('d-none');
        updateDuplicateStats(0, 0, 0, 0);
        return;
    }

    // Calculate stats
    var totalContacts = 0;
    var phoneGroups = 0;
    var emailGroups = 0;

    groups.forEach(function (g) {
        var count = g.count || g.Count || 0;
        totalContacts += count;
        var dtype = (g.duplicateType || g.DuplicateType || '').toLowerCase();
        if (dtype === 'phone' || dtype === 'phonenumber') phoneGroups++;
        else if (dtype === 'email') emailGroups++;
    });

    updateDuplicateStats(groups.length, totalContacts, phoneGroups, emailGroups);

    document.getElementById('duplicatesSection').classList.remove('d-none');
    renderDuplicateGroups(groups);
}

/**
 * Render duplicate groups list
 */
function renderDuplicateGroups(groups) {
    var container = document.getElementById('duplicatesList');
    var filterType = document.getElementById('duplicateTypeFilter').value.toLowerCase();

    var filtered = groups;
    if (filterType) {
        filtered = groups.filter(function (g) {
            var dtype = (g.duplicateType || g.DuplicateType || '').toLowerCase();
            return dtype === filterType.toLowerCase();
        });
    }

    if (filtered.length === 0) {
        container.innerHTML = '<div class="text-center py-4 text-muted">No duplicates match the selected filter</div>';
        return;
    }

    var html = '';
    filtered.forEach(function (group, index) {
        var contacts = group.contacts || group.Contacts || [];
        var dupKey = group.duplicateKey || group.DuplicateKey || 'Unknown';
        var dupType = group.duplicateType || group.DuplicateType || 'Unknown';
        var count = group.count || group.Count || contacts.length;

        var typeBadge = dupType.toLowerCase() === 'email'
            ? '<span class="badge bg-primary"><i class="bi bi-envelope"></i> Email</span>'
            : '<span class="badge bg-info"><i class="bi bi-telephone"></i> Phone</span>';

        html += '<div class="card mb-3 border-start border-4 border-warning">';
        html += '<div class="card-header d-flex justify-content-between align-items-center">';
        html += '<div>' + typeBadge + ' <strong class="ms-2">' + escapeHtml(dupKey) + '</strong>';
        html += ' <span class="badge bg-warning text-dark ms-2">' + count + ' duplicates</span></div>';
        html += '<button class="btn btn-sm btn-primary" onclick="showResolveModal(' + index + ')">';
        html += '<i class="bi bi-tools"></i> Resolve</button>';
        html += '</div>';
        html += '<div class="card-body p-0">';
        html += '<table class="table table-sm mb-0">';
        html += '<thead><tr><th>ID</th><th>Name</th><th>Email</th><th>Phone</th><th>Created</th></tr></thead>';
        html += '<tbody>';

        contacts.forEach(function (c) {
            var cId = c.contactId || c.ContactId || c.id || c.Id || '-';
            var firstName = c.firstName || c.FirstName || '';
            var lastName = c.lastName || c.LastName || '';
            var email = c.email || c.Email || '';
            var phone = c.phoneNumber || c.PhoneNumber || '';
            var created = c.createdAt || c.CreatedAt || '';

            html += '<tr>';
            html += '<td>' + cId + '</td>';
            html += '<td>' + escapeHtml(firstName + ' ' + lastName).trim() + '</td>';
            html += '<td>' + escapeHtml(email) + '</td>';
            html += '<td>' + escapeHtml(phone) + '</td>';
            html += '<td>' + formatShortDate(created) + '</td>';
            html += '</tr>';
        });

        html += '</tbody></table>';
        html += '</div></div>';
    });

    container.innerHTML = html;
}

/**
 * Filter duplicates by type
 */
function filterDuplicates() {
    if (duplicateReport) {
        var groups = duplicateReport.duplicateGroups || duplicateReport.DuplicateGroups || [];
        renderDuplicateGroups(groups);
    }
}

/**
 * Show resolve modal
 */
function showResolveModal(groupIndex) {
    var groups = duplicateReport.duplicateGroups || duplicateReport.DuplicateGroups || [];
    if (groupIndex >= groups.length) return;

    currentResolveGroup = groups[groupIndex];
    var contacts = currentResolveGroup.contacts || currentResolveGroup.Contacts || [];

    // Build radio buttons for primary contact selection
    var container = document.getElementById('primaryContactSelect');
    var html = '';

    contacts.forEach(function (c, idx) {
        var cId = c.contactId || c.ContactId || c.id || c.Id;
        var firstName = c.firstName || c.FirstName || '';
        var lastName = c.lastName || c.LastName || '';
        var email = c.email || c.Email || '';
        var phone = c.phoneNumber || c.PhoneNumber || '';

        html += '<div class="form-check mb-2 p-2 border rounded">';
        html += '<input class="form-check-input" type="radio" name="primaryContact" value="' + cId + '"' + (idx === 0 ? ' checked' : '') + '>';
        html += '<label class="form-check-label">';
        html += '<strong>#' + cId + '</strong> - ' + escapeHtml(firstName + ' ' + lastName).trim();
        if (email) html += ' | ' + escapeHtml(email);
        if (phone) html += ' | ' + escapeHtml(phone);
        html += '</label></div>';
    });

    container.innerHTML = html;

    new bootstrap.Modal(document.getElementById('resolveModal')).show();
}

/**
 * Resolve duplicates
 */
function resolveDuplicates() {
    if (!currentResolveGroup) return;

    var action = document.querySelector('input[name="resolveAction"]:checked').value;
    var primaryContactId = parseInt(document.querySelector('input[name="primaryContact"]:checked').value);
    var contacts = currentResolveGroup.contacts || currentResolveGroup.Contacts || [];

    var duplicateIds = contacts
        .map(function (c) { return c.contactId || c.ContactId || c.id || c.Id; })
        .filter(function (id) { return id !== primaryContactId; });

    var btn = document.getElementById('resolveBtn');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Resolving...';

    $.ajax({
        url: '/Contacts/ResolveDuplicates',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            primaryContactId: primaryContactId,
            duplicateContactIds: duplicateIds,
            action: action
        }),
        success: function (response) {
            if (response.success) {
                showNotification(response.message || 'Duplicates resolved successfully!', 'success');
                bootstrap.Modal.getInstance(document.getElementById('resolveModal')).hide();
                // Re-scan
                scanForDuplicates();
            } else {
                showNotification(response.message || 'Failed to resolve duplicates', 'error');
            }
        },
        error: function () {
            showNotification('An error occurred', 'error');
        },
        complete: function () {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-check-lg"></i> Resolve';
        }
    });
}
