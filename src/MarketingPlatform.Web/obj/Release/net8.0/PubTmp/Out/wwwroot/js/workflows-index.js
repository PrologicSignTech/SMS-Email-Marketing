/**
 * Workflows Index Page - Card Layout with Real API Integration
 * SERVER-SIDE: Calls Web controller endpoints for all operations
 */

let allWorkflows = [];
let currentFilter = 'all';
let searchTimeout = null;
let stepCounter = 0;

const triggerTypeNames = ['Event', 'Schedule', 'Keyword', 'Manual'];
const actionTypeNames = ['Send SMS', 'Send MMS', 'Send Email', 'Wait', 'Add to Group', 'Remove from Group', 'Add Tag'];

$(document).ready(function () {
    loadWorkflows();
    setupFilters();
    setupSearch();
});

function setupFilters() {
    $('#workflowTabs .nav-link').on('click', function (e) {
        e.preventDefault();
        $('#workflowTabs .nav-link').removeClass('active');
        $(this).addClass('active');
        currentFilter = $(this).data('filter');
        renderWorkflows();
    });
}

function setupSearch() {
    $('#workflowSearch').on('input', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            loadWorkflows();
        }, 400);
    });
}

function loadWorkflows() {
    var searchTerm = $('#workflowSearch').val() || '';
    var listContainer = document.getElementById('workflows-list');
    listContainer.innerHTML = '<div class="text-center py-5"><div class="spinner-border" role="status"></div></div>';

    $.ajax({
        url: '/Workflows/GetWorkflows',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            draw: 1,
            start: 0,
            length: 100,
            search: { value: searchTerm }
        }),
        success: function (response) {
            allWorkflows = response.data || [];
            renderWorkflows();
        },
        error: function () {
            listContainer.innerHTML = '<div class="alert alert-danger"><i class="bi bi-exclamation-triangle me-2"></i>Error loading workflows</div>';
        }
    });
}

function renderWorkflows() {
    var listContainer = document.getElementById('workflows-list');
    var filtered = allWorkflows;

    // Apply filter
    if (currentFilter === 'active') {
        filtered = allWorkflows.filter(function (w) { return w.isActive === true; });
    } else if (currentFilter === 'draft') {
        filtered = allWorkflows.filter(function (w) { return w.isActive !== true; });
    }

    if (!filtered || filtered.length === 0) {
        var msg = currentFilter === 'all' ? 'No workflows yet. Create your first workflow to get started!' : 'No ' + currentFilter + ' workflows found.';
        listContainer.innerHTML = '<div class="text-center py-5"><i class="bi bi-diagram-3 fs-1 text-muted d-block mb-3"></i><p class="text-muted">' + msg + '</p></div>';
        return;
    }

    var html = '<div class="row">';
    filtered.forEach(function (workflow) {
        var statusColor = workflow.isActive ? 'success' : 'secondary';
        var statusText = workflow.isActive ? 'Active' : 'Draft';
        var triggerName = triggerTypeNames[workflow.triggerType] || 'Manual';
        var nodeCount = (workflow.nodes && workflow.nodes.length) || 0;
        var modifiedDate = workflow.updatedAt ? new Date(workflow.updatedAt).toLocaleDateString() : new Date(workflow.createdAt).toLocaleDateString();

        html += '<div class="col-md-6 mb-3">';
        html += '<div class="card h-100">';
        html += '<div class="card-body">';
        html += '<div class="d-flex justify-content-between align-items-start mb-2">';
        html += '<div>';
        html += '<h5 class="card-title mb-1">' + escapeHtml(workflow.name) + '</h5>';
        html += '<p class="card-text text-muted small mb-0">' + escapeHtml(workflow.description || '') + '</p>';
        html += '</div>';
        html += '<span class="badge bg-' + statusColor + '">' + statusText + '</span>';
        html += '</div>';
        html += '<div class="d-flex justify-content-between align-items-center mb-3">';
        html += '<small class="text-muted">';
        html += '<i class="bi bi-lightning"></i> ' + escapeHtml(triggerName);
        html += '<i class="bi bi-gear ms-2"></i> ' + nodeCount + ' steps';
        html += '</small>';
        html += '<small class="text-muted">Modified: ' + modifiedDate + '</small>';
        html += '</div>';

        // Execution stats
        if (workflow.totalExecutions > 0) {
            html += '<div class="mb-3">';
            html += '<small class="text-muted d-block mb-1">Executions: ' + workflow.totalExecutions + '</small>';
            var completedPct = workflow.totalExecutions > 0 ? Math.round((workflow.completedExecutions / workflow.totalExecutions) * 100) : 0;
            html += '<div class="progress" style="height: 6px;">';
            html += '<div class="progress-bar bg-success" style="width:' + completedPct + '%" title="Completed: ' + workflow.completedExecutions + '"></div>';
            var failedPct = workflow.totalExecutions > 0 ? Math.round((workflow.failedExecutions / workflow.totalExecutions) * 100) : 0;
            html += '<div class="progress-bar bg-danger" style="width:' + failedPct + '%" title="Failed: ' + workflow.failedExecutions + '"></div>';
            html += '</div>';
            html += '</div>';
        }

        // Action buttons
        html += '<div class="d-flex justify-content-between">';
        html += '<div class="btn-group btn-group-sm">';
        html += '<button class="btn btn-outline-primary" onclick="editWorkflow(' + workflow.id + ')" title="Edit"><i class="bi bi-pencil"></i> Edit</button>';
        html += '<button class="btn btn-outline-info" onclick="duplicateWorkflow(' + workflow.id + ')" title="Duplicate"><i class="bi bi-files"></i> Duplicate</button>';
        html += '</div>';
        html += '<div class="btn-group btn-group-sm">';

        if (!workflow.isActive) {
            html += '<button class="btn btn-outline-success" onclick="activateWorkflow(' + workflow.id + ')" title="Activate"><i class="bi bi-play"></i></button>';
        } else {
            html += '<button class="btn btn-outline-warning" onclick="pauseWorkflow(' + workflow.id + ')" title="Pause"><i class="bi bi-pause"></i></button>';
        }
        html += '<button class="btn btn-outline-danger" onclick="deleteWorkflow(' + workflow.id + ')" title="Delete"><i class="bi bi-trash"></i></button>';
        html += '</div>';
        html += '</div>';

        html += '</div>';
        html += '</div>';
        html += '</div>';
    });
    html += '</div>';

    listContainer.innerHTML = html;
}

