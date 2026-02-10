/**
 * Phone Numbers Index Page - DataTables Implementation
 * SERVER-SIDE: Calls Web controller endpoints instead of API directly
 */

let phoneNumbersTable;

const typeNames = ['Local', 'Toll-Free', 'Short Code', 'Mobile'];
const typeColors = { 0: 'primary', 1: 'success', 2: 'warning', 3: 'info' };

const capabilityNames = ['SMS', 'MMS', 'Both'];
const capabilityColors = { 0: 'info', 1: 'purple', 2: 'primary' };

const statusNames = ['Available', 'Active', 'Suspended', 'Released'];
const statusColors = { 0: 'info', 1: 'success', 2: 'warning', 3: 'secondary' };

$(document).ready(function () {
    initPhoneNumbersTable();
});

function initPhoneNumbersTable() {
    phoneNumbersTable = $('#phoneNumbersTable').DataTable({
        serverSide: true,
        processing: true,

        ajax: {
            url: '/PhoneNumbers/GetPhoneNumbers',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search
                });
            },
            dataSrc: function (json) {
                return json.data || [];
            }
        },

        columns: [
            {
                data: 'number',
                name: 'Number',
                orderable: true,
                searchable: true,
                render: function (data, type, row) {
                    if (type === 'display') {
                        return '<strong>' + escapeHtml(data || 'N/A') + '</strong>';
                    }
                    return data;
                }
            },
            {
                data: 'friendlyName',
                name: 'FriendlyName',
                orderable: true,
                searchable: true,
                render: function (data) {
                    return data ? escapeHtml(data) : '<span class="text-muted">--</span>';
                }
            },
            {
                data: 'numberType',
                name: 'NumberType',
                orderable: true,
                searchable: false,
                render: function (data) {
                    var name = typeNames[data] || 'Unknown';
                    var color = typeColors[data] || 'secondary';
                    return createBadge(name, color);
                }
            },
            {
                data: 'capabilities',
                name: 'Capabilities',
                orderable: true,
                searchable: false,
                render: function (data) {
                    var name = capabilityNames[data] || 'Unknown';
                    var color = capabilityColors[data] || 'secondary';
                    return createBadge(name, color);
                }
            },
            {
                data: 'status',
                name: 'Status',
                orderable: true,
                searchable: false,
                render: function (data) {
                    var name = statusNames[data] || 'Unknown';
                    var color = statusColors[data] || 'secondary';
                    return createBadge(name, color);
                }
            },
            {
                data: 'assignedToUserName',
                name: 'AssignedTo',
                orderable: true,
                searchable: true,
                render: function (data) {
                    return data ? escapeHtml(data) : '<span class="text-muted">Unassigned</span>';
                }
            },
            {
                data: 'monthlyRate',
                name: 'MonthlyRate',
                orderable: true,
                searchable: false,
                render: function (data) {
                    if (data === null || data === undefined) return '<span class="text-muted">--</span>';
                    return '$' + parseFloat(data).toFixed(2);
                }
            },
            {
                data: 'id',
                name: 'Actions',
                orderable: false,
                searchable: false,
                className: 'no-export text-end',
                render: function (data, type, row) {
                    var buttons = '<div class="btn-group btn-group-sm">';
                    buttons += '<button class="btn btn-outline-primary" onclick="editNumber(' + data + ')" title="Edit"><i class="bi bi-pencil"></i></button>';

                    // Show Assign button only for Available/Active numbers and only for Admin/SuperAdmin
                    if ((row.status === 0 || row.status === 1) && typeof isAdminOrSuper !== 'undefined' && isAdminOrSuper) {
                        buttons += '<a class="btn btn-outline-info" href="/PhoneNumbers/Assign" title="Assign"><i class="bi bi-person-plus"></i></a>';
                    }

                    // Show Release button only for Active numbers
                    if (row.status === 1) {
                        buttons += '<button class="btn btn-outline-warning" onclick="releaseNumber(' + data + ')" title="Release"><i class="bi bi-unlock"></i></button>';
                    }

                    buttons += '<button class="btn btn-outline-danger" onclick="deleteNumber(' + data + ')" title="Delete"><i class="bi bi-trash"></i></button>';
                    buttons += '</div>';
                    return buttons;
                }
            }
        ],

        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[0, 'asc']],

        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
            '<"row"<"col-sm-12"tr>>' +
            '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',

        language: {
            processing: '<div class="spinner-border text-primary"></div>',
            emptyTable: '<div class="text-center py-3"><i class="bi bi-telephone-x"></i> No phone numbers found</div>',
            info: 'Showing _START_ to _END_ of _TOTAL_ numbers'
        },

        stateSave: true,
        searchDelay: 300
    });
}

