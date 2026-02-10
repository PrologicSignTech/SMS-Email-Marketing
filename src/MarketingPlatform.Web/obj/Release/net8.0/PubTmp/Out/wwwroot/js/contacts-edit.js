/**
 * Contact Edit Page - Loads contact data and handles update
 * Includes group membership management
 */

const contactId = window.contactsConfig?.contactId || null;
let allGroups = [];
let currentGroupIds = [];

$(document).ready(function () {
    if (contactId) {
        loadContactData();
        loadGroupsForContact();
    }
    setupFormSubmit();
});

function loadContactData() {
    const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');
    const apiUrl = (window.AppUrls && window.AppUrls.buildApiUrl)
        ? window.AppUrls.buildApiUrl(`/api/contacts/${contactId}`)
        : `/api/contacts/${contactId}`;

    $.ajax({
        url: apiUrl,
        method: 'GET',
        headers: { 'Authorization': token ? 'Bearer ' + token : '' },
        success: function (response) {
            const contact = response?.data || response;
            if (contact) {
                populateForm(contact);
            } else {
                showNotification('Contact not found', 'error');
            }
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                showNotification('Session expired. Please log in again.', 'error');
            } else {
                showNotification('Failed to load contact data', 'error');
            }
        }
    });
}

function populateForm(contact) {
    const fields = [
        { id: 'firstName', value: contact.firstName },
        { id: 'lastName', value: contact.lastName },
        { id: 'email', value: contact.email },
        { id: 'phoneNumber', value: contact.phoneNumber },
        { id: 'city', value: contact.city },
        { id: 'country', value: contact.country },
        { id: 'postalCode', value: contact.postalCode }
    ];

    fields.forEach(field => {
        const el = document.getElementById(field.id);
        if (el) el.value = field.value || '';
    });

    const isActiveEl = document.getElementById('isActive');
    if (isActiveEl) isActiveEl.checked = contact.isActive !== false;

    // Store current groups from the contact data
    if (contact.groups && Array.isArray(contact.groups)) {
        // groups might be strings (group names) or objects with id/name
        window._contactGroupNames = contact.groups.map(g => typeof g === 'string' ? g : (g.name || g));
    }
}

function loadGroupsForContact() {
    // Load all available groups
    $.ajax({
        url: '/Contacts/GetContactGroups',
        method: 'GET',
        success: function (response) {
            let groups = [];
            if (response?.success && response.items) {
                if (Array.isArray(response.items)) {
                    groups = response.items;
                } else if (response.items.items && Array.isArray(response.items.items)) {
                    groups = response.items.items;
                } else {
                    groups = response.items;
                }
            }
            allGroups = groups;

            // Now load which groups this contact belongs to
            loadContactGroupMembership();
        },
        error: function () {
            const container = document.getElementById('groupsCheckboxes');
            if (container) container.innerHTML = '<span class="text-muted">Failed to load groups</span>';
        }
    });
}

function loadContactGroupMembership() {
    const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');

    // We need to check each group for this contact's membership
    // Use the contact's groups data from the contact API response
    // Wait a bit for the contact data to load
    setTimeout(function () {
        const contactGroupNames = window._contactGroupNames || [];
        currentGroupIds = [];

        // Match group names to group IDs
        allGroups.forEach(g => {
            const gName = g.name || '';
            if (contactGroupNames.some(cgn => cgn === gName)) {
                currentGroupIds.push(g.id);
            }
        });

        renderGroupCheckboxes();
    }, 500);
}

function renderGroupCheckboxes() {
    const container = document.getElementById('groupsCheckboxes');
    if (!container) return;

    if (!allGroups || allGroups.length === 0) {
        container.innerHTML = '<span class="text-muted">No groups available. <a href="/Contacts/Groups">Create groups</a> first.</span>';
        return;
    }

    let html = '<div class="row">';
    allGroups.forEach(group => {
        const checked = currentGroupIds.includes(group.id) ? 'checked' : '';
        const isStatic = group.isStatic !== false;
        const typeBadge = isStatic ? '' : '<span class="badge bg-warning ms-1" style="font-size:0.65rem">Dynamic</span>';

        html += `
            <div class="col-md-6 col-lg-4 mb-2">
                <div class="form-check">
                    <input class="form-check-input group-checkbox" type="checkbox" value="${group.id}" id="group_${group.id}" ${checked} data-original="${checked ? 'true' : 'false'}">
                    <label class="form-check-label" for="group_${group.id}">
                        ${escapeHtml(group.name || 'Unnamed')} ${typeBadge}
                        <small class="text-muted d-block">${(group.contactCount || 0)} contacts</small>
                    </label>
                </div>
            </div>
        `;
    });
    html += '</div>';
    container.innerHTML = html;
}

