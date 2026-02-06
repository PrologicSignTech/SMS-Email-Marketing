/**
 * CSP-Compliant Event Handlers
 * All inline onclick, onchange, etc. handlers are externalized here
 * Uses data attributes and event delegation for better security
 */

(function() {
    'use strict';

    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function() {
        initializeEventHandlers();
    });

    function initializeEventHandlers() {
        // Use event delegation on document for dynamic elements
        document.addEventListener('click', handleClickEvents);
        document.addEventListener('change', handleChangeEvents);
        document.addEventListener('input', handleInputEvents);
        document.addEventListener('submit', handleSubmitEvents);
    }

    /**
     * Central click event handler using data attributes
     */
    function handleClickEvents(event) {
        const target = event.target.closest('[data-action]');
        if (!target) return;

        const action = target.getAttribute('data-action');
        const params = target.getAttribute('data-params');
        
        // Parse params if provided
        let parsedParams = {};
        if (params) {
            try {
                parsedParams = JSON.parse(params);
            } catch (e) {
                console.error('Invalid JSON in data-params:', e);
            }
        }

        // Route to appropriate handler
        switch (action) {
            // Campaign actions
            case 'start-campaign':
                startCampaign(parsedParams.id);
                break;
            case 'pause-campaign':
                pauseCampaign(parsedParams.id);
                break;
            case 'delete-campaign':
                deleteCampaign(parsedParams.id);
                break;
            case 'view-campaign-stats':
                viewCampaignStats(parsedParams.id);
                break;

            // Contact actions
            case 'delete-contact':
                deleteContact(parsedParams.id);
                break;
            case 'add-to-group':
                addToGroup(parsedParams.contactId, parsedParams.groupId);
                break;
            case 'remove-from-group':
                removeFromGroup(parsedParams.contactId, parsedParams.groupId);
                break;
            case 'view-contact-details':
                viewContactDetails(parsedParams.id);
                break;

            // Template actions
            case 'load-template':
                loadTemplate(parsedParams.id);
                break;
            case 'insert-token':
                insertToken(parsedParams.token);
                break;
            case 'insert-token-value':
                insertTokenValue(parsedParams.value);
                break;
            case 'preview-template':
                previewTemplate(parsedParams.id);
                break;
            case 'delete-template':
                deleteTemplate(parsedParams.id);
                break;

            // Message actions
            case 'send-message':
                sendMessage(parsedParams.id);
                break;
            case 'save-draft':
                saveDraft();
                break;
            case 'toggle-editor':
                toggleEditor(parsedParams.mode);
                break;
            case 'view-message-details':
                viewMessageDetails(parsedParams.id);
                break;

            // User/Role actions
            case 'delete-user':
                deleteUser(parsedParams.id);
                break;
            case 'edit-user':
                editUser(parsedParams.id);
                break;
            case 'delete-role':
                deleteRole(parsedParams.id);
                break;
            case 'edit-role':
                editRole(parsedParams.id);
                break;
            case 'toggle-permission':
                togglePermission(parsedParams.roleId, parsedParams.permission);
                break;

            // Keyword actions
            case 'delete-keyword':
                deleteKeyword(parsedParams.id);
                break;
            case 'edit-keyword':
                editKeyword(parsedParams.id);
                break;
            case 'view-keyword-analytics':
                viewKeywordAnalytics(parsedParams.id);
                break;

            // Webhook actions
            case 'delete-webhook':
                deleteWebhook(parsedParams.id);
                break;
            case 'test-webhook':
                testWebhook(parsedParams.id);
                break;
            case 'view-webhook-logs':
                viewWebhookLogs(parsedParams.id);
                break;

            // Workflow actions
            case 'delete-workflow':
                deleteWorkflow(parsedParams.id);
                break;
            case 'execute-workflow':
                executeWorkflow(parsedParams.id);
                break;
            case 'add-workflow-step':
                addWorkflowStep();
                break;
            case 'remove-workflow-step':
                removeWorkflowStep(parsedParams.index);
                break;

            // Pricing actions
            case 'delete-pricing-tier':
                deletePricingTier(parsedParams.id);
                break;
            case 'edit-pricing-tier':
                editPricingTier(parsedParams.id);
                break;

            // Provider actions
            case 'delete-provider':
                deleteProvider(parsedParams.id);
                break;
            case 'test-provider':
                testProvider(parsedParams.id);
                break;
            case 'toggle-provider':
                toggleProvider(parsedParams.id);
                break;

            // Suppression list actions
            case 'delete-suppression':
                deleteSuppression(parsedParams.id);
                break;
            case 'import-suppression':
                importSuppression();
                break;

            // URL actions
            case 'delete-url':
                deleteUrl(parsedParams.id);
                break;
            case 'copy-url':
                copyUrl(parsedParams.url);
                break;
            case 'view-url-analytics':
                viewUrlAnalytics(parsedParams.id);
                break;

            // Billing actions
            case 'subscribe-plan':
                subscribePlan(parsedParams.planId);
                break;
            case 'cancel-subscription':
                cancelSubscription();
                break;

            // Analytics actions
            case 'export-report':
                exportReport(parsedParams.format);
                break;
            case 'refresh-analytics':
                refreshAnalytics();
                break;

            // Super Admin actions
            case 'update-platform-config':
                updatePlatformConfig();
                break;
            case 'view-audit-log':
                viewAuditLog(parsedParams.id);
                break;

            // Landing page config actions
            case 'add-menu-item':
                addMenuItem();
                break;
            case 'remove-menu-item':
                removeMenuItem(parsedParams.index);
                break;
            case 'add-feature':
                addFeature();
                break;
            case 'remove-feature':
                removeFeature(parsedParams.index);
                break;
            case 'preview-landing-page':
                previewLandingPage();
                break;

            // Generic actions
            case 'confirm-delete':
                if (confirm(parsedParams.message || 'Are you sure you want to delete this item?')) {
                    if (parsedParams.url) {
                        window.location.href = parsedParams.url;
                    } else if (parsedParams.formId) {
                        document.getElementById(parsedParams.formId).submit();
                    }
                }
                break;

            case 'toggle-visibility':
                toggleVisibility(parsedParams.targetId);
                break;

            case 'copy-to-clipboard':
                copyToClipboard(parsedParams.text);
                break;

            default:
                console.warn('Unknown action:', action);
        }
    }

    /**
     * Central change event handler
     */
    function handleChangeEvents(event) {
        const target = event.target;
        
        // Channel type change
        if (target.id === 'channelType' || target.name === 'channelType') {
            handleChannelTypeChange(target.value);
        }

        // Schedule type change
        if (target.id === 'scheduleType' || target.name === 'scheduleType') {
            handleScheduleTypeChange(target.value);
        }

        // Provider type change
        if (target.id === 'providerType' || target.name === 'providerType') {
            handleProviderTypeChange(target.value);
        }

        // Keyword type change
        if (target.id === 'keywordType' || target.name === 'keywordType') {
            handleKeywordTypeChange(target.value);
        }

        // Workflow trigger change
        if (target.id === 'triggerType' || target.name === 'triggerType') {
            handleTriggerTypeChange(target.value);
        }

        // Custom data-onchange attribute
        if (target.hasAttribute('data-onchange')) {
            const action = target.getAttribute('data-onchange');
            executeCustomHandler(action, target);
        }
    }

    /**
     * Central input event handler
     */
    function handleInputEvents(event) {
        const target = event.target;

        // Real-time validation or updates
        if (target.hasAttribute('data-oninput')) {
            const action = target.getAttribute('data-oninput');
            executeCustomHandler(action, target);
        }
    }

    /**
     * Central submit event handler
     */
    function handleSubmitEvents(event) {
        const form = event.target;

        // Form-specific validation or handling
        if (form.hasAttribute('data-onsubmit')) {
            const action = form.getAttribute('data-onsubmit');
            const result = executeCustomHandler(action, form);
            if (result === false) {
                event.preventDefault();
            }
        }
    }

    // ========== Action Implementations ==========

    function startCampaign(id) {
        if (confirm('Are you sure you want to start this campaign?')) {
            fetch(AppUrls.campaigns?.start ? AppUrls.campaigns.start(id) : `/Campaigns/Start/${id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Campaign started successfully');
                    location.reload();
                } else {
                    alert('Failed to start campaign: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred while starting the campaign');
            });
        }
    }

    function pauseCampaign(id) {
        if (confirm('Are you sure you want to pause this campaign?')) {
            fetch(AppUrls.campaigns?.pause ? AppUrls.campaigns.pause(id) : `/Campaigns/Pause/${id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Campaign paused successfully');
                    location.reload();
                } else {
                    alert('Failed to pause campaign: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred while pausing the campaign');
            });
        }
    }

    function deleteCampaign(id) {
        if (confirm('Are you sure you want to delete this campaign? This action cannot be undone.')) {
            window.location.href = AppUrls.campaigns?.delete ? AppUrls.campaigns.delete(id) : `/Campaigns/Delete/${id}`;
        }
    }

    function viewCampaignStats(id) {
        window.location.href = AppUrls.analytics?.campaigns ? `${AppUrls.analytics.campaigns}?campaignId=${id}` : `/Analytics/Campaigns?campaignId=${id}`;
    }

    function deleteContact(id) {
        if (confirm('Are you sure you want to delete this contact?')) {
            window.location.href = AppUrls.contacts?.delete ? AppUrls.contacts.delete(id) : `/Contacts/Delete/${id}`;
        }
    }

    function addToGroup(contactId, groupId) {
        fetch(AppUrls.contacts?.addToGroup || `/Contacts/AddToGroup`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ contactId, groupId })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Contact added to group');
                location.reload();
            } else {
                alert('Failed to add contact to group');
            }
        });
    }

    function removeFromGroup(contactId, groupId) {
        if (confirm('Remove this contact from the group?')) {
            fetch(AppUrls.contacts?.removeFromGroup || `/Contacts/RemoveFromGroup`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ contactId, groupId })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Contact removed from group');
                    location.reload();
                } else {
                    alert('Failed to remove contact from group');
                }
            });
        }
    }

    function viewContactDetails(id) {
        window.location.href = AppUrls.contacts?.details ? AppUrls.contacts.details(id) : `/Contacts/Details/${id}`;
    }

    function loadTemplate(id) {
        if (id) {
            fetch(AppUrls.templates?.get ? AppUrls.templates.get(id) : `/Templates/Get/${id}`)
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        document.getElementById('content').value = data.template.content;
                        alert('Template loaded successfully');
                    }
                });
        }
    }

    function insertToken(token) {
        const contentField = document.getElementById('content') || document.getElementById('messageContent');
        if (contentField) {
            const cursorPos = contentField.selectionStart;
            const textBefore = contentField.value.substring(0, cursorPos);
            const textAfter = contentField.value.substring(cursorPos);
            contentField.value = textBefore + token + textAfter;
            contentField.focus();
            contentField.setSelectionRange(cursorPos + token.length, cursorPos + token.length);
        }
    }

    function insertTokenValue(value) {
        insertToken(value);
    }

    function previewTemplate(id) {
        window.open(`/Templates/Preview/${id}`, '_blank');
    }

    function deleteTemplate(id) {
        if (confirm('Are you sure you want to delete this template?')) {
            window.location.href = AppUrls.templates?.delete ? AppUrls.templates.delete(id) : `/Templates/Delete/${id}`;
        }
    }

    function sendMessage(id) {
        if (confirm('Send this message now?')) {
            fetch(AppUrls.messages?.send ? AppUrls.messages.send(id) : `/Messages/Send/${id}`, {
                method: 'POST'
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Message sent successfully');
                    location.reload();
                } else {
                    alert('Failed to send message: ' + data.message);
                }
            });
        }
    }

    function saveDraft() {
        const form = document.querySelector('form');
        if (form) {
            const formData = new FormData(form);
            formData.append('isDraft', 'true');
            
            fetch(form.action, {
                method: 'POST',
                body: formData
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Draft saved successfully');
                } else {
                    alert('Failed to save draft');
                }
            });
        }
    }

    function toggleEditor(mode) {
        const textEditor = document.getElementById('textEditor');
        const htmlEditor = document.getElementById('htmlEditor');
        
        if (mode === 'text') {
            if (textEditor) textEditor.style.display = 'block';
            if (htmlEditor) htmlEditor.style.display = 'none';
        } else if (mode === 'html') {
            if (textEditor) textEditor.style.display = 'none';
            if (htmlEditor) htmlEditor.style.display = 'block';
        }
    }

    function viewMessageDetails(id) {
        window.location.href = AppUrls.messages?.details ? AppUrls.messages.details(id) : `/Messages/Details/${id}`;
    }

    function deleteUser(id) {
        if (confirm('Are you sure you want to delete this user?')) {
            window.location.href = AppUrls.users?.delete ? AppUrls.users.delete(id) : `/Users/Delete/${id}`;
        }
    }

    function editUser(id) {
        window.location.href = AppUrls.users?.edit ? AppUrls.users.edit(id) : `/Users/Edit/${id}`;
    }

    function deleteRole(id) {
        if (confirm('Are you sure you want to delete this role?')) {
            window.location.href = AppUrls.roles?.delete ? AppUrls.roles.delete(id) : `/Roles/Delete/${id}`;
        }
    }

    function editRole(id) {
        window.location.href = AppUrls.roles?.edit ? AppUrls.roles.edit(id) : `/Roles/Edit/${id}`;
    }

    function togglePermission(roleId, permission) {
        // Get the checkbox from the current click event context
        const checkboxes = document.querySelectorAll(`input[data-role-id="${roleId}"][data-permission="${permission}"]`);
        const checkbox = checkboxes.length > 0 ? checkboxes[0] : null;
        
        if (!checkbox) {
            console.error('Checkbox not found for role and permission');
            return;
        }
        
        fetch(AppUrls.roles?.togglePermission || `/Roles/TogglePermission`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                roleId: roleId,
                permission: permission,
                enabled: checkbox.checked
            })
        })
        .then(response => response.json())
        .then(data => {
            if (!data.success) {
                checkbox.checked = !checkbox.checked;
                alert('Failed to update permission');
            }
        });
    }

    function deleteKeyword(id) {
        if (confirm('Are you sure you want to delete this keyword?')) {
            window.location.href = AppUrls.keywords?.delete ? AppUrls.keywords.delete(id) : `/Keywords/Delete/${id}`;
        }
    }

    function editKeyword(id) {
        window.location.href = AppUrls.keywords?.edit ? AppUrls.keywords.edit(id) : `/Keywords/Edit/${id}`;
    }

    function viewKeywordAnalytics(id) {
        window.location.href = AppUrls.keywords?.analytics ? AppUrls.keywords.analytics(id) : `/Keywords/Analytics/${id}`;
    }

    function deleteWebhook(id) {
        if (confirm('Are you sure you want to delete this webhook?')) {
            window.location.href = AppUrls.webhooks?.delete ? AppUrls.webhooks.delete(id) : `/Webhooks/Delete/${id}`;
        }
    }

    function testWebhook(id) {
        fetch(AppUrls.webhooks?.test ? AppUrls.webhooks.test(id) : `/Webhooks/Test/${id}`, {
            method: 'POST'
        })
        .then(response => response.json())
        .then(data => {
            alert(data.success ? 'Webhook test successful' : 'Webhook test failed: ' + data.message);
        });
    }

    function viewWebhookLogs(id) {
        window.location.href = AppUrls.webhooks?.logs ? AppUrls.webhooks.logs(id) : `/Webhooks/Logs/${id}`;
    }

    function deleteWorkflow(id) {
        if (confirm('Are you sure you want to delete this workflow?')) {
            window.location.href = AppUrls.workflows?.delete ? AppUrls.workflows.delete(id) : `/Workflows/Delete/${id}`;
        }
    }

    function executeWorkflow(id) {
        if (confirm('Execute this workflow now?')) {
            fetch(AppUrls.workflows?.execute ? AppUrls.workflows.execute(id) : `/Workflows/Execute/${id}`, {
                method: 'POST'
            })
            .then(response => response.json())
            .then(data => {
                alert(data.success ? 'Workflow executed successfully' : 'Workflow execution failed');
            });
        }
    }

    function addWorkflowStep() {
        // Implementation depends on specific workflow UI
        console.log('Add workflow step');
    }

    function removeWorkflowStep(index) {
        if (confirm('Remove this workflow step?')) {
            const stepElement = document.querySelector(`[data-step-index="${index}"]`);
            if (stepElement) {
                stepElement.remove();
            }
        }
    }

    function deletePricingTier(id) {
        if (confirm('Are you sure you want to delete this pricing tier?')) {
            window.location.href = AppUrls.pricing?.delete ? AppUrls.pricing.delete(id) : `/Pricing/Delete/${id}`;
        }
    }

    function editPricingTier(id) {
        window.location.href = AppUrls.pricing?.edit ? AppUrls.pricing.edit(id) : `/Pricing/Edit/${id}`;
    }

    function deleteProvider(id) {
        if (confirm('Are you sure you want to delete this provider?')) {
            window.location.href = AppUrls.providers?.delete ? AppUrls.providers.delete(id) : `/Providers/Delete/${id}`;
        }
    }

    function testProvider(id) {
        fetch(AppUrls.providers?.test ? AppUrls.providers.test(id) : `/Providers/Test/${id}`, {
            method: 'POST'
        })
        .then(response => response.json())
        .then(data => {
            alert(data.success ? 'Provider test successful' : 'Provider test failed: ' + data.message);
        });
    }

    function toggleProvider(id) {
        fetch(AppUrls.providers?.toggle ? AppUrls.providers.toggle(id) : `/Providers/Toggle/${id}`, {
            method: 'POST'
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                location.reload();
            } else {
                alert('Failed to toggle provider');
            }
        });
    }

    function deleteSuppression(id) {
        if (confirm('Are you sure you want to delete this suppression entry?')) {
            window.location.href = AppUrls.suppression?.delete ? AppUrls.suppression.delete(id) : `/Suppression/Delete/${id}`;
        }
    }

    function importSuppression() {
        document.getElementById('importFile').click();
    }

    function deleteUrl(id) {
        if (confirm('Are you sure you want to delete this URL?')) {
            window.location.href = AppUrls.urls?.delete ? AppUrls.urls.delete(id) : `/Urls/Delete/${id}`;
        }
    }

    function copyUrl(url) {
        navigator.clipboard.writeText(url).then(() => {
            alert('URL copied to clipboard');
        });
    }

    function viewUrlAnalytics(id) {
        window.location.href = AppUrls.urls?.analytics ? AppUrls.urls.analytics(id) : `/Urls/Analytics/${id}`;
    }

    function subscribePlan(planId) {
        window.location.href = AppUrls.billing?.subscribe ? AppUrls.billing.subscribe(planId) : `/Billing/Subscribe/${planId}`;
    }

    function cancelSubscription() {
        if (confirm('Are you sure you want to cancel your subscription?')) {
            window.location.href = AppUrls.billing?.cancel || `/Billing/Cancel`;
        }
    }

    function exportReport(format) {
        const params = new URLSearchParams(window.location.search);
        params.set('format', format);
        window.location.href = AppUrls.analytics?.export ? `${AppUrls.analytics.export}?${params.toString()}` : `/Analytics/Export?${params.toString()}`;
    }

    function refreshAnalytics() {
        location.reload();
    }

    function updatePlatformConfig() {
        const form = document.getElementById('platformConfigForm');
        if (form) {
            form.submit();
        }
    }

    function viewAuditLog(id) {
        window.location.href = AppUrls.superAdmin?.auditLog ? AppUrls.superAdmin.auditLog(id) : `/SuperAdmin/AuditLog/${id}`;
    }

    function addMenuItem() {
        // Implementation for adding menu items dynamically
        console.log('Add menu item');
    }

    function removeMenuItem(index) {
        const menuItem = document.querySelector(`[data-menu-index="${index}"]`);
        if (menuItem && confirm('Remove this menu item?')) {
            menuItem.remove();
        }
    }

    function addFeature() {
        // Implementation for adding features dynamically
        console.log('Add feature');
    }

    function removeFeature(index) {
        const feature = document.querySelector(`[data-feature-index="${index}"]`);
        if (feature && confirm('Remove this feature?')) {
            feature.remove();
        }
    }

    function previewLandingPage() {
        window.open('/LandingPageConfig/Preview', '_blank');
    }

    function toggleVisibility(targetId) {
        const element = document.getElementById(targetId);
        if (element) {
            element.style.display = element.style.display === 'none' ? 'block' : 'none';
        }
    }

    function copyToClipboard(text) {
        navigator.clipboard.writeText(text).then(() => {
            alert('Copied to clipboard');
        }).catch(err => {
            console.error('Failed to copy:', err);
        });
    }

    // ========== Conditional Field Handlers ==========

    function handleChannelTypeChange(value) {
        const smsFields = document.getElementById('smsFields');
        const mmsFields = document.getElementById('mmsFields');
        const emailFields = document.getElementById('emailFields');

        if (smsFields) smsFields.style.display = value === 'SMS' || value === '0' ? 'block' : 'none';
        if (mmsFields) mmsFields.style.display = value === 'MMS' || value === '1' ? 'block' : 'none';
        if (emailFields) emailFields.style.display = value === 'Email' || value === '2' ? 'block' : 'none';
    }

    function handleScheduleTypeChange(value) {
        const schedulingFields = document.getElementById('schedulingFields');
        if (schedulingFields) {
            schedulingFields.style.display = value === 'Scheduled' ? 'block' : 'none';
        }
    }

    function handleProviderTypeChange(value) {
        // Show/hide provider-specific fields
        const allProviderFields = document.querySelectorAll('[data-provider-type]');
        allProviderFields.forEach(field => {
            field.style.display = 'none';
        });

        const specificFields = document.querySelectorAll(`[data-provider-type="${value}"]`);
        specificFields.forEach(field => {
            field.style.display = 'block';
        });
    }

    function handleKeywordTypeChange(value) {
        const autoReplyFields = document.getElementById('autoReplyFields');
        const forwardFields = document.getElementById('forwardFields');

        if (autoReplyFields) autoReplyFields.style.display = value === 'AutoReply' ? 'block' : 'none';
        if (forwardFields) forwardFields.style.display = value === 'Forward' ? 'block' : 'none';
    }

    function handleTriggerTypeChange(value) {
        const triggerSpecificFields = document.querySelectorAll('[data-trigger-type]');
        triggerSpecificFields.forEach(field => {
            field.style.display = 'none';
        });

        const specificFields = document.querySelectorAll(`[data-trigger-type="${value}"]`);
        specificFields.forEach(field => {
            field.style.display = 'block';
        });
    }

    function executeCustomHandler(action, element) {
        // Execute custom handlers defined in page-specific scripts
        if (typeof window[action] === 'function') {
            return window[action](element);
        }
        return true;
    }

    // Expose utility functions globally if needed
    window.CSPHandlers = {
        insertToken: insertToken,
        insertTokenValue: insertTokenValue,
        toggleVisibility: toggleVisibility,
        copyToClipboard: copyToClipboard,
        loadTemplate: loadTemplate,
        toggleEditor: toggleEditor,
        saveDraft: saveDraft
    };

})();
