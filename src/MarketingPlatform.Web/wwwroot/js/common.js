/**
 * Common JavaScript utilities for Marketing Platform
 * Provides reusable functions for DataTables, AJAX forms, notifications, and validation
 */

// ============================================================================
// NOTIFICATION SYSTEM
// ============================================================================

/**
 * Display a toast notification
 * @param {string} message - Message to display
 * @param {string} type - Type of notification: 'success', 'error', 'warning', 'info'
 * @param {number} duration - Duration in milliseconds (default: 5000)
 */
function showNotification(message, type = 'info', duration = 5000) {
    const alertClass = type === 'success' ? 'alert-success' : 
                      type === 'error' ? 'alert-danger' : 
                      type === 'warning' ? 'alert-warning' : 'alert-info';
    
    const iconClass = type === 'success' ? 'check-circle' : 
                     type === 'error' ? 'exclamation-circle' :
                     type === 'warning' ? 'exclamation-triangle' : 'info-circle';
    
    const notification = `
        <div class="alert ${alertClass} alert-dismissible fade show position-fixed top-0 end-0 m-3" 
             role="alert" style="z-index: 9999; min-width: 300px; max-width: 500px;">
            <i class="bi bi-${iconClass} me-2"></i>${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    $('body').append(notification);
    
    // Auto-dismiss after duration
    setTimeout(() => {
        $('.alert').fadeOut('slow', function() {
            $(this).remove();
        });
    }, duration);
}

// ============================================================================
// DATATABLES HELPER FUNCTIONS
// ============================================================================

/**
 * Initialize a DataTable with common configuration
 * @param {string} selector - jQuery selector for the table
 * @param {object} options - DataTable configuration options
 * @returns {object} DataTable instance
 */
function initDataTable(selector, options = {}) {
    const defaultOptions = {
        serverSide: true,
        processing: true,
        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
             '<"row"<"col-sm-12"Btr>>' +
             '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        buttons: [
            {
                extend: 'csv',
                className: 'btn btn-sm btn-outline-primary',
                text: '<i class="bi bi-file-earmark-csv"></i> CSV',
                exportOptions: {
                    columns: ':visible:not(.no-export)'
                }
            },
            {
                extend: 'excel',
                className: 'btn btn-sm btn-outline-success',
                text: '<i class="bi bi-file-earmark-excel"></i> Excel',
                exportOptions: {
                    columns: ':visible:not(.no-export)'
                }
            },
            {
                extend: 'pdf',
                className: 'btn btn-sm btn-outline-danger',
                text: '<i class="bi bi-file-earmark-pdf"></i> PDF',
                exportOptions: {
                    columns: ':visible:not(.no-export)'
                }
            }
        ],
        language: {
            processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>',
            emptyTable: '<div class="text-center py-3"><i class="bi bi-inbox"></i> No data available</div>',
            zeroRecords: '<div class="text-center py-3"><i class="bi bi-search"></i> No matching records found</div>',
            loadingRecords: 'Loading...',
            paginate: {
                first: '<i class="bi bi-chevron-bar-left"></i>',
                previous: '<i class="bi bi-chevron-left"></i>',
                next: '<i class="bi bi-chevron-right"></i>',
                last: '<i class="bi bi-chevron-bar-right"></i>'
            }
        },
        stateSave: true,
        stateDuration: 60 * 60 * 24, // 24 hours
        // Search debouncing
        searchDelay: 300
    };

    // Merge default options with provided options
    const finalOptions = $.extend(true, {}, defaultOptions, options);

    // Initialize DataTable
    return $(selector).DataTable(finalOptions);
}

/**
 * Format date for display in DataTables
 * @param {string} dateStr - ISO date string
 * @returns {string} Formatted date
 */
function formatDate(dateStr) {
    if (!dateStr) return 'N/A';
    const date = new Date(dateStr);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
}

/**
 * Format date as short date
 * @param {string} dateStr - ISO date string
 * @returns {string} Formatted date
 */
function formatShortDate(dateStr) {
    if (!dateStr) return 'N/A';
    const date = new Date(dateStr);
    return date.toLocaleDateString();
}

/**
 * Create a status badge
 * @param {string} status - Status text
 * @param {string} color - Bootstrap color class (primary, success, danger, etc.)
 * @returns {string} HTML for badge
 */
function createBadge(status, color = 'primary') {
    return `<span class="badge bg-${color}">${escapeHtml(status)}</span>`;
}

/**
 * Escape HTML to prevent XSS
 * @param {string} text - Text to escape
 * @returns {string} Escaped text
 */
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// ============================================================================
// AJAX FORM HANDLING
// ============================================================================

/**
 * Setup AJAX form submission with validation
 * @param {string} formSelector - jQuery selector for the form
 * @param {object} options - Configuration options
 */
function setupAjaxForm(formSelector, options = {}) {
    const defaults = {
        url: null, // API endpoint URL
        method: 'POST',
        onSuccess: null, // Callback on success
        onError: null, // Callback on error
        redirectUrl: null, // URL to redirect to on success
        successMessage: 'Saved successfully!',
        clearOnSuccess: false,
        validateBeforeSubmit: true,
        includeToken: true
    };

    const config = $.extend({}, defaults, options);

    $(formSelector).on('submit', function(e) {
        e.preventDefault();
        
        const $form = $(this);
        const $submitBtn = $form.find('button[type="submit"]');
        
        // Client-side validation
        if (config.validateBeforeSubmit && !this.checkValidity()) {
            e.stopPropagation();
            $form.addClass('was-validated');
            return false;
        }

        // Disable submit button and show loading state
        const originalBtnText = $submitBtn.html();
        $submitBtn.prop('disabled', true)
                 .html('<span class="spinner-border spinner-border-sm me-2"></span>Saving...');

        // Collect form data
        const formData = {};
        $form.find('input, select, textarea').each(function() {
            const $field = $(this);
            const name = $field.attr('name') || $field.attr('id');
            if (name) {
                if ($field.is(':checkbox')) {
                    formData[name] = $field.is(':checked');
                } else if ($field.is(':radio')) {
                    if ($field.is(':checked')) {
                        formData[name] = $field.val();
                    }
                } else {
                    formData[name] = $field.val();
                }
            }
        });

        // Prepare headers
        const headers = {
            'Content-Type': 'application/json'
        };

        if (config.includeToken) {
            const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
            if (token) {
                headers['Authorization'] = 'Bearer ' + token;
            }
        }

        // Make AJAX request
        $.ajax({
            url: config.url,
            method: config.method,
            headers: headers,
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success || response.isSuccess) {
                    showNotification(config.successMessage, 'success');
                    
                    if (config.clearOnSuccess) {
                        $form[0].reset();
                        $form.removeClass('was-validated');
                    }

                    if (config.onSuccess) {
                        config.onSuccess(response);
                    }

                    if (config.redirectUrl) {
                        setTimeout(() => {
                            window.location.href = config.redirectUrl;
                        }, 1000);
                    }
                } else {
                    showNotification(response.message || 'An error occurred', 'error');
                    if (config.onError) {
                        config.onError(response);
                    }
                }
            },
            error: function(xhr) {
                // Handle validation errors
                if (xhr.status === 400 && xhr.responseJSON && xhr.responseJSON.errors) {
                    displayValidationErrors(xhr.responseJSON.errors, $form);
                    showNotification('Please fix the validation errors', 'error');
                } else if (xhr.status === 401) {
                    showNotification('Unauthorized. Please log in again.', 'error');
                    setTimeout(() => {
                        window.location.href = AppUrls.auth.login;
                    }, 2000);
                } else if (xhr.status === 403) {
                    showNotification('You do not have permission to perform this action', 'error');
                } else if (xhr.status === 404) {
                    showNotification('Resource not found', 'error');
                } else {
                    const errorMessage = xhr.responseJSON?.message || 'An error occurred. Please try again.';
                    showNotification(errorMessage, 'error');
                }

                if (config.onError) {
                    config.onError(xhr);
                }
            },
            complete: function() {
                // Re-enable submit button
                $submitBtn.prop('disabled', false).html(originalBtnText);
            }
        });

        return false;
    });
}

/**
 * Display validation errors on form fields
 * @param {object} errors - Object with field names as keys and error messages as values
 * @param {jQuery} $form - Form jQuery object
 */
function displayValidationErrors(errors, $form) {
    // Clear previous errors
    $form.find('.is-invalid').removeClass('is-invalid');
    $form.find('.invalid-feedback').hide().text('');

    // Display new errors
    Object.keys(errors).forEach(key => {
        const $field = $form.find(`#${key}, [name="${key}"]`);
        if ($field.length) {
            $field.addClass('is-invalid');
            
            let errorMessage = errors[key];
            if (Array.isArray(errorMessage)) {
                errorMessage = errorMessage[0];
            }

            let $feedback = $field.siblings('.invalid-feedback');
            if (!$feedback.length) {
                $feedback = $('<div class="invalid-feedback"></div>');
                $field.after($feedback);
            }
            
            $feedback.text(errorMessage).show();
        }
    });
}

