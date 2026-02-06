// Contact Groups Management
document.addEventListener('DOMContentLoaded', function() {
    loadGroups();
});

async function loadGroups() {
    const listContainer = document.getElementById('groups-list');
    
    const mockGroups = [
        { id: 1, name: 'Customers', description: 'All active customers', contactCount: 1234, isStatic: true },
        { id: 2, name: 'Newsletter Subscribers', description: 'Newsletter opt-in contacts', contactCount: 5678, isStatic: true },
        { id: 3, name: 'VIP Members', description: 'High-value customers', contactCount: 89, isStatic: false },
        { id: 4, name: 'Recent Purchasers', description: 'Purchased in last 30 days', contactCount: 456, isStatic: false }
    ];
    
    let html = '<div class="row">';
    mockGroups.forEach(group => {
        html += `
            <div class="col-md-6 mb-3">
                <div class="card">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start">
                            <div>
                                <h5 class="card-title">${group.name}</h5>
                                <p class="card-text text-muted">${group.description}</p>
                                <p class="mb-0">
                                    <strong>${group.contactCount.toLocaleString()}</strong> contacts
                                    ${group.isStatic ? '<span class="badge bg-info ms-2">Static</span>' : '<span class="badge bg-warning ms-2">Dynamic</span>'}
                                </p>
                            </div>
                            <div class="btn-group">
                                <button class="btn btn-sm btn-outline-primary" data-action="view" data-group-id="${group.id}">
                                    <i class="bi bi-eye"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-success" data-action="edit" data-group-id="${group.id}">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-danger" data-action="delete" data-group-id="${group.id}">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });
    html += '</div>';
    
    listContainer.innerHTML = html;
    
    // Attach event listeners
    listContainer.addEventListener('click', function(e) {
        const button = e.target.closest('[data-action]');
        if (!button) return;
        
        const action = button.dataset.action;
        const groupId = button.dataset.groupId;
        
        if (action === 'view') {
            viewGroup(groupId);
        } else if (action === 'edit') {
            editGroup(groupId);
        } else if (action === 'delete') {
            deleteGroup(groupId);
        }
    });
}

// Create group button handler
document.addEventListener('DOMContentLoaded', function() {
    const createBtn = document.querySelector('[data-action="create-group"]');
    if (createBtn) {
        createBtn.addEventListener('click', createGroup);
    }
});

function createGroup() {
    showNotification('Create group functionality (Demo)', 'info');
}

function viewGroup(id) {
    showNotification(`View group ${id} (Demo)`, 'info');
}

function editGroup(id) {
    showNotification(`Edit group ${id} (Demo)`, 'info');
}

function deleteGroup(id) {
    if (confirm('Are you sure you want to delete this group?')) {
        showNotification('Group deleted! (Demo)', 'success');
        loadGroups();
    }
}
