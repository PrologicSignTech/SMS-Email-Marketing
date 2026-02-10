// Campaign Creation Form - Multi-step wizard
let currentStep = 1;
const totalSteps = 4;

document.addEventListener('DOMContentLoaded', function() {
    initializeForm();
    setupEventListeners();
});

function initializeForm() {
    // Set default campaign type behavior
    const campaignTypeSelect = document.getElementById('campaignType');
    campaignTypeSelect.addEventListener('change', handleCampaignTypeChange);

    // Initialize character counter
    const messageBody = document.getElementById('messageBody');
    messageBody.addEventListener('input', updateCharacterCount);

    // Initialize target type handlers
    document.querySelectorAll('input[name="targetType"]').forEach(radio => {
        radio.addEventListener('change', handleTargetTypeChange);
    });

    // Initialize schedule type handlers
    document.querySelectorAll('input[name="scheduleType"]').forEach(radio => {
        radio.addEventListener('change', handleScheduleTypeChange);
    });

    // Initialize channel radio handlers
    document.querySelectorAll('input[name="channel"]').forEach(radio => {
        radio.addEventListener('change', function() {
            updateContentFieldsByChannel(this.value);
        });
    });

    // Timezone aware checkbox
    document.getElementById('timeZoneAware').addEventListener('change', function() {
        document.getElementById('timezoneGroup').style.display = this.checked ? 'block' : 'none';
    });

    // Recurrence rule select
    document.getElementById('recurrenceRule').addEventListener('change', function() {
        document.getElementById('customRecurrence').classList.toggle('d-none', this.value !== 'custom');
    });

    // Template select handler - populate message body when template is selected
    const templateSelect = document.getElementById('templateSelect');
    if (templateSelect) {
        templateSelect.addEventListener('change', handleTemplateSelect);
    }

    // Load contact groups and templates from Web Controller
    loadGroups();
    loadTemplates();
}

function setupEventListeners() {
    // Next button
    document.getElementById('nextBtn').addEventListener('click', nextStep);

    // Previous button
    document.getElementById('prevBtn').addEventListener('click', prevStep);

    // Form submission
    document.getElementById('campaignForm').addEventListener('submit', handleSubmit);

    // Save draft button
    document.getElementById('saveDraftBtn').addEventListener('click', saveDraft);

    // Calculate audience button
    document.getElementById('calculateAudienceBtn').addEventListener('click', calculateAudience);

    // Add custom token button
    document.getElementById('addTokenBtn').addEventListener('click', addCustomToken);
}

function handleCampaignTypeChange(e) {
    const campaignType = e.target.value;
    const channelSelection = document.getElementById('channelSelection');

    // Show channel selection for Multi-Channel campaigns
    if (campaignType === '3') {
        channelSelection.style.display = 'block';
        // Auto-select first channel
        document.getElementById('channelSMS').checked = true;
        updateContentFieldsByChannel('0');
    } else {
        channelSelection.style.display = 'none';
        // Update content fields based on campaign type
        updateContentFieldsByChannel(campaignType);
    }
}

function updateContentFieldsByChannel(channelType) {
    const emailSubject = document.getElementById('emailSubjectGroup');
    const htmlContent = document.getElementById('htmlContentGroup');
    const mediaUrls = document.getElementById('mediaUrlsGroup');
    const smsSegments = document.getElementById('smsSegments');

    // Hide all optional fields
    emailSubject.classList.add('d-none');
    htmlContent.classList.add('d-none');
    mediaUrls.classList.add('d-none');
    smsSegments.classList.add('d-none');

    // Show relevant fields based on channel
    switch(channelType) {
        case '0': // SMS
            smsSegments.classList.remove('d-none');
            break;
        case '1': // MMS
            smsSegments.classList.remove('d-none');
            mediaUrls.classList.remove('d-none');
            break;
        case '2': // Email
            emailSubject.classList.remove('d-none');
            htmlContent.classList.remove('d-none');
            break;
    }
}

function updateCharacterCount() {
    const messageBody = document.getElementById('messageBody');
    const charCount = document.getElementById('charCount');
    const segmentCount = document.getElementById('segmentCount');

    const length = messageBody.value.length;
    charCount.textContent = length;

    // Calculate SMS segments (160 chars per segment for standard GSM, 70 for Unicode)
    const segments = Math.ceil(length / 160) || 0;
    segmentCount.textContent = segments;
}