// ============================================================================
// CRUD OPERATIONS
// ============================================================================

function showCreateModal() {
    document.getElementById('wfEditId').value = '';
    document.getElementById('wfName').value = '';
    document.getElementById('wfDescription').value = '';
    document.getElementById('wfTriggerType').value = '3';
    document.getElementById('wfIsActive').value = 'false';
    document.getElementById('wfTriggerCriteria').value = '';
    document.getElementById('wfStepsList').innerHTML = '';
    document.getElementById('workflowModalTitle').innerHTML = '<i class="bi bi-plus-circle me-2"></i>Create Workflow';
    document.getElementById('saveWorkflowBtn').innerHTML = '<i class="bi bi-save me-1"></i>Save Workflow';
    stepCounter = 0;
    addStep(); // Start with one step
    var modal = new bootstrap.Modal(document.getElementById('workflowModal'));
    modal.show();
}

function editWorkflow(id) {
    var workflow = allWorkflows.find(function (w) { return w.id === id; });
    if (!workflow) {
        showNotification('Workflow not found', 'error');
        return;
    }

    document.getElementById('wfEditId').value = workflow.id;
    document.getElementById('wfName').value = workflow.name || '';
    document.getElementById('wfDescription').value = workflow.description || '';
    document.getElementById('wfTriggerType').value = workflow.triggerType || 0;
    document.getElementById('wfIsActive').value = workflow.isActive ? 'true' : 'false';
    document.getElementById('wfTriggerCriteria').value = workflow.triggerCriteria || '';
    document.getElementById('workflowModalTitle').innerHTML = '<i class="bi bi-pencil me-2"></i>Edit Workflow';
    document.getElementById('saveWorkflowBtn').innerHTML = '<i class="bi bi-save me-1"></i>Update Workflow';

    // Populate steps
    document.getElementById('wfStepsList').innerHTML = '';
    stepCounter = 0;

    if (workflow.nodes && workflow.nodes.length > 0) {
        workflow.nodes.forEach(function (node) {
            addStep(node);
        });
    } else {
        addStep();
    }

    var modal = new bootstrap.Modal(document.getElementById('workflowModal'));
    modal.show();
}

function addStep(data) {
    stepCounter++;
    var idx = stepCounter;
    var actionType = data ? data.actionType : 0;
    var actionConfig = data ? (data.actionConfiguration || '') : '';
    var delayMinutes = data ? (data.delayMinutes || '') : '';
    var nodeLabel = data ? (data.nodeLabel || '') : '';

    var stepHtml = '<div class="card mb-2" id="step-' + idx + '">';
    stepHtml += '<div class="card-body py-2">';
    stepHtml += '<div class="d-flex justify-content-between align-items-center mb-2">';
    stepHtml += '<strong class="small">Step ' + idx + '</strong>';
    stepHtml += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="removeStep(' + idx + ')"><i class="bi bi-x"></i></button>';
    stepHtml += '</div>';
    stepHtml += '<div class="row g-2">';
    stepHtml += '<div class="col-md-4">';
    stepHtml += '<select class="form-select form-select-sm step-action" data-step="' + idx + '">';

    for (var i = 0; i < actionTypeNames.length; i++) {
        var selected = (i === actionType) ? ' selected' : '';
        stepHtml += '<option value="' + i + '"' + selected + '>' + actionTypeNames[i] + '</option>';
    }

    stepHtml += '</select>';
    stepHtml += '</div>';
    stepHtml += '<div class="col-md-3">';
    stepHtml += '<input type="text" class="form-control form-control-sm step-label" data-step="' + idx + '" placeholder="Label" value="' + escapeHtml(nodeLabel) + '">';
    stepHtml += '</div>';
    stepHtml += '<div class="col-md-3">';
    stepHtml += '<input type="number" class="form-control form-control-sm step-delay" data-step="' + idx + '" placeholder="Delay (min)" value="' + (delayMinutes || '') + '">';
    stepHtml += '</div>';
    stepHtml += '<div class="col-md-2">';
    stepHtml += '<input type="text" class="form-control form-control-sm step-config" data-step="' + idx + '" placeholder="Config" value="' + escapeHtml(actionConfig) + '" title="Action configuration JSON">';
    stepHtml += '</div>';
    stepHtml += '</div>';
    stepHtml += '</div>';
    stepHtml += '</div>';

    document.getElementById('wfStepsList').insertAdjacentHTML('beforeend', stepHtml);
}

