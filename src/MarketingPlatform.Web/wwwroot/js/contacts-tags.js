/**
 * Contacts Tag Management - SERVER-SIDE API INTEGRATION
 * All calls go through Web Controller proxy
 */

let tagsTable;

$(document).ready(function () {
    initTagsTable();
    setupTagPreview();
});

/**
 * Initialize tags DataTable
 */
function initTagsTable() {
    tagsTable = initDataTable('#tagsTable', {
        ajax: {
            url: '/Contacts/GetTags',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        columns: [
            {
                data: 'name',
                render: function (data, type, row) {
                    var color = row.color || '#667eea';
                    return '<span class="badge fs-6" style="background-color:' + escapeHtml(color) + ';">' + escapeHtml(data || '') + '</span>';
                }
            },
            {
                data: 'color',
                render: function (data) {
                    if (!data) return '<span class="text-muted">Default</span>';
                    return '<div class="d-flex align-items-center gap-2">' +
                        '<div style="width:20px;height:20px;border-radius:4px;background:' + escapeHtml(data) + ';"></div>' +
                        '<code>' + escapeHtml(data) + '</code></div>';
                }
            },
            {
                data: 'contactCount',
                render: function (data) {
                    var count = data || 0;
                    return '<span class="badge bg-light text-dark">' + count + ' contacts</span>';
                }
            },
            {
                data: 'createdAt',
                render: function (data) {
                    return formatShortDate(data);
                }
            },
            {
                data: null,
                orderable: false,
                className: 'text-end',
                render: function (data, type, row) {
                    return '<div class="btn-group btn-group-sm">' +
                        '<button class="btn btn-outline-primary" onclick="editTag(' + row.id + ', \'' + escapeHtml(row.name || '') + '\', \'' + escapeHtml(row.color || '#667eea') + '\')" title="Edit">' +
                        '<i class="bi bi-pencil"></i></button>' +
                        '<button class="btn btn-outline-danger" onclick="deleteTag(' + row.id + ', \'' + escapeHtml(row.name || '') + '\')" title="Delete">' +
                        '<i class="bi bi-trash"></i></button>' +
                        '</div>';
                }
            }
        ],
        order: [[0, 'asc']],
        drawCallback: function () {
            updateTagStats();
        }
    });
}

/**
 * Update summary statistics
 */
function updateTagStats() {
    if (tagsTable) {
        var info = tagsTable.page.info();
        document.getElementById('totalTags').textContent = info.recordsTotal || 0;

        // Calculate total tagged contacts from visible data
        var totalTagged = 0;
        tagsTable.rows().every(function () {
            var data = this.data();
            totalTagged += (data.contactCount || 0);
        });
        document.getElementById('totalTagged').textContent = totalTagged;
    }
}

/**
 * Setup live tag preview
 */
function setupTagPreview() {
    var nameInput = document.getElementById('tagName');
    var colorInput = document.getElementById('tagColor');
    var preview = document.getElementById('tagPreview');

    if (nameInput) {
        nameInput.addEventListener('input', function () {
            preview.textContent = this.value || 'Sample Tag';
        });
    }
    if (colorInput) {
        colorInput.addEventListener('input', function () {
            preview.style.backgroundColor = this.value;
        });
    }
}

/**
 * Show create tag modal
 */
function showCreateTagModal() {
    document.getElementById('tagModalTitle').textContent = 'Create Tag';
    document.getElementById('tagId').value = '';
    document.getElementById('tagName').value = '';
    document.getElementById('tagColor').value = '#667eea';
    document.getElementById('tagPreview').textContent = 'Sample Tag';
    document.getElementById('tagPreview').style.backgroundColor = '#667eea';
    new bootstrap.Modal(document.getElementById('tagModal')).show();
}

/**
 * Edit tag
 */
function editTag(id, name, color) {
    document.getElementById('tagModalTitle').textContent = 'Edit Tag';
    document.getElementById('tagId').value = id;
    document.getElementById('tagName').value = name;
    document.getElementById('tagColor').value = color || '#667eea';
    document.getElementById('tagPreview').textContent = name;
    document.getElementById('tagPreview').style.backgroundColor = color || '#667eea';
    new bootstrap.Modal(document.getElementById('tagModal')).show();
}

/**
 * Save tag (create or update)
 */
function saveTag() {
    var name = document.getElementById('tagName').value.trim();
    var color = document.getElementById('tagColor').value;
    var tagId = document.getElementById('tagId').value;

    if (!name) {
        showNotification('Please enter a tag name', 'error');
        return;
    }

    var btn = document.getElementById('saveTagBtn');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Saving...';

    var tagData = { name: name, color: color };

    var url = tagId ? '/Contacts/UpdateTag?id=' + tagId : '/Contacts/CreateTag';

    $.ajax({
        url: url,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(tagData),
        success: function (response) {
            if (response.success) {
                showNotification(tagId ? 'Tag updated successfully' : 'Tag created successfully', 'success');
                bootstrap.Modal.getInstance(document.getElementById('tagModal')).hide();
                tagsTable.ajax.reload();
            } else {
                showNotification(response.message || 'Failed to save tag', 'error');
            }
        },
        error: function () {
            showNotification('An error occurred', 'error');
        },
        complete: function () {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-check-lg"></i> Save Tag';
        }
    });
}

/**
 * Delete tag
 */
function deleteTag(id, name) {
    confirmAction('Are you sure you want to delete the tag "' + name + '"? This will remove it from all contacts.', function () {
        $.ajax({
            url: '/Contacts/DeleteTag?id=' + id,
            method: 'POST',
            success: function (response) {
                if (response.success) {
                    showNotification('Tag deleted successfully', 'success');
                    tagsTable.ajax.reload();
                } else {
                    showNotification(response.message || 'Failed to delete tag', 'error');
                }
            },
            error: function () {
                showNotification('An error occurred', 'error');
            }
        });
    });
}
