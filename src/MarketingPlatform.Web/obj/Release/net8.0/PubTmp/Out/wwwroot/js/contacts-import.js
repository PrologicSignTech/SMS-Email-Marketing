/**
 * Contacts Import - SERVER-SIDE API INTEGRATION
 * Handles CSV and Excel file imports through Web Controller proxy
 */

$(document).ready(function () {
    setupDropZone();
    loadGroupsForImport();
});

/**
 * Setup drag & drop zone
 */
function setupDropZone() {
    var dropZone = document.getElementById('dropZone');
    var fileInput = document.getElementById('importFile');

    if (!dropZone || !fileInput) return;

    // Click to browse
    dropZone.addEventListener('click', function () {
        fileInput.click();
    });

    // File selected
    fileInput.addEventListener('change', function () {
        if (this.files.length > 0) {
            handleFileSelected(this.files[0]);
        }
    });

    // Drag events
    dropZone.addEventListener('dragover', function (e) {
        e.preventDefault();
        e.stopPropagation();
        this.style.borderColor = '#667eea';
        this.style.background = '#eef0ff';
    });

    dropZone.addEventListener('dragleave', function (e) {
        e.preventDefault();
        e.stopPropagation();
        this.style.borderColor = '';
        this.style.background = '#f8f9fa';
    });

    dropZone.addEventListener('drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
        this.style.borderColor = '';
        this.style.background = '#f8f9fa';

        if (e.dataTransfer.files.length > 0) {
            fileInput.files = e.dataTransfer.files;
            handleFileSelected(e.dataTransfer.files[0]);
        }
    });

    // Update accepted file types based on radio
    document.querySelectorAll('input[name="fileType"]').forEach(function (radio) {
        radio.addEventListener('change', function () {
            fileInput.accept = this.value === 'csv' ? '.csv' : '.xlsx,.xls';
        });
    });
}

/**
 * Handle file selection
 */
function handleFileSelected(file) {
    var fileType = document.querySelector('input[name="fileType"]:checked').value;
    var validExtensions = fileType === 'csv' ? ['.csv'] : ['.xlsx', '.xls'];
    var ext = '.' + file.name.split('.').pop().toLowerCase();

    if (!validExtensions.includes(ext)) {
        showNotification('Invalid file type. Please select a ' + fileType.toUpperCase() + ' file.', 'error');
        clearFile();
        return;
    }

    document.getElementById('selectedFile').classList.remove('d-none');
    document.getElementById('fileName').textContent = file.name;
    document.getElementById('fileSize').textContent = '(' + formatFileSize(file.size) + ')';
    document.getElementById('importBtn').disabled = false;

    // Hide results if previously shown
    document.getElementById('importResults').classList.add('d-none');
}

/**
 * Clear selected file
 */
function clearFile() {
    document.getElementById('importFile').value = '';
    document.getElementById('selectedFile').classList.add('d-none');
    document.getElementById('importBtn').disabled = true;
}

/**
 * Format file size
 */
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    var k = 1024;
    var sizes = ['Bytes', 'KB', 'MB', 'GB'];
    var i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * Load groups for dropdown
 */
function loadGroupsForImport() {
    $.get('/Contacts/GetContactGroups', function (response) {
        var select = document.getElementById('importGroupId');
        if (!select) return;

        var items = [];
        if (response.success && response.items) {
            items = Array.isArray(response.items) ? response.items : [];
        }

        items.forEach(function (g) {
            var option = document.createElement('option');
            option.value = g.id;
            option.textContent = g.name + (g.contactCount ? ' (' + g.contactCount + ' contacts)' : '');
            select.appendChild(option);
        });
    });
}

/**
 * Start import process
 */
function startImport() {
    var fileInput = document.getElementById('importFile');
    if (!fileInput.files.length) {
        showNotification('Please select a file first', 'error');
        return;
    }

    var file = fileInput.files[0];
    var fileType = document.querySelector('input[name="fileType"]:checked').value;
    var groupId = document.getElementById('importGroupId').value;

    // Show progress
    document.getElementById('importProgress').classList.remove('d-none');
    document.getElementById('importResults').classList.add('d-none');
    document.getElementById('importBtn').disabled = true;

    // Build FormData
    var formData = new FormData();
    formData.append('file', file);
    if (groupId) {
        formData.append('groupId', groupId);
    }

    var url = fileType === 'csv' ? '/Contacts/ImportCsv' : '/Contacts/ImportExcel';

    $.ajax({
        url: url,
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            document.getElementById('importProgress').classList.add('d-none');

            if (response.success) {
                showImportResults(response.data);
                showNotification(response.message || 'Import completed!', 'success');
            } else {
                showNotification(response.message || 'Import failed', 'error');
            }
        },
        error: function (xhr) {
            document.getElementById('importProgress').classList.add('d-none');
            showNotification('Import failed. Please try again.', 'error');
        },
        complete: function () {
            document.getElementById('importBtn').disabled = false;
        }
    });
}

/**
 * Display import results
 */
function showImportResults(data) {
    if (!data) return;

    document.getElementById('importResults').classList.remove('d-none');

    // Handle both camelCase and PascalCase property names
    var totalRows = data.totalRows || data.TotalRows || 0;
    var successCount = data.successCount || data.SuccessCount || 0;
    var duplicateCount = data.duplicateCount || data.DuplicateCount || 0;
    var failureCount = data.failureCount || data.FailureCount || 0;
    var errors = data.errors || data.Errors || [];

    document.getElementById('resultTotal').textContent = totalRows;
    document.getElementById('resultSuccess').textContent = successCount;
    document.getElementById('resultDuplicates').textContent = duplicateCount;
    document.getElementById('resultErrors').textContent = failureCount;

    // Show error details if any
    if (errors && errors.length > 0) {
        document.getElementById('importErrorDetails').classList.remove('d-none');
        var tbody = document.getElementById('errorTableBody');
        var html = '';
        errors.forEach(function (err, index) {
            html += '<tr><td>' + (index + 1) + '</td><td>' + escapeHtml(typeof err === 'string' ? err : JSON.stringify(err)) + '</td></tr>';
        });
        tbody.innerHTML = html;
    } else {
        document.getElementById('importErrorDetails').classList.add('d-none');
    }
}
