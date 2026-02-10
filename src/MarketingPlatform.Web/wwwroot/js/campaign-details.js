/**
 * Campaign Details Page
 * Loads and displays campaign details from the model passed by the server
 */

const StatusNames = ['Draft', 'Scheduled', 'Running', 'Paused', 'Completed', 'Failed'];
const StatusColors = ['secondary', 'warning', 'primary', 'info', 'success', 'danger'];
const TypeNames = ['SMS', 'MMS', 'Email', 'Multi-Channel'];
const TypeColors = ['primary', 'info', 'success', 'warning'];
const ChannelNames = ['SMS', 'MMS', 'Email'];
const TargetTypeNames = ['All Contacts', 'Specific Groups', 'Segment'];
const ScheduleTypeNames = ['Immediate', 'Recurring'];

document.addEventListener('DOMContentLoaded', function () {
    loadCampaignDetails();
});

function loadCampaignDetails() {
    // The campaign data is rendered as JSON by the Razor page via the model
    // We need to fetch it from the current URL as JSON, or parse the page data
    // Since the controller passes object model to view, let's fetch via API through Web controller
    const pathParts = window.location.pathname.split('/');
    const campaignId = pathParts[pathParts.length - 1];

    if (!campaignId || isNaN(campaignId)) {
        showError('Invalid campaign ID');
        return;
    }

    const token = (typeof getAuthToken === 'function') ? getAuthToken() : (window.authConfig?.token || '');
    const apiUrl = window.AppUrls ? window.AppUrls.buildApiUrl(`/api/campaigns/${campaignId}`) : `/api/campaigns/${campaignId}`;

    $.ajax({
        url: apiUrl,
        method: 'GET',
        headers: {
            'Authorization': token ? 'Bearer ' + token : ''
        },
        success: function (response) {
            let campaign = null;
            if (response?.success && response.data) {
                campaign = response.data;
            } else if (response?.id) {
                campaign = response;
            }

            if (campaign) {
                renderCampaignDetails(campaign);
            } else {
                showError('Campaign not found');
            }
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                showError('Session expired. Please log in again.');
            } else {
                showError('Failed to load campaign details');
            }
        }
    });
}

