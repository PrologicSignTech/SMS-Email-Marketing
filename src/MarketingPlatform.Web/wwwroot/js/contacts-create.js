/**
 * Contacts Create/Edit Page - AJAX Form Submission
 * Handles contact creation and editing with validation and toast notifications
 */

const contactId = window.contactsConfig?.contactId || null;
const isEditMode = contactId !== null;

// Initialize on document ready
$(document).ready(function() {
    if (isEditMode) {
        loadContactData();
    }
    setupFormSubmit();
    setupFormValidation();
});

/**
 * Load contact data for editing
 */
function loadContactData() {
    $.ajax({
        url: window.AppUrls.buildApiUrl(window.AppUrls.api.contacts.get(contactId)),
        method: 'GET',
        headers: getAjaxHeaders(),
        success: function(response) {
            const contact = response.data || response;
            populateForm(contact);
        },
        error: function(xhr) {
            handleAjaxError(xhr, 'Failed to load contact data');
            setTimeout(() => {
                window.location.href = window.AppUrls.contacts?.index || '/Contacts/Index';
            }, 2000);
        }
    });
}

/**
 * Populate form with contact data
 */
function populateForm(contact) {
    const fields = [
        { id: 'firstName', value: contact.firstName },
        { id: 'lastName', value: contact.lastName },
        { id: 'email', value: contact.email },
        { id: 'phoneNumber', value: contact.phoneNumber },
        { id: 'company', value: contact.company },
        { id: 'city', value: contact.city },
        { id: 'state', value: contact.state },
        { id: 'country', value: contact.country },
        { id: 'notes', value: contact.notes }
    ];
    
    fields.forEach(field => {
        const element = document.getElementById(field.id);
        if (element) element.value = field.value || '';
    });
    
    const emailOptInEl = document.getElementById('emailOptIn');
    if (emailOptInEl) emailOptInEl.checked = contact.emailOptIn;
    
    const smsOptInEl = document.getElementById('smsOptIn');
    if (smsOptInEl) smsOptInEl.checked = contact.smsOptIn;
}

/**
 * Setup form submission handler
 */
function setupFormSubmit() {
    $('#contactForm').on('submit', function(e) {
        e.preventDefault();
        
        if (!validateForm()) {
            return;
        }
        
        const formData = getFormData();
        submitForm(formData);
    });
}

/**
 * Get form data
 */
function getFormData() {
    return {
        id: contactId,
        firstName: $('#firstName').val().trim(),
        lastName: $('#lastName').val().trim(),
        email: $('#email').val().trim(),
        phoneNumber: $('#phoneNumber').val().trim(),
        company: $('#company').val().trim(),
        city: $('#city').val().trim(),
        state: $('#state').val().trim(),
        country: $('#country').val().trim(),
        notes: $('#notes').val().trim(),
        emailOptIn: $('#emailOptIn').is(':checked'),
        smsOptIn: $('#smsOptIn').is(':checked')
    };
}

/**
 * Submit form via AJAX
 */
function submitForm(formData) {
    const url = isEditMode ? 
        window.AppUrls.buildApiUrl(window.AppUrls.api.contacts.update(contactId)) : 
        window.AppUrls.buildApiUrl(window.AppUrls.api.contacts.create);
    const method = isEditMode ? 'PUT' : 'POST';
    
    // Disable submit button
    const submitBtn = document.querySelector('#contactForm button[type="submit"]');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="spinner-border spinner-border-sm me-2"></i>Saving...';
    }
    
    $.ajax({
        url: url,
        method: method,
        headers: getAjaxHeaders(),
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function(response) {
            if (response.success || response.isSuccess) {
                const message = response.message || (isEditMode ? 'Contact updated successfully!' : 'Contact created successfully!');
                showNotification(message, 'success');
                
                // Redirect after short delay
                setTimeout(() => {
                    window.location.href = window.AppUrls.contacts?.index || '/Contacts/Index';
                }, 1500);
            } else {
                showNotification(response.message || 'Failed to save contact', 'error');
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> ' + (isEditMode ? 'Update Contact' : 'Create Contact');
                }
            }
        },
        error: function(xhr) {
            handleAjaxError(xhr, 'Failed to save contact');
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> ' + (isEditMode ? 'Update Contact' : 'Create Contact');
            }
        }
    });
}

