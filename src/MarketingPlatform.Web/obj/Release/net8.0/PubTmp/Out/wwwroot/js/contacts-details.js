/**
 * Contact Details Page - Loads real contact data from API
 */

document.addEventListener('DOMContentLoaded', function () {
    loadContactDetails();
});

function loadContactDetails() {
    const pathParts = window.location.pathname.split('/');
    const contactId = pathParts[pathParts.length - 1];

    if (!contactId || isNaN(contactId)) {
        document.getElementById('contactName').textContent = 'Invalid Contact';
        return;
    }

    // Store contactId for edit button
    window._contactId = contactId;

    const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');
    const apiUrl = (window.AppUrls && window.AppUrls.buildApiUrl)
        ? window.AppUrls.buildApiUrl(`/api/contacts/${contactId}`)
        : `/api/contacts/${contactId}`;

    $.ajax({
        url: apiUrl,
        method: 'GET',
        headers: { 'Authorization': token ? 'Bearer ' + token : '' },
        success: function (response) {
            let contact = null;
            if (response?.success && response.data) {
                contact = response.data;
            } else if (response?.id) {
                contact = response;
            }

            if (contact) {
                renderContact(contact);
            } else {
                document.getElementById('contactName').textContent = 'Contact not found';
            }
        },
        error: function (xhr) {
            document.getElementById('contactName').textContent = 'Error loading contact';
            if (xhr.status === 401) {
                if (typeof showNotification === 'function') showNotification('Session expired. Please log in again.', 'error');
            } else {
                if (typeof showNotification === 'function') showNotification('Failed to load contact details', 'error');
            }
        }
    });
}

function renderContact(c) {
    // Header card
    const name = [c.firstName, c.lastName].filter(Boolean).join(' ') || 'Unknown';
    document.getElementById('contactName').textContent = name;
    document.getElementById('contactEmail').textContent = c.email || '-';

    const statusBadge = document.getElementById('statusBadge');
    if (c.isActive !== false) {
        statusBadge.className = 'badge bg-success';
        statusBadge.textContent = 'Active';
    } else {
        statusBadge.className = 'badge bg-secondary';
        statusBadge.textContent = 'Inactive';
    }

    // Detail fields
    setText('firstName', c.firstName);
    setText('lastName', c.lastName);
    setText('emailDetail', c.email);
    setText('phone', c.phoneNumber);
    setText('city', c.city);
    setText('country', c.country);
    setText('postalCode', c.postalCode);
    setText('statusDetail', c.isActive !== false ? 'Active' : 'Inactive');
    setText('createdAt', c.createdAt ? new Date(c.createdAt).toLocaleDateString() : '-');

    // Groups
    const groupsDiv = document.getElementById('groupsList');
    if (groupsDiv) {
        if (c.groups && c.groups.length > 0) {
            groupsDiv.innerHTML = c.groups.map(g =>
                `<span class="badge bg-primary me-1 mb-1">${escapeHtml(g)}</span>`
            ).join('');
        } else {
            groupsDiv.innerHTML = '<span class="text-muted">No groups assigned</span>';
        }
    }

    // Tags
    const tagsDiv = document.getElementById('tagsList');
    if (tagsDiv) {
        if (c.tags && c.tags.length > 0) {
            tagsDiv.innerHTML = c.tags.map(t =>
                `<span class="badge bg-info me-1 mb-1">${escapeHtml(t)}</span>`
            ).join('');
        } else {
            tagsDiv.innerHTML = '<span class="text-muted">No tags</span>';
        }
    }

    // Custom Attributes
    const attrsDiv = document.getElementById('customAttributes');
    if (attrsDiv) {
        if (c.customAttributes && Object.keys(c.customAttributes).length > 0) {
            let html = '<div class="table-responsive"><table class="table table-sm mb-0">';
            html += '<thead><tr><th>Key</th><th>Value</th></tr></thead><tbody>';
            for (const [key, val] of Object.entries(c.customAttributes)) {
                html += `<tr><td class="text-muted">${escapeHtml(key)}</td><td>${escapeHtml(val)}</td></tr>`;
            }
            html += '</tbody></table></div>';
            attrsDiv.innerHTML = html;
        } else {
            attrsDiv.innerHTML = '<span class="text-muted">No custom attributes</span>';
        }
    }
}

function setText(id, value) {
    const el = document.getElementById(id);
    if (el) el.textContent = value || '-';
}

function escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

function editContact() {
    const contactId = window._contactId || window.location.pathname.split('/').pop();
    window.location.href = `/Contacts/Edit/${contactId}`;
}

if (typeof window !== 'undefined') {
    window.editContact = editContact;
}