function addNumber() {
    var number = document.getElementById('addNumber').value.trim();
    if (!number) {
        showNotification('Phone number is required', 'error');
        return;
    }

    var formData = {
        number: number,
        friendlyName: document.getElementById('addFriendlyName').value.trim() || null,
        numberType: parseInt(document.getElementById('addNumberType').value),
        capabilities: parseInt(document.getElementById('addCapabilities').value),
        monthlyRate: parseFloat(document.getElementById('addRate').value) || 0,
        country: document.getElementById('addCountry').value.trim() || 'US',
        region: document.getElementById('addRegion').value.trim() || null,
        notes: document.getElementById('addNotes').value.trim() || null
    };

    $.ajax({
        url: '/PhoneNumbers/AddNumber',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (response) {
            if (response.success) {
                showNotification(response.message || 'Phone number added successfully!', 'success');
                $('#addNumberModal').modal('hide');
                // Reset fields
                document.getElementById('addNumber').value = '';
                document.getElementById('addFriendlyName').value = '';
                document.getElementById('addNumberType').value = '0';
                document.getElementById('addCapabilities').value = '0';
                document.getElementById('addRate').value = '1.00';
                document.getElementById('addCountry').value = 'US';
                document.getElementById('addRegion').value = '';
                document.getElementById('addNotes').value = '';
                phoneNumbersTable.ajax.reload(null, false);
            } else {
                showNotification(response.message || 'Failed to add phone number', 'error');
            }
        },
        error: function (xhr) {
            var msg = xhr.responseJSON?.message || 'Failed to add phone number';
            showNotification(msg, 'error');
        }
    });
}

function editNumber(id) {
    // Fetch current data from the table row
    var rowData = phoneNumbersTable.rows().data().toArray().find(function (r) { return r.id === id; });

    if (rowData) {
        document.getElementById('editId').value = rowData.id;
        document.getElementById('editFriendlyName').value = rowData.friendlyName || '';
        document.getElementById('editCapabilities').value = rowData.capabilities;
        document.getElementById('editNotes').value = rowData.notes || '';
        $('#editNumberModal').modal('show');
    } else {
        showNotification('Could not find phone number data', 'error');
    }
}

function saveEdit() {
    var id = document.getElementById('editId').value;

    var formData = {
        friendlyName: document.getElementById('editFriendlyName').value.trim(),
        capabilities: parseInt(document.getElementById('editCapabilities').value),
        notes: document.getElementById('editNotes').value.trim() || null
    };

    $.ajax({
        url: '/PhoneNumbers/UpdateNumber?id=' + id,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (response) {
            if (response.success) {
                showNotification(response.message || 'Phone number updated successfully!', 'success');
                $('#editNumberModal').modal('hide');
                phoneNumbersTable.ajax.reload(null, false);
            } else {
                showNotification(response.message || 'Failed to update phone number', 'error');
            }
        },
        error: function (xhr) {
            var msg = xhr.responseJSON?.message || 'Failed to update phone number';
            showNotification(msg, 'error');
        }
    });
}

function deleteNumber(id) {
    confirmAction('Are you sure you want to delete this phone number? This action cannot be undone.', function () {
        $.ajax({
            url: '/PhoneNumbers/Delete?id=' + id,
            method: 'POST',
            success: function (response) {
                if (response.success) {
                    showNotification(response.message || 'Phone number deleted successfully!', 'success');
                    phoneNumbersTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to delete phone number', 'error');
                }
            },
            error: function () {
                showNotification('Failed to delete phone number', 'error');
            }
        });
    });
}

function releaseNumber(id) {
    confirmAction('Are you sure you want to release this phone number? It will no longer be active.', function () {
        $.ajax({
            url: '/PhoneNumbers/ReleaseNumber?id=' + id,
            method: 'POST',
            success: function (response) {
                if (response.success) {
                    showNotification(response.message || 'Phone number released successfully!', 'success');
                    phoneNumbersTable.ajax.reload(null, false);
                } else {
                    showNotification(response.message || 'Failed to release phone number', 'error');
                }
            },
            error: function () {
                showNotification('Failed to release phone number', 'error');
            }
        });
    });
}