function setupFormSubmit() {
    $('#contactForm').on('submit', function (e) {
        e.preventDefault();

        const firstName = $('#firstName').val().trim();
        const lastName = $('#lastName').val().trim();
        const email = $('#email').val().trim();

        if (!firstName || !lastName || !email) {
            showNotification('First name, last name, and email are required', 'error');
            return;
        }

        const formData = {
            phoneNumber: $('#phoneNumber').val().trim(),
            email: email,
            firstName: firstName,
            lastName: lastName,
            country: $('#country').val().trim(),
            city: $('#city').val().trim(),
            postalCode: $('#postalCode').val().trim(),
            isActive: $('#isActive').is(':checked')
        };

        const submitBtn = document.getElementById('submitBtn');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="spinner-border spinner-border-sm me-2"></i>Saving...';
        }

        const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');
        const apiUrl = (window.AppUrls && window.AppUrls.buildApiUrl)
            ? window.AppUrls.buildApiUrl(`/api/contacts/${contactId}`)
            : `/api/contacts/${contactId}`;

        $.ajax({
            url: apiUrl,
            method: 'PUT',
            contentType: 'application/json',
            headers: { 'Authorization': token ? 'Bearer ' + token : '' },
            data: JSON.stringify(formData),
            success: function (response) {
                if (response?.success !== false) {
                    // Now update group memberships
                    updateGroupMemberships(token, function () {
                        showNotification('Contact updated successfully!', 'success');
                        setTimeout(() => { window.location.href = '/Contacts'; }, 1500);
                    });
                } else {
                    showNotification(response?.message || 'Failed to update contact', 'error');
                    resetButton();
                }
            },
            error: function (xhr) {
                const msg = xhr.responseJSON?.message || 'Failed to update contact';
                showNotification(msg, 'error');
                resetButton();
            }
        });

        function resetButton() {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Update Contact';
            }
        }
    });
}

function updateGroupMemberships(token, callback) {
    const checkboxes = document.querySelectorAll('.group-checkbox');
    if (!checkboxes || checkboxes.length === 0) {
        callback();
        return;
    }

    const promises = [];

    checkboxes.forEach(cb => {
        const groupId = parseInt(cb.value);
        const isChecked = cb.checked;
        const wasChecked = cb.dataset.original === 'true';

        if (isChecked && !wasChecked) {
            // Add contact to group
            const addUrl = (window.AppUrls && window.AppUrls.buildApiUrl)
                ? window.AppUrls.buildApiUrl(`/api/contactgroups/${groupId}/contacts/${contactId}`)
                : `/api/contactgroups/${groupId}/contacts/${contactId}`;

            promises.push(
                $.ajax({
                    url: addUrl,
                    method: 'POST',
                    headers: { 'Authorization': token ? 'Bearer ' + token : '' }
                }).catch(err => {
                    console.error(`Failed to add contact to group ${groupId}:`, err);
                })
            );
        } else if (!isChecked && wasChecked) {
            // Remove contact from group
            const removeUrl = (window.AppUrls && window.AppUrls.buildApiUrl)
                ? window.AppUrls.buildApiUrl(`/api/contactgroups/${groupId}/contacts/${contactId}`)
                : `/api/contactgroups/${groupId}/contacts/${contactId}`;

            promises.push(
                $.ajax({
                    url: removeUrl,
                    method: 'DELETE',
                    headers: { 'Authorization': token ? 'Bearer ' + token : '' }
                }).catch(err => {
                    console.error(`Failed to remove contact from group ${groupId}:`, err);
                })
            );
        }
    });

    if (promises.length > 0) {
        Promise.all(promises).then(callback).catch(callback);
    } else {
        callback();
    }
}

function escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}
