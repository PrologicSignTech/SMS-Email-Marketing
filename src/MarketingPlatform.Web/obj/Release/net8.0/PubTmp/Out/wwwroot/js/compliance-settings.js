/**
 * Compliance Settings - SERVER-SIDE API INTEGRATION
 * Property names match UpdateComplianceSettingsDto exactly
 */

$(document).ready(function () {
    loadSettings();
    checkQuietHours();
});

/**
 * Load current compliance settings
 */
function loadSettings() {
    $.ajax({
        url: '/Compliance/GetSettings',
        method: 'GET',
        success: function (response) {
            document.getElementById('settingsLoading').classList.add('d-none');
            document.getElementById('settingsForm').classList.remove('d-none');

            if (response.success && response.data) {
                populateSettings(response.data);
            }
        },
        error: function () {
            document.getElementById('settingsLoading').innerHTML =
                '<div class="text-danger"><i class="bi bi-exclamation-triangle"></i> Failed to load settings</div>';
        }
    });
}

/**
 * Populate form with settings data
 */
function populateSettings(data) {
    // Double Opt-In settings
    setChecked('requireDoubleOptIn', data.requireDoubleOptIn || data.RequireDoubleOptIn || false);
    setChecked('requireDoubleOptInSms', data.requireDoubleOptInSms || data.RequireDoubleOptInSms || false);
    setChecked('requireDoubleOptInEmail', data.requireDoubleOptInEmail || data.RequireDoubleOptInEmail || false);

    // Compliance features
    setChecked('enableAuditLogging', data.enableAuditLogging || data.EnableAuditLogging || true);
    setChecked('enforceSuppressionList', data.enforceSuppressionList || data.EnforceSuppressionList || false);
    setChecked('enableConsentTracking', data.enableConsentTracking || data.EnableConsentTracking || false);
    setValue('consentRetentionDays', data.consentRetentionDays || data.ConsentRetentionDays || 365);

    // Quiet hours
    setChecked('enableQuietHours', data.enableQuietHours || data.EnableQuietHours || false);

    // Parse TimeSpan values (could come as "21:00:00" or "21:00")
    var qhStart = data.quietHoursStart || data.QuietHoursStart || '21:00';
    var qhEnd = data.quietHoursEnd || data.QuietHoursEnd || '08:00';
    setValue('quietHoursStart', parseTimeForInput(qhStart));
    setValue('quietHoursEnd', parseTimeForInput(qhEnd));
    setValue('quietHoursTimeZone', data.quietHoursTimeZone || data.QuietHoursTimeZone || 'America/New_York');

    // Auto-response keywords
    var optOut = data.optOutKeywords || data.OptOutKeywords || '';
    var optIn = data.optInKeywords || data.OptInKeywords || '';
    setValue('optOutKeywords', Array.isArray(optOut) ? optOut.join(', ') : optOut);
    setValue('optInKeywords', Array.isArray(optIn) ? optIn.join(', ') : optIn);

    // Confirmation messages
    setValue('optOutConfirmationMessage', data.optOutConfirmationMessage || data.OptOutConfirmationMessage || '');
    setValue('optInConfirmationMessage', data.optInConfirmationMessage || data.OptInConfirmationMessage || '');

    // Company info
    setValue('companyName', data.companyName || data.CompanyName || '');
    setValue('companyAddress', data.companyAddress || data.CompanyAddress || '');
    setValue('privacyPolicyUrl', data.privacyPolicyUrl || data.PrivacyPolicyUrl || '');
    setValue('termsOfServiceUrl', data.termsOfServiceUrl || data.TermsOfServiceUrl || '');
}

/**
 * Parse TimeSpan string to HH:mm format for input[type=time]
 */
function parseTimeForInput(value) {
    if (!value) return '00:00';
    var str = String(value);
    // Handle "21:00:00" or "21:00" or "PT21H" etc.
    var parts = str.split(':');
    if (parts.length >= 2) {
        return parts[0].padStart(2, '0') + ':' + parts[1].padStart(2, '0');
    }
    return str;
}

