/**
 * contacts-details.js - Contact details page functionality
 * Handles edit contact navigation
 */

/**
 * Navigate to edit contact page
 * @param {number} contactId - The ID of the contact to edit (optional, defaults to 1)
 */
function editContact(contactId = 1) {
    // Use AppUrls if available, otherwise construct URL directly
    const editUrl = (window.AppUrls && window.AppUrls.contacts && window.AppUrls.contacts.edit) 
        ? (typeof window.AppUrls.contacts.edit === 'function' 
            ? window.AppUrls.contacts.edit(contactId)
            : `/contacts/edit/${contactId}`)
        : `/contacts/edit/${contactId}`;
    
    window.location.href = editUrl;
}

// Make function available globally for event handlers
if (typeof window !== 'undefined') {
    window.editContact = editContact;
}
