/**
 * Keywords Edit Page - Server-Side API Integration
 * Calls Web controller endpoints, NOT API directly
 */

var keywordId = window.location.pathname.split('/').pop();

document.addEventListener('DOMContentLoaded', function() {
    loadKeyword();
    setupEventListeners();
});

function loadPhoneNumbers(selectedValue) {
    $.get('/Keywords/GetPhoneNumbers', function(response) {
        var select = document.getElementById('shortCode');
        select.innerHTML = '<option value="">Select Phone Number</option>';
        if (response.success && response.items) {
            var items = Array.isArray(response.items) ? response.items : [];
            items.forEach(function(num) {
                var opt = document.createElement('option');
                opt.value = num.phoneNumber || num.number || '';
                var label = num.phoneNumber || num.number || '';
                if (num.friendlyName) label += ' (' + num.friendlyName + ')';
                opt.textContent = label;
                if (selectedValue && opt.value === selectedValue) opt.selected = true;
                select.appendChild(opt);
            });
        }
    });
}

function loadCampaigns(selectedValue) {
    $.get('/Keywords/GetCampaigns', function(response) {
        var select = document.getElementById('campaignId');
        select.innerHTML = '<option value="">No Campaign (Optional)</option>';
        if (response.success && response.items) {
            var items = Array.isArray(response.items) ? response.items : [];
            items.forEach(function(c) {
                var opt = document.createElement('option');
                opt.value = c.id || '';
                opt.textContent = c.name || 'Campaign #' + c.id;
                if (selectedValue && opt.value == selectedValue) opt.selected = true;
                select.appendChild(opt);
            });
        }
    });
}

function loadKeyword() {
    $.get('/Keywords/GetKeyword?id=' + keywordId, function(response) {
        if (response.success && response.data) {
            var kw = response.data;
            document.getElementById('keyword').value = kw.keywordText || '';
            document.getElementById('autoResponse').value = kw.responseMessage || '';

            loadPhoneNumbers(kw.shortCode || '');
            loadCampaigns(kw.linkedCampaignId ? kw.linkedCampaignId.toString() : '');

            var isActiveEl = document.getElementById('isActive');
            if (isActiveEl) isActiveEl.checked = kw.isActive !== false;

            var doubleOptInEl = document.getElementById('doubleOptIn');
            if (doubleOptInEl) doubleOptInEl.checked = !!kw.requireDoubleOptIn;

            var trackClicksEl = document.getElementById('trackClicks');
            if (trackClicksEl) trackClicksEl.checked = kw.trackClicks !== false;

            var tagsEl = document.getElementById('tags');
            if (tagsEl) tagsEl.value = kw.tags || '';

            updateCharCount();

            var statTotal = document.getElementById('statTotal');
            if (statTotal) statTotal.textContent = (kw.activityCount || 0).toLocaleString();

            document.getElementById('loading').style.display = 'none';
            document.getElementById('keywordForm').style.display = 'block';
        } else {
            document.getElementById('loading').innerHTML = '<div class="alert alert-danger">Keyword not found</div>';
        }
    });
}

function setupEventListeners() {
    var keywordInput = document.getElementById('keyword');
    if (keywordInput) {
        keywordInput.addEventListener('input', function() {
            this.value = this.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
        });
    }

    var autoResponse = document.getElementById('autoResponse');
    if (autoResponse) {
        autoResponse.addEventListener('input', updateCharCount);
    }

    var form = document.getElementById('keywordForm');
    if (form) {
        form.addEventListener('submit', handleSubmit);
    }

    var deleteBtn = document.querySelector('[data-action="delete-keyword"]');
    if (deleteBtn) {
        deleteBtn.addEventListener('click', handleDelete);
    }

    var previewBtn = document.querySelector('[data-action="preview-keyword"]');
    if (previewBtn) {
        previewBtn.addEventListener('click', previewKeyword);
    }
}

function updateCharCount() {
    var el = document.getElementById('autoResponse');
    var counter = document.getElementById('charCount');
    if (el && counter) {
        var count = el.value.length;
        counter.textContent = count + '/160 characters';
        counter.classList.toggle('text-danger', count > 160);
        counter.classList.toggle('text-muted', count <= 160);
    }
}

function previewKeyword() {
    var shortCode = document.getElementById('shortCode')?.value || '';
    var message = document.getElementById('autoResponse')?.value || '';
    if (!shortCode || !message) {
        showNotification('Please select a number and enter auto-response.', 'warning');
        return;
    }
    var el1 = document.getElementById('previewShortCode');
    if (el1) el1.textContent = shortCode;
    var el2 = document.getElementById('previewMessage');
    if (el2) el2.textContent = message;
    var modal = new bootstrap.Modal(document.getElementById('previewModal'));
    modal.show();
}

function handleDelete() {
    confirmAction('Are you sure you want to delete this keyword?', function() {
        $.post('/Keywords/Delete?id=' + keywordId, function(response) {
            if (response.success) {
                showNotification('Keyword deleted!', 'success');
                setTimeout(function() { window.location.href = '/Keywords/Index'; }, 1000);
            } else {
                showNotification(response.message || 'Failed to delete', 'error');
            }
        });
    });
}

function handleSubmit(e) {
    e.preventDefault();

    var submitBtn = e.target.querySelector('button[type="submit"]');
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Saving...';

    var data = {
        keywordText: document.getElementById('keyword').value.trim(),
        responseMessage: document.getElementById('autoResponse').value,
        linkedCampaignId: document.getElementById('campaignId').value ? parseInt(document.getElementById('campaignId').value) : null,
        isActive: document.getElementById('isActive')?.checked || false
    };

    $.ajax({
        url: '/Keywords/UpdateKeyword?id=' + keywordId,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function(response) {
            if (response.success) {
                showNotification('Keyword updated!', 'success');
                setTimeout(function() { window.location.href = '/Keywords/Index'; }, 1000);
            } else {
                showNotification(response.message || 'Failed to update', 'error');
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Save Changes';
            }
        },
        error: function() {
            showNotification('An error occurred', 'error');
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Save Changes';
        }
    });
}