/**
 * Setup form validation
 */
function setupFormValidation() {
    // Add Bootstrap validation classes
    const form = document.getElementById('contactForm');
    
    form.addEventListener('submit', function(event) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }
        form.classList.add('was-validated');
    }, false);
    
    // Email validation
    $('#email').on('blur', function() {
        const email = $(this).val();
        if (email && !isValidEmail(email)) {
            $(this).addClass('is-invalid');
            $(this).next('.invalid-feedback').remove();
            $(this).after('<div class="invalid-feedback">Please enter a valid email address</div>');
        } else {
            $(this).removeClass('is-invalid');
            $(this).next('.invalid-feedback').remove();
        }
    });
    
    // Phone validation (optional)
    $('#phoneNumber').on('blur', function() {
        const phone = $(this).val();
        if (phone && !isValidPhone(phone)) {
            $(this).addClass('is-invalid');
            $(this).next('.invalid-feedback').remove();
            $(this).after('<div class="invalid-feedback">Please enter a valid phone number</div>');
        } else {
            $(this).removeClass('is-invalid');
            $(this).next('.invalid-feedback').remove();
        }
    });
}

/**
 * Validate form
 */
function validateForm() {
    let isValid = true;
    
    // Required fields
    const firstName = $('#firstName').val().trim();
    const lastName = $('#lastName').val().trim();
    const email = $('#email').val().trim();
    
    if (!firstName) {
        showNotification('First name is required', 'error');
        $('#firstName').focus();
        isValid = false;
    } else if (!lastName) {
        showNotification('Last name is required', 'error');
        $('#lastName').focus();
        isValid = false;
    } else if (!email) {
        showNotification('Email is required', 'error');
        $('#email').focus();
        isValid = false;
    } else if (!isValidEmail(email)) {
        showNotification('Please enter a valid email address', 'error');
        $('#email').focus();
        isValid = false;
    }
    
    return isValid;
}

/**
 * Validate email format
 */
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Validate phone format
 * Accepts international formats with country codes, area codes, and various separators
 */
function isValidPhone(phone) {
    // International phone number validation (E.164 format and common variations)
    // Accepts: +1234567890, +1-234-567-8900, +1 (234) 567-8900, etc.
    const phoneRegex = /^[\+]?[(]?[0-9]{1,4}[)]?[-\s\.]?[(]?[0-9]{1,4}[)]?[-\s\.]?[0-9]{1,9}$/;
    const digitsOnly = phone.replace(/\D/g, '');
    return phoneRegex.test(phone) && digitsOnly.length >= 10 && digitsOnly.length <= 15;
}

/**
 * Handle AJAX errors
 */
function handleAjaxError(xhr, defaultMessage) {
    if (xhr.status === 401) {
        showNotification('Session expired. Please log in again.', 'error');
        setTimeout(() => {
            window.location.href = window.AppUrls.auth.login;
        }, 2000);
    } else if (xhr.status === 403) {
        showNotification('You do not have permission to perform this action', 'error');
    } else if (xhr.status === 400) {
        const errors = xhr.responseJSON?.errors || xhr.responseJSON?.message;
        if (errors) {
            if (typeof errors === 'object') {
                // Display validation errors
                Object.keys(errors).forEach(key => {
                    showNotification(errors[key][0] || errors[key], 'error');
                });
            } else {
                showNotification(errors, 'error');
            }
        } else {
            showNotification(defaultMessage, 'error');
        }
    } else {
        const errorMessage = xhr.responseJSON?.message || defaultMessage;
        showNotification(errorMessage, 'error');
    }
}