// ============================================================================
// FORM VALIDATION
// ============================================================================

/**
 * Setup real-time validation for form fields
 * @param {string} formSelector - jQuery selector for the form
 */
function setupRealtimeValidation(formSelector) {
    $(formSelector).find('.form-control, .form-select').on('blur change', function() {
        validateField($(this));
    });
}

/**
 * Validate a single field
 * @param {jQuery} $field - Field jQuery object
 * @returns {boolean} Whether the field is valid
 */
function validateField($field) {
    const isValid = $field[0].checkValidity();
    
    if (isValid) {
        $field.removeClass('is-invalid').addClass('is-valid');
        $field.siblings('.invalid-feedback').hide();
    } else {
        $field.removeClass('is-valid').addClass('is-invalid');
        const validationMessage = $field[0].validationMessage;
        
        let $feedback = $field.siblings('.invalid-feedback');
        if (!$feedback.length) {
            $feedback = $('<div class="invalid-feedback"></div>');
            $field.after($feedback);
        }
        
        $feedback.text(validationMessage).show();
    }
    
    return isValid;
}

/**
 * Clear validation state from a form
 * @param {string} formSelector - jQuery selector for the form
 */
function clearValidation(formSelector) {
    const $form = $(formSelector);
    $form.removeClass('was-validated');
    $form.find('.is-valid, .is-invalid').removeClass('is-valid is-invalid');
    $form.find('.invalid-feedback').hide();
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

/**
 * Get authentication token from storage
 * @returns {string|null} Authentication token
 */
function getAuthToken() {
    return localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
}

/**
 * Create AJAX headers with authentication
 * @returns {object} Headers object
 */
function getAjaxHeaders() {
    const headers = {
        'Content-Type': 'application/json'
    };
    
    const token = getAuthToken();
    if (token) {
        headers['Authorization'] = 'Bearer ' + token;
    }
    
    return headers;
}

/**
 * Format number with thousands separator
 * @param {number} num - Number to format
 * @returns {string} Formatted number
 */
function formatNumber(num) {
    if (num === null || num === undefined) return 'N/A';
    return num.toLocaleString();
}

/**
 * Calculate percentage
 * @param {number} value - Numerator
 * @param {number} total - Denominator
 * @param {number} decimals - Number of decimal places
 * @returns {string} Percentage string
 */
function calculatePercentage(value, total, decimals = 1) {
    if (!total || total === 0) return '0%';
    return ((value / total) * 100).toFixed(decimals) + '%';
}

/**
 * Debounce function - delays execution until after wait time
 * @param {function} func - Function to debounce
 * @param {number} wait - Wait time in milliseconds
 * @returns {function} Debounced function
 */
function debounce(func, wait = 300) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Confirm action with a dialog
 * @param {string} message - Confirmation message
 * @param {function} onConfirm - Callback when confirmed
 * @param {function} onCancel - Callback when cancelled
 */
function confirmAction(message, onConfirm, onCancel = null) {
    if (confirm(message)) {
        if (onConfirm) onConfirm();
    } else {
        if (onCancel) onCancel();
    }
}

// ============================================================================
// INITIALIZATION
// ============================================================================

// Initialize tooltips and popovers on document ready
$(document).ready(function() {
    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize Bootstrap popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
});
