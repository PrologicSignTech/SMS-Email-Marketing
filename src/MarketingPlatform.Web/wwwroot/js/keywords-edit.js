// Keywords Edit Page
const keywordId = window.location.pathname.split('/').pop();

document.addEventListener('DOMContentLoaded', function() {
    loadKeyword();
    setupEventListeners();
});

function setupEventListeners() {
    // Keyword input validation
    const keywordInput = document.getElementById('keyword');
    if (keywordInput) {
        keywordInput.addEventListener('input', function(e) {
            this.value = this.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
        });
    }

    // Auto-response character counter
    const autoResponse = document.getElementById('autoResponse');
    if (autoResponse) {
        autoResponse.addEventListener('input', updateCharCount);
    }

    // Form submission
    const form = document.getElementById('keywordForm');
    if (form) {
        form.addEventListener('submit', handleSubmit);
    }

    // Delete button
    const deleteBtn = document.querySelector('[data-action="delete-keyword"]');
    if (deleteBtn) {
        deleteBtn.addEventListener('click', deleteKeyword);
    }

    // Preview button
    const previewBtn = document.querySelector('[data-action="preview-keyword"]');
    if (previewBtn) {
        previewBtn.addEventListener('click', previewKeyword);
    }
}

async function loadKeyword() {
    const loadingEl = document.getElementById('loading');
    const formEl = document.getElementById('keywordForm');
    
    try {
        const response = await fetch(window.AppUrls.buildApiUrl(window.AppUrls.api.keywords.get(keywordId)), {
            method: 'GET',
            headers: getAjaxHeaders()
        });
        
        if (!response.ok) {
            throw new Error('Failed to load keyword');
        }
        
        const result = await response.json();
        const keyword = result.data || result;
        
        populateForm(keyword);
        updateStats(keyword.stats || {});
        renderRecentActivity(keyword.recentActivity || []);
        
        if (loadingEl) loadingEl.style.display = 'none';
        if (formEl) formEl.style.display = 'block';
    } catch (error) {
        console.error('Error loading keyword:', error);
        if (loadingEl) {
            loadingEl.innerHTML = `<div class="alert alert-danger">Error loading keyword: ${error.message}</div>`;
        }
    }
}

function populateForm(keyword) {
    const fields = [
        { id: 'keyword', value: keyword.keywordText || keyword.keyword },
        { id: 'shortCode', value: keyword.shortCode },
        { id: 'campaignId', value: keyword.campaignId },
        { id: 'autoResponse', value: keyword.responseMessage || keyword.autoResponseMessage },
        { id: 'tags', value: keyword.tags }
    ];
    
    fields.forEach(field => {
        const element = document.getElementById(field.id);
        if (element) element.value = field.value || '';
    });
    
    const doubleOptInEl = document.getElementById('doubleOptIn');
    if (doubleOptInEl) doubleOptInEl.checked = !!keyword.requireDoubleOptIn;
    
    const trackClicksEl = document.getElementById('trackClicks');
    if (trackClicksEl) trackClicksEl.checked = keyword.trackClicks === undefined ? true : !!keyword.trackClicks;
    
    const isActiveEl = document.getElementById('isActive');
    if (isActiveEl) isActiveEl.checked = keyword.isActive === undefined ? true : !!keyword.isActive;
    
    updateCharCount();
}

function updateStats(stats) {
    if (!stats) return;
    
    const statElements = [
        { id: 'statTotal', value: stats.totalMessages },
        { id: 'statOptIns', value: stats.optIns },
        { id: 'statOptOuts', value: stats.optOuts },
        { id: 'statRate', value: stats.successRate }
    ];
    
    statElements.forEach(({ id, value }) => {
        const element = document.getElementById(id);
        if (element && value !== undefined) {
            element.textContent = typeof value === 'number' ? value.toLocaleString() : value;
        }
    });
}

