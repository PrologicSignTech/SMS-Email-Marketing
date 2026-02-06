/**
 * Keywords Create Page - API Integration
 * Handles keyword creation with validation and API bindings
 */

/**
 * Initialize the create page
 */
function initKeywordCreate() {
    setupEventListeners();
    setupFormValidation();
    loadExistingKeywords();
}

/**
 * Setup all event listeners
 */
function setupEventListeners() {
    // Keyword input - uppercase and alphanumeric only
    const keywordInput = document.getElementById('keyword');
    if (keywordInput) {
        keywordInput.addEventListener('input', handleKeywordInput);
        keywordInput.addEventListener('blur', checkKeywordAvailability);
    }

    // Auto-response character counter
    const autoResponseInput = document.getElementById('autoResponse');
    if (autoResponseInput) {
        autoResponseInput.addEventListener('input', updateCharacterCount);
    }

    // Preview button
    const previewBtn = document.querySelector('button[onclick*="previewKeyword"]');
    if (previewBtn) {
        previewBtn.onclick = null; // Remove inline handler
        previewBtn.addEventListener('click', previewKeyword);
    }

    // Form submission
    const form = document.getElementById('keywordForm');
    if (form) {
        form.addEventListener('submit', handleFormSubmit);
    }
}

/**
 * Handle keyword input - convert to uppercase and validate format
 */
function handleKeywordInput(e) {
    const input = e.target;
    input.value = input.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
    
    // Clear any previous validation messages
    input.classList.remove('is-invalid', 'is-valid');
    const feedback = input.parentElement.querySelector('.invalid-feedback');
    if (feedback) {
        feedback.style.display = 'none';
    }
}

/**
 * Check keyword availability via API
 */
async function checkKeywordAvailability() {
    const keywordInput = document.getElementById('keyword');
    if (!keywordInput) return;
    
    const keyword = keywordInput.value.trim();
    
    if (!keyword || keyword.length < 2) {
        return;
    }

    try {
        const url = window.AppUrls.buildUrl(
            window.AppUrls.buildApiUrl(window.AppUrls.api.keywords.checkAvailability),
            { keywordText: keyword }
        );
        
        const response = await fetch(url, {
            method: 'GET',
            headers: getAjaxHeaders()
        });

        if (response.ok) {
            const result = await response.json();
            const isAvailable = result.data || result.isSuccess;
            
            if (isAvailable) {
                keywordInput.classList.remove('is-invalid');
                keywordInput.classList.add('is-valid');
                showFieldFeedback(keywordInput, 'Keyword is available!', 'valid');
            } else {
                keywordInput.classList.remove('is-valid');
                keywordInput.classList.add('is-invalid');
                showFieldFeedback(keywordInput, 'This keyword is already taken or reserved', 'invalid');
            }
        }
    } catch (error) {
        console.error('Error checking keyword availability:', error);
    }
}

/**
 * Show feedback message for a field
 */
function showFieldFeedback(field, message, type) {
    const feedbackClass = type === 'valid' ? 'valid-feedback' : 'invalid-feedback';
    let feedback = field.parentElement.querySelector(`.${feedbackClass}`);
    
    if (!feedback) {
        feedback = document.createElement('div');
        feedback.className = feedbackClass;
        field.parentElement.appendChild(feedback);
    }
    
    feedback.textContent = message;
    feedback.style.display = 'block';
}

/**
 * Update character count for auto-response message
 */
function updateCharacterCount(e) {
    const textarea = e.target;
    const count = textarea.value.length;
    const counter = document.getElementById('charCount');
    
    if (counter) {
        counter.textContent = `${count}/160 characters`;
        
        if (count > 160) {
            counter.classList.add('text-danger');
            counter.classList.remove('text-muted');
        } else {
            counter.classList.remove('text-danger');
            counter.classList.add('text-muted');
        }
    }
}

/**
 * Preview keyword auto-response
 */
function previewKeyword(e) {
    if (e) e.preventDefault();
    
    const shortCodeEl = document.getElementById('shortCode');
    const messageEl = document.getElementById('autoResponse');
    const shortCode = shortCodeEl ? shortCodeEl.value : '';
    const message = messageEl ? messageEl.value : '';
    
    if (!shortCode || !message) {
        showNotification('Please fill in short code and auto-response message to preview.', 'warning');
        return;
    }
    
    const previewShortCodeEl = document.getElementById('previewShortCode');
    if (previewShortCodeEl) previewShortCodeEl.textContent = shortCode;
    
    const previewMessageEl = document.getElementById('previewMessage');
    if (previewMessageEl) previewMessageEl.textContent = message;
    
    const modalElement = document.getElementById('previewModal');
    if (modalElement) {
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
}

/**
 * Setup form validation
 */
function setupFormValidation() {
    const form = document.getElementById('keywordForm');
    if (!form) return;

    // Add Bootstrap validation classes
    form.classList.add('needs-validation');
    
    // Real-time validation for required fields
    const requiredFields = form.querySelectorAll('[required]');
    requiredFields.forEach(field => {
        field.addEventListener('blur', function() {
            validateField(this);
        });
    });
}

/**
 * Validate a single form field
 */
function validateField(field) {
    if (field.checkValidity()) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
        return true;
    } else {
        field.classList.remove('is-valid');
        field.classList.add('is-invalid');
        return false;
    }
}

