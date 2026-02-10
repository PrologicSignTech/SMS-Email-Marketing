/**
 * Messages Compose Page - Real API Integration
 * Loads groups as recipients, templates, sends messages via API
 */

let recipientGroups = [];

document.addEventListener('DOMContentLoaded', function() {
    loadRecipientGroups();
    loadPhoneNumbers();
    setupChannelSwitch();
    setupSchedulingToggle();
    setupCharCounter();
    setupTokenButtons();
    setupLoadTemplate();
    setupFormSubmission();
    setupRecipientsCalculator();
});

function loadRecipientGroups() {
    fetch('/Messages/GetRecipientGroups')
        .then(r => r.json())
        .then(result => {
            const select = document.getElementById('recipients');
            select.innerHTML = '';

            let groups = [];
            const data = result.data;
            if (data) {
                if (Array.isArray(data)) {
                    groups = data;
                } else if (data.items && Array.isArray(data.items)) {
                    groups = data.items;
                }
            }
            recipientGroups = groups;

            if (groups.length === 0) {
                select.innerHTML = '<option disabled>No groups found. Create groups first.</option>';
                return;
            }

            groups.forEach(g => {
                const opt = document.createElement('option');
                opt.value = g.id;
                opt.textContent = `${g.name || 'Unnamed'} (${g.contactCount || 0} contacts)`;
                opt.dataset.count = g.contactCount || 0;
                select.appendChild(opt);
            });
        })
        .catch(err => {
            console.error('Error loading groups:', err);
            document.getElementById('recipients').innerHTML = '<option disabled>Failed to load groups</option>';
        });
}

function loadPhoneNumbers() {
    fetch('/Messages/GetMyPhoneNumbers')
        .then(r => r.json())
        .then(result => {
            const select = document.getElementById('fromPhone');
            select.innerHTML = '';

            let numbers = [];
            if (result.data) {
                if (Array.isArray(result.data)) numbers = result.data;
                else if (result.data.items) numbers = result.data.items;
            }

            if (numbers.length === 0) {
                select.innerHTML = '<option value="">No numbers assigned - Buy or get one assigned</option>';
                return;
            }

            select.innerHTML = '<option value="">Select a number</option>';
            var capNames = ['SMS', 'MMS', 'Both'];
            numbers.forEach(n => {
                var opt = document.createElement('option');
                opt.value = n.number;
                opt.textContent = n.number + (n.friendlyName ? ' - ' + n.friendlyName : '') + ' (' + (capNames[n.capabilities] || 'SMS') + ')';
                select.appendChild(opt);
            });
        })
        .catch(err => {
            console.error('Error loading phone numbers:', err);
            document.getElementById('fromPhone').innerHTML = '<option value="">Failed to load numbers</option>';
        });
}

function setupChannelSwitch() {
    document.querySelectorAll('input[name="channel"]').forEach(radio => {
        radio.addEventListener('change', function() {
            const val = parseInt(this.value);
            // 0=SMS, 1=MMS, 2=Email
            document.getElementById('emailFields').style.display = val === 2 ? 'block' : 'none';
            document.getElementById('htmlEditor').style.display = val === 2 ? 'block' : 'none';
            document.getElementById('smsFields').style.display = (val === 0 || val === 1) ? 'block' : 'none';
            document.getElementById('mmsFields').style.display = val === 1 ? 'block' : 'none';

            const bodyHelp = document.getElementById('bodyHelp');
            if (val === 0) bodyHelp.textContent = 'SMS message (160 characters recommended)';
            else if (val === 1) bodyHelp.textContent = 'MMS message with media attachment';
            else bodyHelp.textContent = 'Email message content (plain text)';
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
            document.getElementById('charCounter').textContent = this.value.length + ' characters';
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
                total += parseInt(option.dataset.count || 0);
            });
            document.getElementById('reachCount').textContent = total.toLocaleString();
        });
    }
}

function setupTokenButtons() {
    document.querySelectorAll('.token-badge').forEach(badge => {
        badge.addEventListener('click', function() {
            const textarea = document.getElementById('messageBody');
            const start = textarea.selectionStart;
            const text = textarea.value;
            const token = this.dataset.token;
            textarea.value = text.substring(0, start) + token + text.substring(textarea.selectionEnd);
            textarea.focus();
            textarea.setSelectionRange(start + token.length, start + token.length);
            document.getElementById('charCounter').textContent = textarea.value.length + ' characters';
        });
    });
}