function renderCampaignDetails(c) {
    // Header
    document.getElementById('campaignSubtitle').textContent = c.description || 'No description';
    document.getElementById('campaignName').textContent = c.name || 'Unnamed Campaign';

    // Status badge
    const statusIdx = c.status ?? 0;
    const statusName = StatusNames[statusIdx] || 'Unknown';
    const statusColor = StatusColors[statusIdx] || 'secondary';
    document.getElementById('campaignStatusBadge').innerHTML =
        `<span class="badge bg-${statusColor} fs-6">${statusName}</span>`;

    // Type badge
    const typeIdx = c.type ?? 0;
    const typeName = TypeNames[typeIdx] || 'Unknown';
    const typeColor = TypeColors[typeIdx] || 'secondary';
    document.getElementById('campaignTypeBadge').innerHTML =
        `<span class="badge bg-${typeColor}">${typeName}</span>`;

    // Stats
    document.getElementById('totalRecipients').textContent = (c.totalRecipients || 0).toLocaleString();
    document.getElementById('successCount').textContent = (c.successCount || 0).toLocaleString();
    document.getElementById('failureCount').textContent = (c.failureCount || 0).toLocaleString();
    document.getElementById('isABTest').textContent = c.isABTest ? 'Yes' : 'No';

    // Basic info
    document.getElementById('detailName').textContent = c.name || '-';
    document.getElementById('detailDescription').textContent = c.description || '-';
    document.getElementById('detailType').innerHTML = `<span class="badge bg-${typeColor}">${typeName}</span>`;
    document.getElementById('detailStatus').innerHTML = `<span class="badge bg-${statusColor}">${statusName}</span>`;
    document.getElementById('detailCreated').textContent = c.createdAt ? formatDateTime(c.createdAt) : '-';
    document.getElementById('detailScheduled').textContent = c.scheduledAt ? formatDateTime(c.scheduledAt) : 'Not scheduled';
    document.getElementById('detailStarted').textContent = c.startedAt ? formatDateTime(c.startedAt) : '-';
    document.getElementById('detailCompleted').textContent = c.completedAt ? formatDateTime(c.completedAt) : '-';

    // Content tab
    if (c.content) {
        const ch = c.content;
        const channelIdx = ch.channel ?? 0;
        document.getElementById('contentChannel').innerHTML =
            `<span class="badge bg-${TypeColors[channelIdx] || 'secondary'}">${ChannelNames[channelIdx] || 'Unknown'}</span>`;
        document.getElementById('contentMessageBody').textContent = ch.messageBody || '-';

        if (ch.subject) {
            document.getElementById('contentSubjectRow').style.display = '';
            document.getElementById('contentSubject').textContent = ch.subject;
        }
        if (ch.htmlContent) {
            document.getElementById('contentHtmlRow').style.display = '';
            document.getElementById('contentHtml').textContent = ch.htmlContent;
        }
        if (ch.templateId) {
            document.getElementById('contentTemplateRow').style.display = '';
            document.getElementById('contentTemplate').textContent = ch.templateId;
        }
    } else {
        document.getElementById('contentDetails').style.display = 'none';
        document.getElementById('noContent').style.display = '';
    }

    // Audience tab
    if (c.audience) {
        const a = c.audience;
        const targetIdx = a.targetType ?? 0;
        document.getElementById('audienceTargetType').textContent = TargetTypeNames[targetIdx] || 'Unknown';

        if (a.groupIds && a.groupIds.length > 0) {
            document.getElementById('audienceGroupsRow').style.display = '';
            document.getElementById('audienceGroups').innerHTML =
                a.groupIds.map(id => `<span class="badge bg-secondary me-1">Group #${id}</span>`).join('');
        }
        if (a.segmentCriteria) {
            document.getElementById('audienceSegmentRow').style.display = '';
            document.getElementById('audienceSegment').textContent = a.segmentCriteria;
        }
    } else {
        document.getElementById('audienceDetails').style.display = 'none';
        document.getElementById('noAudience').style.display = '';
    }

    // Schedule tab
    if (c.schedule) {
        const s = c.schedule;
        document.getElementById('scheduleType').textContent = ScheduleTypeNames[s.scheduleType ?? 0] || 'Unknown';

        if (s.scheduledDate) {
            document.getElementById('scheduleDateRow').style.display = '';
            document.getElementById('scheduleDate').textContent = formatDateTime(s.scheduledDate);
        }
        if (s.recurrenceRule) {
            document.getElementById('scheduleRecurrenceRow').style.display = '';
            document.getElementById('scheduleRecurrence').textContent = s.recurrenceRule;
        }
        document.getElementById('scheduleTimezone').textContent =
            s.timeZoneAware ? (s.preferredTimeZone || 'Yes') : 'No';
    } else {
        document.getElementById('scheduleDetails').style.display = 'none';
        document.getElementById('noSchedule').style.display = '';
    }

    // Variants tab
    if (c.isABTest && c.variants && c.variants.length > 0) {
        document.getElementById('variantsTabItem').style.display = '';
        let variantsHtml = '<div class="table-responsive"><table class="table table-sm">';
        variantsHtml += '<thead><tr><th>Name</th><th>Traffic %</th><th>Sent</th><th>Delivered</th><th>Failed</th><th>Control</th></tr></thead><tbody>';
        c.variants.forEach(v => {
            variantsHtml += `<tr>
                <td>${escapeHtml(v.name || '-')}</td>
                <td>${v.trafficPercentage || 0}%</td>
                <td>${v.sentCount || 0}</td>
                <td>${v.deliveredCount || 0}</td>
                <td>${v.failedCount || 0}</td>
                <td>${v.isControl ? '<i class="bi bi-check-circle text-success"></i>' : '-'}</td>
            </tr>`;
        });
        variantsHtml += '</tbody></table></div>';
        document.getElementById('variantsList').innerHTML = variantsHtml;
    } else {
        document.getElementById('noVariants').style.display = '';
    }

    // Action buttons
    renderActions(c);

    // Header actions - add Edit button for Draft/Scheduled campaigns
    if (statusIdx === 0 || statusIdx === 1) {
        const headerDiv = document.getElementById('headerActions');
        const editBtn = document.createElement('a');
        editBtn.href = `/Campaigns/Create`;
        editBtn.className = 'btn btn-primary me-2';
        editBtn.innerHTML = '<i class="bi bi-pencil me-1"></i>Edit';
        headerDiv.insertBefore(editBtn, headerDiv.firstChild);
    }
}

