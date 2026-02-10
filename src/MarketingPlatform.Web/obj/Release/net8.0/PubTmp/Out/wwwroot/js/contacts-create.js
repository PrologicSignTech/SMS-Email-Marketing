/**
 * Contacts Create Page - Creates contact via Web Controller (server-side proxy to API)
 * Loads actual groups from Web Controller and assigns contact to selected groups after creation
 */

let allGroups = [];

$(document).ready(function () {
    loadGroups();
    setupFormSubmit();
});

function loadGroups() {
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
                }
            }
            allGroups = groups;
            renderGroupCheckboxes();
        },
        error: function () {
            const container = document.getElementById('groupsCheckboxes');
            if (container) container.innerHTML = '<span class="text-muted">No groups available. <a href="/Contacts/Groups">Create groups</a> first.</span>';
        }
    });
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
        const isStatic = group.isStatic !== false;
        const typeBadge = isStatic ? '' : '<span class="badge bg-warning ms-1" style="font-size:0.65rem">Dynamic</span>';

        html += `
            <div class="col-md-6 col-lg-4 mb-2">
                <div class="form-check">
                    <input class="form-check-input group-checkbox" type="checkbox" value="${group.id}" id="group_${group.id}">
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

        if (!firstName) { showNotification('First name is required', 'error'); return; }
        if (!lastName) { showNotification('Last name is required', 'error'); return; }
        if (!email) { showNotification('Email is required', 'error'); return; }

        const formData = {
            firstName: firstName,
            lastName: lastName,
            email: email,
            phoneNumber: $('#phoneNumber').val().trim(),
            city: $('#city').val().trim(),
            country: $('#country').val().trim(),
            postalCode: $('#postalCode').val().trim()
        };

        const submitBtn = document.getElementById('submitBtn');
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="spinner-border spinner-border-sm me-2"></i>Creating...';

        $.ajax({
            url: '/Contacts/CreateContact',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response?.success) {
                    const contactData = response.data;
                    const newContactId = contactData?.id;

                    if (newContactId) {
                        assignToGroups(newContactId, function () {
                            showNotification('Contact created successfully!', 'success');
                            setTimeout(() => { window.location.href = '/Contacts'; }, 1000);
                        });
                    } else {
                        showNotification('Contact created!', 'success');
                        setTimeout(() => { window.location.href = '/Contacts'; }, 1000);
                    }
                } else {
                    showNotification(response?.message || 'Failed to create contact', 'error');
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Create Contact';
                }
            },
            error: function (xhr) {
                const msg = xhr.responseJSON?.message || 'Failed to create contact';
                showNotification(msg, 'error');
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Create Contact';
            }
        });
    });
}

function assignToGroups(contactId, callback) {
    const selectedGroups = [];
    document.querySelectorAll('.group-checkbox:checked').forEach(cb => {
        selectedGroups.push(parseInt(cb.value));
    });

    if (selectedGroups.length === 0) {
        callback();
        return;
    }

    const promises = selectedGroups.map(groupId => {
        return $.ajax({
            url: `/Contacts/AddContactToGroup?groupId=${groupId}&contactId=${contactId}`,
            method: 'POST'
        }).catch(err => {
            console.error(`Failed to add to group ${groupId}:`, err);
        });
    });

    Promise.all(promises).then(callback).catch(callback);
}

function escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}