function setupLoadTemplate() {
    const btn = document.getElementById('loadTemplateBtn');
    if (btn) {
        btn.addEventListener('click', function() {
            const channelRadio = document.querySelector('input[name="channel"]:checked');
            const channel = channelRadio ? parseInt(channelRadio.value) : null;

            const templateListDiv = document.getElementById('templateList');
            const listItems = document.getElementById('templateListItems');
            listItems.innerHTML = '<span class="text-muted p-3 d-block">Loading templates...</span>';
            templateListDiv.style.display = '';

            const url = channel !== null
                ? `/Messages/GetTemplatesForCompose?channel=${channel}`
                : '/Messages/GetTemplatesForCompose';

            fetch(url)
                .then(r => r.json())
                .then(result => {
                    let templates = [];
                    const data = result.data;
                    if (data) {
                        if (Array.isArray(data)) templates = data;
                        else if (data.items && Array.isArray(data.items)) templates = data.items;
                    }

                    if (templates.length === 0) {
                        listItems.innerHTML = '<span class="text-muted p-3 d-block">No templates found</span>';
                        return;
                    }

                    listItems.innerHTML = '';
                    templates.forEach(tpl => {
                        const channelNames = ['SMS', 'MMS', 'Email'];
                        const item = document.createElement('a');
                        item.href = '#';
                        item.className = 'list-group-item list-group-item-action py-2 px-3';
                        item.innerHTML = `<strong>${escapeHtml(tpl.name)}</strong>
                            <span class="badge bg-secondary ms-1">${channelNames[tpl.channel] || ''}</span>
                            <br><small class="text-muted">${escapeHtml((tpl.messageBody || '').substring(0, 60))}...</small>`;
                        item.addEventListener('click', function(e) {
                            e.preventDefault();
                            document.getElementById('messageBody').value = tpl.messageBody || '';
                            if (tpl.subject) document.getElementById('subject').value = tpl.subject;
                            if (tpl.htmlContent) document.getElementById('htmlContent').value = tpl.htmlContent;
                            document.getElementById('charCounter').textContent = (tpl.messageBody || '').length + ' characters';
                            templateListDiv.style.display = 'none';
                            showNotification('Template loaded!', 'success');
                        });
                        listItems.appendChild(item);
                    });
                })
                .catch(err => {
                    console.error('Error loading templates:', err);
                    listItems.innerHTML = '<span class="text-danger p-3 d-block">Failed to load templates</span>';
                });
        });
    }
}

function setupFormSubmission() {
    const form = document.getElementById('messageForm');
    if (form) {
        form.addEventListener('submit', handleSubmit);
    }
}

async function handleSubmit(e) {
    e.preventDefault();

    const channelRadio = document.querySelector('input[name="channel"]:checked');
    if (!channelRadio) {
        showNotification('Please select a channel', 'error');
        return;
    }
    const channel = parseInt(channelRadio.value);

    const selectedRecipients = Array.from(document.getElementById('recipients').selectedOptions);
    if (selectedRecipients.length === 0) {
        showNotification('Please select at least one recipient group', 'error');
        return;
    }

    const messageBody = document.getElementById('messageBody').value.trim();
    if (!messageBody) {
        showNotification('Please enter message content', 'error');
        return;
    }

    if (channel === 2 && !document.getElementById('subject').value.trim()) {
        showNotification('Please enter an email subject', 'error');
        return;
    }

    const scheduling = document.querySelector('input[name="scheduling"]:checked').value;
    let scheduledAt = null;
    if (scheduling === 'later') {
        scheduledAt = document.getElementById('scheduledTime').value;
        if (!scheduledAt) {
            showNotification('Please select a scheduled date/time', 'error');
            return;
        }
    }

    const action = scheduling === 'later' ? 'schedule' : 'send immediately';
    if (!confirm(`${scheduling === 'later' ? 'Schedule' : 'Send'} this message to the selected groups?`)) return;

    const btn = document.getElementById('sendBtn');
    btn.disabled = true;
    btn.innerHTML = '<i class="spinner-border spinner-border-sm me-1"></i>Sending...';

    // Get contact IDs from selected groups
    const groupIds = selectedRecipients.map(opt => parseInt(opt.value));
    let allContactIds = [];

    try {
        // Fetch contacts for each selected group via Web controller
        for (const gid of groupIds) {
            try {
                const resp = await fetch(`/Messages/GetGroupContacts?groupId=${gid}`);
                const result = await resp.json();
                const data = result.data;
                let items = [];
                if (data && data.items) items = data.items;
                else if (Array.isArray(data)) items = data;

                items.forEach(c => {
                    if (c.id && !allContactIds.includes(c.id)) {
                        allContactIds.push(c.id);
                    }
                });
            } catch (err) {
                console.error(`Failed to fetch group ${gid} contacts:`, err);
            }
        }

        if (allContactIds.length === 0) {
            showNotification('No contacts found in selected groups', 'error');
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-send"></i> Send Message';
            return;
        }

        const messageData = {
            campaignId: 0,
            contactIds: allContactIds,
            channel: channel,
            subject: channel === 2 ? document.getElementById('subject').value.trim() : null,
            messageBody: messageBody,
            htmlContent: channel === 2 ? (document.getElementById('htmlContent').value.trim() || null) : null,
            mediaUrls: channel === 1 && document.getElementById('mediaUrl').value.trim()
                ? [document.getElementById('mediaUrl').value.trim()] : null,
            scheduledAt: scheduledAt ? new Date(scheduledAt).toISOString() : null
        };

        const response = await fetch('/Messages/SendMessage', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(messageData)
        });

        const result = await response.json();

        if (result.success) {
            showNotification(scheduling === 'later'
                ? 'Message scheduled successfully!'
                : `Message sent to ${allContactIds.length} contacts!`, 'success');
            setTimeout(() => { window.location.href = '/Messages'; }, 1500);
        } else {
            showNotification(result.message || 'Failed to send message', 'error');
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-send"></i> Send Message';
        }
    } catch (error) {
        console.error('Error sending message:', error);
        showNotification('An error occurred while sending', 'error');
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-send"></i> Send Message';
    }
}

function escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}
