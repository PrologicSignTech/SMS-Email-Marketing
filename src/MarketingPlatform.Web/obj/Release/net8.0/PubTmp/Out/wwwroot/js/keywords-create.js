/**
 * Keywords Create Page - Server-Side API Integration
 * Calls Web controller endpoints, NOT API directly
 */

document.addEventListener('DOMContentLoaded', function() {
    loadPhoneNumbers();
    loadCampaigns();
    setupEventListeners();
});

function loadPhoneNumbers() {
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
                select.appendChild(opt);
            });
            if (items.length === 0) {
                select.innerHTML = '<option value="">No phone numbers available</option>';
            }
        }
    });
}

function loadCampaigns() {
    $.get('/Keywords/GetCampaigns', function(response) {
        var select = document.getElementById('campaignId');
        select.innerHTML = '<option value="">No Campaign (Optional)</option>';
        if (response.success && response.items) {
            var items = Array.isArray(response.items) ? response.items : [];
            items.forEach(function(c) {
                var opt = document.createElement('option');
                opt.value = c.id || '';
                opt.textContent = c.name || 'Campaign #' + c.id;
                select.appendChild(opt);
            });
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

    var previewBtn = document.getElementById('previewBtn');
    if (previewBtn) {
        previewBtn.addEventListener('click', function(e) {
            e.preventDefault();
            previewKeyword();
        });
    }

    var form = document.getElementById('keywordForm');
    if (form) {
        form.addEventListener('submit', handleFormSubmit);
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
        showNotification('Please select a phone number and enter auto-response message.', 'warning');
        return;
    }
    var el1 = document.getElementById('previewShortCode');
    if (el1) el1.textContent = shortCode;
    var el2 = document.getElementById('previewMessage');
    if (el2) el2.textContent = message;
    var modal = new bootstrap.Modal(document.getElementById('previewModal'));
    modal.show();
}

function handleFormSubmit(e) {
    e.preventDefault();
    var keyword = document.getElementById('keyword').value.trim();
    if (!keyword || keyword.length < 2) {
        showNotification('Keyword must be at least 2 characters.', 'error');
        return;
    }

    var submitBtn = e.target.querySelector('button[type="submit"]');
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Creating...';

    var data = {
        keywordText: keyword,
        description: 'SMS keyword: ' + keyword,
        responseMessage: document.getElementById('autoResponse').value,
        linkedCampaignId: document.getElementById('campaignId').value ? parseInt(document.getElementById('campaignId').value) : null,
        optInGroupId: null
    };

    $.ajax({
        url: '/Keywords/CreateKeyword',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function(response) {
            if (response.success) {
                showNotification('Keyword created successfully!', 'success');
                setTimeout(function() { window.location.href = '/Keywords/Index'; }, 1000);
            } else {
                showNotification(response.message || 'Failed to create keyword', 'error');
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Create Keyword';
            }
        },
        error: function() {
            showNotification('An error occurred', 'error');
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Create Keyword';
        }
    });
}