/**
 * Handle form submission
 */
async function handleFormSubmit(e) {
    e.preventDefault();
    
    const form = e.target;
    
    // Validate form
    if (!form.checkValidity()) {
        e.stopPropagation();
        form.classList.add('was-validated');
        showNotification('Please fill in all required fields correctly.', 'error');
        return false;
    }

    // Additional validation
    const keyword = document.getElementById('keyword').value;
    if (keyword.length < 2) {
        showNotification('Keyword must be at least 2 characters long.', 'error');
        return false;
    }
    
    const autoResponse = document.getElementById('autoResponse').value;
    if (autoResponse.length > 160) {
        showNotification('Auto-response message exceeds 160 characters. Please shorten it.', 'error');
        return false;
    }
    
    // Prepare data for API
    const data = {
        keywordText: keyword,
        description: `SMS keyword: ${keyword}`,
        responseMessage: autoResponse,
        linkedCampaignId: document.getElementById('campaignId').value || null,
        optInGroupId: null // Could be added if needed
    };
    
    // Submit to API
    await createKeyword(data);
    
    return false;
}

/**
 * Create keyword via API
 */
async function createKeyword(data) {
    const submitBtn = document.querySelector('#keywordForm button[type="submit"]');
    const originalBtnContent = submitBtn ? submitBtn.innerHTML : '';
    
    // Disable button and show loading state
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Creating...';
    }
    
    try {
        const response = await fetch(window.AppUrls.buildApiUrl(window.AppUrls.api.keywords.create), {
            method: 'POST',
            headers: getAjaxHeaders(),
            body: JSON.stringify(data)
        });

        const result = await response.json();
        
        if (response.ok && (result.success || result.isSuccess)) {
            showNotification('Keyword created successfully!', 'success');
            
            // Redirect after short delay
            setTimeout(() => {
                window.location.href = window.AppUrls.keywords?.index || '/Keywords/Index';
            }, 1500);
        } else {
            // Handle validation errors
            if (result.errors) {
                displayValidationErrors(result.errors);
                showNotification('Please fix the validation errors.', 'error');
            } else {
                const errorMsg = result.message || result.error || 'Failed to create keyword';
                showNotification(errorMsg, 'error');
            }
            
            // Re-enable button
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalBtnContent;
            }
        }
    } catch (error) {
        console.error('Error creating keyword:', error);
        showNotification('An error occurred while creating the keyword. Please try again.', 'error');
        
        // Re-enable button
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalBtnContent;
        }
    }
}

/**
 * Display validation errors on form fields
 */
function displayValidationErrors(errors) {
    const form = document.getElementById('keywordForm');
    
    // Clear previous errors
    form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
    form.querySelectorAll('.invalid-feedback').forEach(el => el.style.display = 'none');
    
    // Display new errors
    Object.keys(errors).forEach(key => {
        const fieldName = key.charAt(0).toLowerCase() + key.slice(1); // Convert to camelCase
        const field = document.getElementById(fieldName) || document.getElementById(key);
        
        if (field) {
            field.classList.add('is-invalid');
            
            let errorMessage = errors[key];
            if (Array.isArray(errorMessage)) {
                errorMessage = errorMessage[0];
            }
            
            showFieldFeedback(field, errorMessage, 'invalid');
        }
    });
}

/**
 * Load existing keywords for reference/validation
 */
async function loadExistingKeywords() {
    try {
        const url = window.AppUrls.buildUrl(window.AppUrls.buildApiUrl(window.AppUrls.api.keywords.list), {
            pageNumber: 1,
            pageSize: 100
        });
        
        const response = await fetch(url, {
            method: 'GET',
            headers: getAjaxHeaders()
        });

        if (response.ok) {
            const result = await response.json();
            const keywords = result.data?.items || result.items || [];
            
            // Store for validation
            window.existingKeywords = keywords.map(k => k.keywordText || k.keyword);
            
            console.log(`Loaded ${keywords.length} existing keywords for validation`);
        }
    } catch (error) {
        console.error('Error loading existing keywords:', error);
        // Non-critical error, continue without existing keywords list
    }
}

// Initialize on document ready
document.addEventListener('DOMContentLoaded', initKeywordCreate);
