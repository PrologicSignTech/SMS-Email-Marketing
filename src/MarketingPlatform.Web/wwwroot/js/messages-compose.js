// Messages Compose Page
document.addEventListener('DOMContentLoaded', function() {
    setupChannelSwitch();
    setupSchedulingToggle();
    setupCharCounter();
    setupRecipientsCalculator();
    setupFormSubmission();
    setupTokenButtons();
    setupActionButtons();
});

function setupChannelSwitch() {
    document.querySelectorAll('input[name="channel"]').forEach(radio => {
        radio.addEventListener('change', function() {
            const channel = this.value;
            document.getElementById('emailFields').style.display = channel === 'Email' ? 'block' : 'none';
            document.getElementById('htmlEditor').style.display = channel === 'Email' ? 'block' : 'none';
            document.getElementById('smsFields').style.display = (channel === 'SMS' || channel === 'MMS') ? 'block' : 'none';
            document.getElementById('mmsFields').style.display = channel === 'MMS' ? 'block' : 'none';
            
            const bodyHelp = document.getElementById('bodyHelp');
            if (channel === 'SMS') {
                bodyHelp.textContent = 'SMS message (160 characters recommended)';
            } else if (channel === 'MMS') {
                bodyHelp.textContent = 'MMS message with media attachment';
            } else {
                bodyHelp.textContent = 'Email message content (plain text)';
            }
        });
    });
}

function setupSchedulingToggle() {
    document.querySelectorAll('input[name="scheduling"]').forEach(radio => {
        radio.addEventListener('change', function() {
            document.getElementById('schedulingFields').style.display = 
                this.value === 'later' ? 'block' : 'none';
        });
    });
}

function setupCharCounter() {
    const messageBody = document.getElementById('messageBody');
    if (messageBody) {
        messageBody.addEventListener('input', function() {
            const count = this.value.length;
            document.getElementById('charCounter').textContent = `${count} characters`;
        });
    }
}

function setupRecipientsCalculator() {
    const recipients = document.getElementById('recipients');
    if (recipients) {
        recipients.addEventListener('change', function() {
            const selected = Array.from(this.selectedOptions);
            let total = 0;
            selected.forEach(option => {
                const match = option.text.match(/\(([0-9,]+)\)/);
                if (match) {
                    total += parseInt(match[1].replace(/,/g, ''));
                }
            });
            document.querySelector('#estimatedReach h3').textContent = total.toLocaleString();
            checkCompliance(total);
        });
    }
}

function checkCompliance(recipientCount) {
    const complianceDiv = document.getElementById('complianceCheck');
    let html = '';
    
    if (recipientCount > 0) {
        html += '<div class="alert alert-success alert-sm p-2 mb-1"><small><i class="bi bi-check-circle"></i> Recipients verified</small></div>';
        html += '<div class="alert alert-success alert-sm p-2 mb-1"><small><i class="bi bi-check-circle"></i> Opt-in consent checked</small></div>';
        html += '<div class="alert alert-success alert-sm p-2 mb-0"><small><i class="bi bi-check-circle"></i> Unsubscribe link present</small></div>';
    } else {
        html = '<p class="text-muted small mb-0">Select recipients to check compliance</p>';
    }
    
    complianceDiv.innerHTML = html;
}

function setupTokenButtons() {
    // Token badges click handlers
    const tokenBadges = document.querySelectorAll('[data-token]');
    tokenBadges.forEach(badge => {
        badge.addEventListener('click', function() {
            insertTokenValue(this.dataset.token);
        });
    });
}

function setupActionButtons() {
    // Load template button
    const loadTemplateBtn = document.querySelector('[data-action="load-template"]');
    if (loadTemplateBtn) {
        loadTemplateBtn.addEventListener('click', loadTemplate);
    }

    // Insert token button
    const insertTokenBtn = document.querySelector('[data-action="insert-token"]');
    if (insertTokenBtn) {
        insertTokenBtn.addEventListener('click', insertToken);
    }

    // Toggle editor buttons
    const editorToggles = document.querySelectorAll('[data-editor-mode]');
    editorToggles.forEach(btn => {
        btn.addEventListener('click', function() {
            toggleEditor(this.dataset.editorMode);
        });
    });

    // Save draft button
    const saveDraftBtn = document.querySelector('[data-action="save-draft"]');
    if (saveDraftBtn) {
        saveDraftBtn.addEventListener('click', saveDraft);
    }

    // Preview button
    const previewBtn = document.querySelector('[data-action="preview-message"]');
    if (previewBtn) {
        previewBtn.addEventListener('click', previewMessage);
    }
}

function setupFormSubmission() {
    const form = document.getElementById('messageForm');
    if (form) {
        form.addEventListener('submit', handleSubmit);
    }
}

function insertTokenValue(token) {
    const textarea = document.getElementById('messageBody');
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const text = textarea.value;
    textarea.value = text.substring(0, start) + token + text.substring(end);
    textarea.focus();
    textarea.setSelectionRange(start + token.length, start + token.length);
}

function loadTemplate() {
    showNotification('Template selector would open here (Demo)', 'info');
}

function insertToken() {
    showNotification('Token selector would open here (Demo)', 'info');
}

function toggleEditor(mode) {
    document.querySelectorAll('[data-editor-mode]').forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');
}

function saveDraft() {
    showNotification('Message saved as draft! (Demo)', 'success');
}

function previewMessage() {
    window.location.href = '/Messages/Preview';
}

async function handleSubmit(e) {
    e.preventDefault();
    
    const channel = document.querySelector('input[name="channel"]:checked').value;
    const recipients = document.getElementById('recipients').selectedOptions.length;
    
    if (recipients === 0) {
        showNotification('Please select at least one recipient group.', 'warning');
        return;
    }
    
    if (channel === 'Email' && !document.getElementById('subject').value) {
        showNotification('Please enter an email subject.', 'warning');
        return;
    }
    
    if (!document.getElementById('messageBody').value) {
        showNotification('Please enter message content.', 'warning');
        return;
    }
    
    const scheduling = document.querySelector('input[name="scheduling"]:checked').value;
    if (scheduling === 'later') {
        if (confirm('Schedule this message for delivery?')) {
            showNotification('Message scheduled successfully! (Demo)', 'success');
            setTimeout(() => {
                window.location.href = '/Messages/Index';
            }, 1000);
        }
    } else {
        if (confirm('Send this message immediately?')) {
            showNotification('Message sent successfully! (Demo)', 'success');
            setTimeout(() => {
                window.location.href = '/Messages/Index';
            }, 1000);
        }
    }
}
