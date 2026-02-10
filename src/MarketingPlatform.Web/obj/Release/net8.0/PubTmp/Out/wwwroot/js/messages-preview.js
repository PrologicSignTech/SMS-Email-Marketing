/**
 * Messages Preview Page JavaScript
 * Handles message preview switching between mobile and desktop views
 * and test message functionality
 */

document.addEventListener('DOMContentLoaded', function() {
    loadPreviewData();
    setupEventListeners();
});

/**
 * Setup event listeners for preview controls and test form
 */
function setupEventListeners() {
    // Preview mode switching buttons
    const previewButtons = document.querySelectorAll('[data-preview-mode]');
    previewButtons.forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const mode = this.dataset.previewMode;
            switchPreview(mode);
        });
    });

    // Test form submission
    const testForm = document.getElementById('testForm');
    if (testForm) {
        testForm.addEventListener('submit', handleTestFormSubmit);
    }
}

/**
 * Load preview data and populate UI
 */
function loadPreviewData() {
    const mockData = {
        channel: 'SMS',
        from: 'Your Company',
        sender: '+1 (234) 567-890',
        subject: 'Flash Sale Alert',
        content: 'FLASH SALE! Get 50% off today only. Use code FLASH50 at checkout. Shop now: https://example.com/sale\n\nReply STOP to opt-out.',
        emailFrom: 'sales@yourcompany.com',
        recipients: '1,245 recipients'
    };
    
    document.getElementById('previewFrom').textContent = mockData.from;
    document.getElementById('previewSender').textContent = mockData.sender;
    document.getElementById('previewContent').innerHTML = '<p class="mb-0">' + mockData.content.replace(/\n/g, '<br>') + '</p>';
    document.getElementById('previewSubject').textContent = mockData.subject;
    document.getElementById('previewEmailFrom').textContent = mockData.emailFrom;
    document.getElementById('previewRecipients').textContent = mockData.recipients;
    document.getElementById('previewEmailContent').innerHTML = '<p>' + mockData.content.replace(/\n/g, '<br>') + '</p>';
}

/**
 * Switch between mobile and desktop preview modes
 */
function switchPreview(mode) {
    // Update active button states
    const buttons = document.querySelectorAll('[data-preview-mode]');
    buttons.forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.previewMode === mode) {
            btn.classList.add('active');
        }
    });
    
    // Toggle preview display
    if (mode === 'mobile') {
        document.getElementById('mobilePreview').style.display = 'block';
        document.getElementById('desktopPreview').style.display = 'none';
    } else {
        document.getElementById('mobilePreview').style.display = 'none';
        document.getElementById('desktopPreview').style.display = 'block';
    }
}

/**
 * Handle test form submission
 */
function handleTestFormSubmit(e) {
    e.preventDefault();
    
    const recipient = document.getElementById('testRecipient').value;
    if (!recipient) {
        showNotification('Please enter a test recipient.', 'warning');
        return;
    }
    
    showNotification(`Test message sent to ${recipient}! (Demo)`, 'success');
    document.getElementById('check5').checked = true;
    document.getElementById('testForm').reset();
}