function renderActions(c) {
    const container = document.getElementById('actionButtons');
    let html = '<div class="d-grid gap-2">';
    const id = c.id;

    // Duplicate always available
    html += `<button class="btn btn-outline-secondary" onclick="duplicateCampaign(${id})">
        <i class="bi bi-files me-2"></i>Duplicate Campaign
    </button>`;

    switch (c.status) {
        case 0: // Draft
            html += `<button class="btn btn-success" onclick="startCampaign(${id})">
                <i class="bi bi-play-fill me-2"></i>Start Campaign
            </button>`;
            html += `<button class="btn btn-outline-danger" onclick="deleteCampaign(${id})">
                <i class="bi bi-trash me-2"></i>Delete Campaign
            </button>`;
            break;
        case 1: // Scheduled
            html += `<button class="btn btn-success" onclick="startCampaign(${id})">
                <i class="bi bi-play-fill me-2"></i>Start Now
            </button>`;
            html += `<button class="btn btn-outline-danger" onclick="cancelCampaign(${id})">
                <i class="bi bi-x-circle me-2"></i>Cancel Campaign
            </button>`;
            break;
        case 2: // Running
            html += `<button class="btn btn-warning" onclick="pauseCampaign(${id})">
                <i class="bi bi-pause-fill me-2"></i>Pause Campaign
            </button>`;
            html += `<button class="btn btn-outline-danger" onclick="cancelCampaign(${id})">
                <i class="bi bi-x-circle me-2"></i>Cancel Campaign
            </button>`;
            break;
        case 3: // Paused
            html += `<button class="btn btn-success" onclick="resumeCampaign(${id})">
                <i class="bi bi-play-fill me-2"></i>Resume Campaign
            </button>`;
            html += `<button class="btn btn-outline-danger" onclick="cancelCampaign(${id})">
                <i class="bi bi-x-circle me-2"></i>Cancel Campaign
            </button>`;
            break;
        case 4: // Completed
        case 5: // Failed
            html += `<button class="btn btn-outline-danger" onclick="deleteCampaign(${id})">
                <i class="bi bi-trash me-2"></i>Delete Campaign
            </button>`;
            break;
    }

    html += '</div>';
    container.innerHTML = html;
}

// Campaign action functions (call Web controller endpoints)
function campaignAction(url, successMsg, failMsg) {
    $.ajax({
        url: url,
        method: 'POST',
        headers: typeof getAjaxHeaders === 'function' ? getAjaxHeaders() : {},
        success: function (response) {
            if (response.success || response.isSuccess) {
                showNotification(successMsg, 'success');
                setTimeout(() => location.reload(), 1500);
            } else {
                showNotification(response.message || failMsg, 'error');
            }
        },
        error: function (xhr) {
            showNotification(failMsg, 'error');
        }
    });
}

function duplicateCampaign(id) {
    if (confirm('Duplicate this campaign?')) {
        campaignAction(`/Campaigns/Duplicate?id=${id}`, 'Campaign duplicated!', 'Failed to duplicate');
    }
}

function startCampaign(id) {
    if (confirm('Start this campaign?')) {
        campaignAction(`/Campaigns/Start?id=${id}`, 'Campaign started!', 'Failed to start');
    }
}

function pauseCampaign(id) {
    if (confirm('Pause this campaign?')) {
        campaignAction(`/Campaigns/Pause?id=${id}`, 'Campaign paused!', 'Failed to pause');
    }
}

function resumeCampaign(id) {
    if (confirm('Resume this campaign?')) {
        campaignAction(`/Campaigns/Resume?id=${id}`, 'Campaign resumed!', 'Failed to resume');
    }
}

function cancelCampaign(id) {
    if (confirm('Cancel this campaign? This cannot be undone.')) {
        campaignAction(`/Campaigns/Cancel?id=${id}`, 'Campaign cancelled!', 'Failed to cancel');
    }
}

function deleteCampaign(id) {
    if (confirm('Delete this campaign? This cannot be undone.')) {
        $.ajax({
            url: `/Campaigns/Delete?id=${id}`,
            method: 'POST',
            headers: typeof getAjaxHeaders === 'function' ? getAjaxHeaders() : {},
            success: function (response) {
                if (response.success || response.isSuccess) {
                    showNotification('Campaign deleted!', 'success');
                    setTimeout(() => { window.location.href = '/Campaigns'; }, 1500);
                } else {
                    showNotification(response.message || 'Failed to delete', 'error');
                }
            },
            error: function () {
                showNotification('Failed to delete campaign', 'error');
            }
        });
    }
}

// Helpers
function formatDateTime(dateStr) {
    if (!dateStr) return '-';
    try {
        const d = new Date(dateStr);
        return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } catch {
        return dateStr;
    }
}

function escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

function showError(msg) {
    document.getElementById('campaignName').textContent = 'Error';
    document.getElementById('campaignSubtitle').textContent = msg;
}