/**
 * Save compliance settings - matches UpdateComplianceSettingsDto exactly
 */
function saveSettings() {
    var btn = document.getElementById('saveSettingsBtn');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Saving...';

    // Build TimeSpan strings from time inputs (HH:mm -> HH:mm:00)
    var startTime = document.getElementById('quietHoursStart').value || '21:00';
    var endTime = document.getElementById('quietHoursEnd').value || '08:00';

    var settings = {
        // Double Opt-In
        requireDoubleOptIn: document.getElementById('requireDoubleOptIn').checked,
        requireDoubleOptInSms: document.getElementById('requireDoubleOptInSms').checked,
        requireDoubleOptInEmail: document.getElementById('requireDoubleOptInEmail').checked,

        // Quiet Hours
        enableQuietHours: document.getElementById('enableQuietHours').checked,
        quietHoursStart: startTime + ':00',
        quietHoursEnd: endTime + ':00',
        quietHoursTimeZone: document.getElementById('quietHoursTimeZone').value,

        // Company Information
        companyName: document.getElementById('companyName').value,
        companyAddress: document.getElementById('companyAddress').value,
        privacyPolicyUrl: document.getElementById('privacyPolicyUrl').value,
        termsOfServiceUrl: document.getElementById('termsOfServiceUrl').value,

        // Auto-response Settings
        optOutKeywords: document.getElementById('optOutKeywords').value,
        optInKeywords: document.getElementById('optInKeywords').value,
        optOutConfirmationMessage: document.getElementById('optOutConfirmationMessage').value,
        optInConfirmationMessage: document.getElementById('optInConfirmationMessage').value,

        // Compliance Features
        enforceSuppressionList: document.getElementById('enforceSuppressionList').checked,
        enableConsentTracking: document.getElementById('enableConsentTracking').checked,
        enableAuditLogging: document.getElementById('enableAuditLogging').checked,
        consentRetentionDays: parseInt(document.getElementById('consentRetentionDays').value) || 365
    };

    $.ajax({
        url: '/Compliance/UpdateSettings',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(settings),
        success: function (response) {
            if (response.success) {
                showNotification('Compliance settings saved successfully!', 'success');
            } else {
                showNotification(response.message || 'Failed to save settings', 'error');
            }
        },
        error: function (xhr) {
            var msg = 'An error occurred while saving settings';
            try {
                var errData = JSON.parse(xhr.responseText);
                if (errData.message) msg = errData.message;
            } catch (e) { }
            showNotification(msg, 'error');
        },
        complete: function () {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-check-lg"></i> Save Settings';
        }
    });
}

/**
 * Check quiet hours status
 */
function checkQuietHours() {
    $.ajax({
        url: '/Compliance/CheckQuietHours',
        method: 'GET',
        success: function (response) {
            var statusEl = document.getElementById('quietHoursStatusText');
            if (response.success && response.data) {
                var data = response.data;
                var isQuiet = data.isQuietHours || data.IsQuietHours || false;
                if (isQuiet) {
                    statusEl.textContent = 'Quiet hours are currently ACTIVE. Messages will be queued.';
                    document.getElementById('quietHoursStatus').className = 'alert alert-warning';
                } else {
                    statusEl.textContent = 'Quiet hours are not active. Messages can be sent now.';
                    document.getElementById('quietHoursStatus').className = 'alert alert-success';
                }
            } else {
                statusEl.textContent = 'Unable to determine quiet hours status.';
            }
        },
        error: function () {
            document.getElementById('quietHoursStatusText').textContent = 'Could not check quiet hours status.';
        }
    });
}

// Helper functions
function setChecked(id, value) {
    var el = document.getElementById(id);
    if (el) el.checked = !!value;
}

function setValue(id, value) {
    var el = document.getElementById(id);
    if (el) el.value = value || '';
}