function handleTargetTypeChange(e) {
    const groupSelection = document.getElementById('groupSelection');
    const segmentCriteria = document.getElementById('segmentCriteria');

    groupSelection.classList.add('d-none');
    segmentCriteria.classList.add('d-none');

    if (e.target.value === '1') {
        groupSelection.classList.remove('d-none');
    } else if (e.target.value === '2') {
        segmentCriteria.classList.remove('d-none');
    }
}

function handleScheduleTypeChange(e) {
    const recurrenceGroup = document.getElementById('recurrenceGroup');

    if (e.target.value === '1') { // Recurring
        recurrenceGroup.classList.remove('d-none');
    } else {
        recurrenceGroup.classList.add('d-none');
    }
}

function nextStep() {
    if (validateStep(currentStep)) {
        if (currentStep < totalSteps) {
            showStep(currentStep + 1);
        }
    }
}

function prevStep() {
    if (currentStep > 1) {
        showStep(currentStep - 1);
    }
}

function showStep(step) {
    // Hide all steps
    document.querySelectorAll('.step-content').forEach(content => {
        content.classList.add('d-none');
    });

    // Remove active class from all indicators
    document.querySelectorAll('.step-indicator').forEach(indicator => {
        indicator.classList.remove('active');
    });

    // Show current step
    document.getElementById(`step${step}`).classList.remove('d-none');
    document.querySelector(`.step-indicator[data-step="${step}"]`).classList.add('active');

    // Update navigation buttons
    document.getElementById('prevBtn').style.display = step === 1 ? 'none' : 'block';
    document.getElementById('nextBtn').style.display = step === totalSteps ? 'none' : 'block';
    document.getElementById('saveDraftBtn').classList.toggle('d-none', step !== totalSteps);
    document.getElementById('createBtn').classList.toggle('d-none', step !== totalSteps);

    currentStep = step;

    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function validateStep(step) {
    let isValid = true;
    const errorMessages = [];

    switch(step) {
        case 1: // Basic Info
            const name = document.getElementById('campaignName').value.trim();
            const type = document.getElementById('campaignType').value;

            if (!name) {
                errorMessages.push('Campaign name is required');
                isValid = false;
            }
            if (!type) {
                errorMessages.push('Campaign type is required');
                isValid = false;
            }
            break;

        case 2: // Content
            const messageBody = document.getElementById('messageBody').value.trim();
            const campaignType = document.getElementById('campaignType').value;

            if (!messageBody) {
                errorMessages.push('Message body is required');
                isValid = false;
            }

            // Validate email subject if email campaign
            if (campaignType === '2' || (campaignType === '3' && document.getElementById('channelEmail').checked)) {
                const subject = document.getElementById('emailSubject').value.trim();
                if (!subject) {
                    errorMessages.push('Email subject is required');
                    isValid = false;
                }
            }
            break;

        case 3: // Audience
            const targetType = document.querySelector('input[name="targetType"]:checked').value;

            if (targetType === '1') {
                const selectedGroups = Array.from(document.getElementById('groupSelect').selectedOptions);
                if (selectedGroups.length === 0) {
                    errorMessages.push('Please select at least one group');
                    isValid = false;
                }
            }
            break;

        case 4: // Schedule
            // Schedule validation is optional - can send immediately
            break;
    }

    if (!isValid) {
        showNotification('Validation errors:\n' + errorMessages.join('\n'), 'error');
    }

    return isValid;
}

function handleSubmit(e) {
    e.preventDefault();

    if (!validateStep(currentStep)) {
        return;
    }

    const campaignData = buildCampaignData();
    const createBtn = document.getElementById('createBtn');

    if (createBtn) {
        createBtn.disabled = true;
        createBtn.innerHTML = '<i class="spinner-border spinner-border-sm me-2"></i>Creating...';
    }

    $.ajax({
        url: '/Campaigns/CreateCampaign',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(campaignData),
        success: function(response) {
            if (response.success) {
                showNotification(response.message || 'Campaign created successfully!', 'success');
                setTimeout(function() {
                    window.location.href = '/Campaigns';
                }, 1500);
            } else {
                showNotification(response.message || 'Failed to create campaign', 'error');
                if (createBtn) {
                    createBtn.disabled = false;
                    createBtn.innerHTML = '<i class="bi bi-rocket-takeoff"></i> Create Campaign';
                }
            }
        },
        error: function(xhr) {
            if (xhr.status === 401) {
                showNotification('Session expired. Please log in again.', 'error');
                setTimeout(function() { window.location.href = '/Auth/Login'; }, 2000);
            } else {
                const errorMsg = xhr.responseJSON?.message || 'Failed to create campaign';
                showNotification(errorMsg, 'error');
            }
            if (createBtn) {
                createBtn.disabled = false;
                createBtn.innerHTML = '<i class="bi bi-rocket-takeoff"></i> Create Campaign';
            }
        }
    });
}

function saveDraft() {
    const campaignData = buildCampaignData();
    campaignData.status = 0; // Draft status

    const saveDraftBtn = document.getElementById('saveDraftBtn');
    if (saveDraftBtn) {
        saveDraftBtn.disabled = true;
        saveDraftBtn.innerHTML = '<i class="spinner-border spinner-border-sm me-2"></i>Saving...';
    }

    $.ajax({
        url: '/Campaigns/CreateCampaign',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(campaignData),
        success: function(response) {
            if (response.success) {
                showNotification('Draft saved successfully!', 'success');
                setTimeout(function() { window.location.href = '/Campaigns'; }, 1500);
            } else {
                showNotification(response.message || 'Failed to save draft', 'error');
            }
            if (saveDraftBtn) {
                saveDraftBtn.disabled = false;
                saveDraftBtn.innerHTML = '<i class="bi bi-save"></i> Save Draft';
            }
        },
        error: function(xhr) {
            if (xhr.status === 401) {
                showNotification('Session expired. Please log in again.', 'error');
                setTimeout(function() { window.location.href = '/Auth/Login'; }, 2000);
            } else {
                const errorMsg = xhr.responseJSON?.message || 'Failed to save draft';
                showNotification(errorMsg, 'error');
            }
            if (saveDraftBtn) {
                saveDraftBtn.disabled = false;
                saveDraftBtn.innerHTML = '<i class="bi bi-save"></i> Save Draft';
            }
        }
    });
}

function buildCampaignData() {
    // Get campaign type and determine channel
    const campaignType = parseInt(document.getElementById('campaignType').value);
    let channel = campaignType;

    // For multi-channel, get selected channel
    if (campaignType === 3) {
        const selectedChannel = document.querySelector('input[name="channel"]:checked');
        channel = selectedChannel ? parseInt(selectedChannel.value) : 0;
    }

    // Build personalization tokens
    const personalizationTokens = {};
    const firstName = document.getElementById('tokenFirstName').value.trim();
    const lastName = document.getElementById('tokenLastName').value.trim();

    if (firstName) personalizationTokens.FirstName = firstName;
    if (lastName) personalizationTokens.LastName = lastName;

    // Build content object
    const content = {
        channel: channel,
        messageBody: document.getElementById('messageBody').value.trim(),
        personalizationTokens: personalizationTokens
    };

    // Add template ID if selected
    const templateSelect = document.getElementById('templateSelect');
    if (templateSelect && templateSelect.value) {
        content.templateId = parseInt(templateSelect.value);
    }

    // Add email-specific fields
    if (channel === 2) {
        content.subject = document.getElementById('emailSubject').value.trim();
        const htmlContent = document.getElementById('htmlContent').value.trim();
        if (htmlContent) {
            content.htmlContent = htmlContent;
        }
    }

    // Add MMS media URLs
    if (channel === 1) {
        const mediaUrls = [];
        const url1 = document.getElementById('mediaUrl1').value.trim();
        const url2 = document.getElementById('mediaUrl2').value.trim();
        if (url1) mediaUrls.push(url1);
        if (url2) mediaUrls.push(url2);
        if (mediaUrls.length > 0) {
            content.mediaUrls = mediaUrls;
        }
    }

    // Build audience object
    const targetType = parseInt(document.querySelector('input[name="targetType"]:checked').value);
    const audience = {
        targetType: targetType
    };

    if (targetType === 1) {
        const selectedGroups = Array.from(document.getElementById('groupSelect').selectedOptions)
            .map(option => parseInt(option.value));
        audience.groupIds = selectedGroups;
    } else if (targetType === 2) {
        audience.segmentCriteria = document.getElementById('segmentInput').value.trim();
    }

    // Add exclusion lists if any
    const exclusionLists = Array.from(document.getElementById('exclusionLists').selectedOptions)
        .map(option => parseInt(option.value));
    if (exclusionLists.length > 0) {
        audience.exclusionListIds = exclusionLists;
    }

    // Build schedule object
    const scheduleType = parseInt(document.querySelector('input[name="scheduleType"]:checked').value);
    let schedule = null;

    const scheduleDate = document.getElementById('scheduleDate').value;
    const scheduleTime = document.getElementById('scheduleTime').value;

    if (scheduleDate || scheduleType > 0) {
        schedule = {
            scheduleType: scheduleType,
            timeZoneAware: document.getElementById('timeZoneAware').checked,
            preferredTimeZone: document.getElementById('preferredTimeZone').value
        };

        if (scheduleDate && scheduleTime) {
            schedule.scheduledDate = `${scheduleDate}T${scheduleTime}:00`;
        }

        if (scheduleType === 1) { // Recurring
            const recurrenceRule = document.getElementById('recurrenceRule').value;
            if (recurrenceRule === 'custom') {
                schedule.recurrenceRule = document.getElementById('customRecurrence').value;
            } else {
                schedule.recurrenceRule = recurrenceRule;
            }
        }
    }

    // Build final campaign object
    const campaignData = {
        name: document.getElementById('campaignName').value.trim(),
        description: document.getElementById('campaignDescription').value.trim(),
        type: campaignType,
        content: content,
        audience: audience
    };

    if (schedule) {
        campaignData.schedule = schedule;
    }

    return campaignData;
}

// Load templates from Web Controller
async function loadTemplates() {
    const templateSelect = document.getElementById('templateSelect');
    if (!templateSelect) return;

    try {
        const response = await $.ajax({
            url: '/Campaigns/GetTemplates',
            method: 'GET'
        });

        let templates = [];
        if (response?.success && response.data) {
            if (Array.isArray(response.data)) {
                templates = response.data;
            } else if (response.data.items && Array.isArray(response.data.items)) {
                templates = response.data.items;
            }
        }

        // Keep the "No Template" option and add templates
        templateSelect.innerHTML = '<option value="">No Template</option>';

        if (templates.length > 0) {
            templates.forEach(function(template) {
                const option = document.createElement('option');
                option.value = template.id;
                option.textContent = template.name || template.title || 'Unnamed Template';
                // Store template content as data attribute for quick loading
                if (template.content || template.body || template.messageBody) {
                    option.dataset.content = template.content || template.body || template.messageBody || '';
                }
                if (template.subject) {
                    option.dataset.subject = template.subject;
                }
                templateSelect.appendChild(option);
            });
        }
    } catch (err) {
        console.error('Failed to load templates:', err);
    }
}

// Handle template selection - populate message body
function handleTemplateSelect() {
    const templateSelect = document.getElementById('templateSelect');
    const selectedOption = templateSelect.options[templateSelect.selectedIndex];

    if (selectedOption && selectedOption.value) {
        // Fill message body from template content
        const content = selectedOption.dataset.content;
        if (content) {
            document.getElementById('messageBody').value = content;
            updateCharacterCount();
        }
        // Fill email subject if available
        const subject = selectedOption.dataset.subject;
        if (subject) {
            const emailSubjectField = document.getElementById('emailSubject');
            if (emailSubjectField) {
                emailSubjectField.value = subject;
            }
        }
    }
}

// Load contact groups from Web Controller
async function loadGroups() {
    const groupSelect = document.getElementById('groupSelect');
    const exclusionLists = document.getElementById('exclusionLists');

    groupSelect.innerHTML = '<option value="">Loading groups...</option>';
    exclusionLists.innerHTML = '<option value="">Loading...</option>';

    try {
        const response = await $.ajax({
            url: '/Campaigns/GetContactGroups',
            method: 'GET'
        });

        let groups = [];
        if (response?.success && response.data) {
            if (Array.isArray(response.data)) {
                groups = response.data;
            } else if (response.data.items && Array.isArray(response.data.items)) {
                groups = response.data.items;
            }
        }

        groupSelect.innerHTML = '';
        exclusionLists.innerHTML = '<option value="">No exclusions</option>';

        if (groups.length === 0) {
            groupSelect.innerHTML = '<option value="">No groups available</option>';
        } else {
            groups.forEach(function(group) {
                const option1 = document.createElement('option');
                option1.value = group.id;
                option1.textContent = group.name;
                groupSelect.appendChild(option1);

                const option2 = document.createElement('option');
                option2.value = group.id;
                option2.textContent = group.name;
                exclusionLists.appendChild(option2);
            });
        }
    } catch (err) {
        console.error('Failed to load groups:', err);
        groupSelect.innerHTML = '<option value="">Failed to load groups</option>';
        exclusionLists.innerHTML = '<option value="">No exclusions</option>';
    }
}

async function calculateAudience() {
    const targetType = parseInt(document.querySelector('input[name="targetType"]:checked').value);
    const audienceSizeElement = document.getElementById('audienceSize');

    audienceSizeElement.textContent = 'Calculating...';

    const audienceData = { targetType: targetType };

    if (targetType === 1) {
        audienceData.groupIds = Array.from(document.getElementById('groupSelect').selectedOptions)
            .map(function(option) { return parseInt(option.value); });
    } else if (targetType === 2) {
        audienceData.segmentCriteria = document.getElementById('segmentInput').value.trim();
    }

    try {
        const response = await $.ajax({
            url: '/Campaigns/CalculateAudience',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(audienceData)
        });

        if (response?.success && response.data !== undefined) {
            const size = typeof response.data === 'number' ? response.data : (response.data.count || response.data.size || 0);
            audienceSizeElement.textContent = size.toLocaleString();
        } else {
            audienceSizeElement.textContent = response?.data?.toLocaleString() || '0';
        }
    } catch (err) {
        console.error('Failed to calculate audience:', err);
        // Fallback to client-side estimation
        let estimatedSize = 0;
        if (targetType === 0) {
            estimatedSize = 'All contacts';
        } else if (targetType === 1) {
            const selectedGroups = Array.from(document.getElementById('groupSelect').selectedOptions);
            estimatedSize = selectedGroups.length > 0 ? '~' + (selectedGroups.length * 250) : '0';
        } else {
            estimatedSize = 'N/A';
        }
        audienceSizeElement.textContent = estimatedSize.toLocaleString ? estimatedSize.toLocaleString() : estimatedSize;
    }
}

function addCustomToken() {
    const customTokensDiv = document.getElementById('customTokens');
    const tokenCount = customTokensDiv.children.length + 1;

    const tokenGroup = document.createElement('div');
    tokenGroup.className = 'input-group mb-2 mt-2';
    tokenGroup.innerHTML = `
        <span class="input-group-text">{{</span>
        <input type="text" class="form-control" placeholder="TokenName" id="customTokenName${tokenCount}">
        <span class="input-group-text">}}</span>
        <input type="text" class="form-control" placeholder="Default value" id="customTokenValue${tokenCount}">
        <button class="btn btn-outline-danger" type="button" onclick="this.parentElement.remove()">
            <i class="bi bi-trash"></i>
        </button>
    `;

    customTokensDiv.appendChild(tokenGroup);
}

// Notification helper function for better UX
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; max-width: 400px;';

    // Safely set text content to prevent XSS
    const messageDiv = document.createElement('div');
    messageDiv.textContent = message;
    messageDiv.innerHTML = messageDiv.innerHTML.replace(/\n/g, '<br>');

    const closeButton = document.createElement('button');
    closeButton.type = 'button';
    closeButton.className = 'btn-close';
    closeButton.setAttribute('data-bs-dismiss', 'alert');
    closeButton.setAttribute('aria-label', 'Close');

    notification.appendChild(messageDiv);
    notification.appendChild(closeButton);

    document.body.appendChild(notification);

    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => notification.remove(), 150);
    }, 5000);
}