function renderRecentActivity(activities) {
    const container = document.getElementById('recentActivity');
    if (!activities || activities.length === 0) {
        container.innerHTML = '<p class="text-muted small mb-0">No recent activity</p>';
        return;
    }
    
    let html = '<div class="list-group list-group-flush">';
    activities.forEach(activity => {
        const iconClass = activity.event.includes('opt-in') ? 'text-success' : 'text-danger';
        const icon = activity.event.includes('opt-in') ? 'plus-circle' : 'dash-circle';
        html += `
            <div class="list-group-item px-0">
                <div class="d-flex">
                    <i class="bi bi-${icon} ${iconClass} me-2"></i>
                    <div class="flex-grow-1">
                        <small class="d-block">${activity.event}</small>
                        <small class="text-muted">${activity.phone}</small>
                    </div>
                    <small class="text-muted">${activity.date}</small>
                </div>
            </div>
        `;
    });
    html += '</div>';
    container.innerHTML = html;
}

function updateCharCount() {
    const autoResponseEl = document.getElementById('autoResponse');
    const counterEl = document.getElementById('charCount');
    
    if (!autoResponseEl || !counterEl) return;
    
    const count = autoResponseEl.value.length;
    counterEl.textContent = `${count}/160 characters`;
    counterEl.classList.toggle('text-danger', count > 160);
}

function previewKeyword() {
    const shortCodeEl = document.getElementById('shortCode');
    const messageEl = document.getElementById('autoResponse');
    
    const shortCode = shortCodeEl ? shortCodeEl.value : '';
    const message = messageEl ? messageEl.value : '';
    
    if (!shortCode || !message) {
        showNotification('Please fill in short code and auto-response message to preview.', 'warning');
        return;
    }
    
    const previewShortCodeEl = document.getElementById('previewShortCode');
    if (previewShortCodeEl) previewShortCodeEl.textContent = shortCode;
    
    const previewMessageEl = document.getElementById('previewMessage');
    if (previewMessageEl) previewMessageEl.textContent = message;
    
    const modalElement = document.getElementById('previewModal');
    if (modalElement) {
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
}

async function deleteKeyword() {
    if (!confirm('Are you sure you want to delete this keyword? This action cannot be undone.')) return;
    
    try {
        const response = await fetch(window.AppUrls.buildApiUrl(window.AppUrls.api.keywords.delete(keywordId)), {
            method: 'DELETE',
            headers: getAjaxHeaders()
        });
        
        const result = await response.json();
        
        if (response.ok && (result.success || result.isSuccess)) {
            showNotification('Keyword deleted successfully!', 'success');
            setTimeout(() => {
                window.location.href = window.AppUrls.keywords?.index || '/Keywords/Index';
            }, 1000);
        } else {
            showNotification('Failed to delete keyword: ' + (result.message || 'Unknown error'), 'error');
        }
    } catch (error) {
        showNotification('Failed to delete keyword: ' + error.message, 'error');
    }
}

async function handleSubmit(e) {
    e.preventDefault();
    
    const autoResponseEl = document.getElementById('autoResponse');
    if (autoResponseEl && autoResponseEl.value.length > 160) {
        showNotification('Auto-response message exceeds 160 characters.', 'error');
        return;
    }
    
    const data = {
        id: keywordId,
        keywordText: document.getElementById('keyword')?.value,
        shortCode: document.getElementById('shortCode')?.value,
        campaignId: document.getElementById('campaignId')?.value || null,
        responseMessage: autoResponseEl?.value,
        requireDoubleOptIn: document.getElementById('doubleOptIn')?.checked || false,
        trackClicks: document.getElementById('trackClicks')?.checked || false,
        tags: document.getElementById('tags')?.value || '',
        isActive: document.getElementById('isActive')?.checked || false
    };
    
    try {
        const response = await fetch(window.AppUrls.buildApiUrl(window.AppUrls.api.keywords.update(keywordId)), {
            method: 'PUT',
            headers: getAjaxHeaders(),
            body: JSON.stringify(data)
        });
        
        const result = await response.json();
        
        if (response.ok && (result.success || result.isSuccess)) {
            showNotification('Keyword updated successfully!', 'success');
            setTimeout(() => {
                window.location.href = window.AppUrls.keywords?.index || '/Keywords/Index';
            }, 1000);
        } else {
            showNotification('Failed to update keyword: ' + (result.message || 'Unknown error'), 'error');
        }
    } catch (error) {
        showNotification('Failed to update keyword: ' + error.message, 'error');
    }
}
