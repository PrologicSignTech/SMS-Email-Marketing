/**
 * landing-config.js - Landing page configuration functionality
 * Handles menu item management for landing page navigation
 */

/**
 * Add a new menu item to the menu configuration
 */
function addMenuItem() {
    const menuItems = document.getElementById('menuItems');
    if (!menuItems) return;
    
    const newItem = document.createElement('div');
    newItem.className = 'card mb-2';
    newItem.innerHTML = `
        <div class="card-body">
            <div class="row">
                <div class="col-md-4">
                    <input type="text" class="form-control" placeholder="Label">
                </div>
                <div class="col-md-4">
                    <input type="text" class="form-control" placeholder="URL">
                </div>
                <div class="col-md-3">
                    <input type="number" class="form-control" placeholder="Order">
                </div>
                <div class="col-md-1">
                    <button class="btn btn-sm btn-danger w-100" onclick="removeItem(this)">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>
        </div>
    `;
    menuItems.appendChild(newItem);
}

/**
 * Remove a menu item from the configuration
 * @param {HTMLElement} btn - The remove button element
 */
function removeItem(btn) {
    if (!btn) return;
    
    const card = btn.closest('.card');
    if (card) {
        card.remove();
    }
}

// Make functions available globally for event handlers
if (typeof window !== 'undefined') {
    window.addMenuItem = addMenuItem;
    window.removeItem = removeItem;
}