function removeStep(idx) {
    var el = document.getElementById('step-' + idx);
    if (el) el.remove();
}

function saveWorkflow() {
    var name = document.getElementById('wfName').value.trim();
    if (!name) {
        showNotification('Workflow name is required', 'error');
        return;
    }

    var editId = document.getElementById('wfEditId').value;
    var isEdit = editId && editId !== '';

    // Collect steps
    var nodes = [];
    var stepCards = document.querySelectorAll('#wfStepsList .card');
    var order = 1;
    stepCards.forEach(function (card) {
        var actionEl = card.querySelector('.step-action');
        var labelEl = card.querySelector('.step-label');
        var delayEl = card.querySelector('.step-delay');
        var configEl = card.querySelector('.step-config');

        nodes.push({
            stepOrder: order++,
            actionType: parseInt(actionEl.value),
            nodeLabel: labelEl.value.trim() || null,
            delayMinutes: delayEl.value ? parseInt(delayEl.value) : null,
            actionConfiguration: configEl.value.trim() || null
        });
    });

    var workflowData = {
        name: name,
        description: document.getElementById('wfDescription').value.trim() || null,
        triggerType: parseInt(document.getElementById('wfTriggerType').value),
        triggerCriteria: document.getElementById('wfTriggerCriteria').value.trim() || null,
        isActive: document.getElementById('wfIsActive').value === 'true',
        nodes: nodes
    };

    var url = isEdit ? '/Workflows/UpdateWorkflow?id=' + editId : '/Workflows/CreateWorkflow';

    $.ajax({
        url: url,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(workflowData),
        success: function (response) {
            if (response.success) {
                showNotification(response.message || (isEdit ? 'Workflow updated!' : 'Workflow created!'), 'success');
                $('#workflowModal').modal('hide');
                loadWorkflows();
            } else {
                showNotification(response.message || 'Operation failed', 'error');
            }
        },
        error: function (xhr) {
            var msg = xhr.responseJSON?.message || 'An error occurred';
            showNotification(msg, 'error');
        }
    });
}

function duplicateWorkflow(id) {
    confirmAction('Duplicate this workflow?', function () {
        $.ajax({
            url: '/Workflows/Duplicate?id=' + id,
            method: 'POST',
            success: function (response) {
                if (response.success) {
                    showNotification(response.message || 'Workflow duplicated!', 'success');
                    loadWorkflows();
                } else {
                    showNotification(response.message || 'Failed to duplicate', 'error');
                }
            },
            error: function () {
                showNotification('Failed to duplicate workflow', 'error');
            }
        });
    });
}

function deleteWorkflow(id) {
    confirmAction('Are you sure you want to delete this workflow? This cannot be undone.', function () {
        $.ajax({
            url: '/Workflows/Delete?id=' + id,
            method: 'POST',
            success: function (response) {
                if (response.success) {
                    showNotification(response.message || 'Workflow deleted!', 'success');
                    loadWorkflows();
                } else {
                    showNotification(response.message || 'Failed to delete', 'error');
                }
            },
            error: function () {
                showNotification('Failed to delete workflow', 'error');
            }
        });
    });
}

function activateWorkflow(id) {
    confirmAction('Activate this workflow?', function () {
        $.ajax({
            url: '/Workflows/Activate?id=' + id,
            method: 'POST',
            success: function (response) {
                if (response.success) {
                    showNotification(response.message || 'Workflow activated!', 'success');
                    loadWorkflows();
                } else {
                    showNotification(response.message || 'Failed to activate', 'error');
                }
            },
            error: function () {
                showNotification('Failed to activate workflow', 'error');
            }
        });
    });
}

function pauseWorkflow(id) {
    confirmAction('Pause this workflow?', function () {
        $.ajax({
            url: '/Workflows/Pause?id=' + id,
            method: 'POST',
            success: function (response) {
                if (response.success) {
                    showNotification(response.message || 'Workflow paused!', 'success');
                    loadWorkflows();
                } else {
                    showNotification(response.message || 'Failed to pause', 'error');
                }
            },
            error: function () {
                showNotification('Failed to pause workflow', 'error');
            }
        });
    });
}
