// Register.js - Registration flow with OTP verification
let registrationData = {};
let countdownInterval;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    loadRecaptchaConfig();
    initializePasswordToggle();
    initializeRegistrationForm();
    initializeOtpForm();
    initializeResendOtp();
});

async function loadRecaptchaConfig() {
    try {
        if (typeof window.authConfig !== 'undefined' && window.authConfig.recaptchaSiteKey) {
            renderRecaptcha(window.authConfig.recaptchaSiteKey);
        } else {
            const recaptchaConfigUrl = window.AppUrls ? window.AppUrls.buildApiUrl(window.AppUrls.api.auth.recaptchaConfig) : '/api/auth/recaptcha-config';
                 
            const response = await fetch(recaptchaConfigUrl);
            if (response.ok) {
                const config = await response.json();
                renderRecaptcha(config.siteKey);
            }
        }
    } catch (error) {
        console.error('Error loading reCAPTCHA config:', error);
    }
}

function renderRecaptcha(siteKey) {
    const recaptchaContainer = document.querySelector('.g-recaptcha');
    if (recaptchaContainer && siteKey ) {
        recaptchaContainer.setAttribute('data-sitekey', siteKey);
    }
}

function initializePasswordToggle() {
    const toggleButton = document.getElementById('togglePassword');
    if (!toggleButton) return;
    toggleButton.addEventListener('click', function() {
        const passwordInput = document.getElementById('password');
        const icon = this.querySelector('i');
        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            icon.classList.replace('bi-eye', 'bi-eye-slash');
        } else {
            passwordInput.type = 'password';
            icon.classList.replace('bi-eye-slash', 'bi-eye');
        }
    });
}

function initializeRegistrationForm() {
    const registerForm = document.getElementById('registerForm');
    if (!registerForm) return;
    registerForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;
        const recaptchaResponse = grecaptcha.getResponse();
        const errorDiv = document.getElementById('errorMessage');
        const successDiv = document.getElementById('successMessage');
        const registerBtn = document.getElementById('registerBtn');
        
        if (errorDiv) errorDiv.classList.add('d-none');
        if (successDiv) successDiv.classList.add('d-none');
        if (password !== confirmPassword) {
            if (errorDiv) {
                errorDiv.textContent = 'Passwords do not match';
                errorDiv.classList.remove('d-none');
            }
            return;
        }
        if (!recaptchaResponse) {
            if (errorDiv) {
                errorDiv.textContent = 'Please complete the reCAPTCHA verification';
                errorDiv.classList.remove('d-none');
            }
            return;
        }
        if (registerBtn) {
            registerBtn.disabled = true;
            registerBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Creating account...';
        }
        registrationData = {
            firstName: document.getElementById('firstName').value,
            lastName: document.getElementById('lastName').value,
            email: email,
            phoneNumber: document.getElementById('phoneNumber').value,
            password: password,
            company: document.getElementById('company').value,
            recaptchaToken: recaptchaResponse
        };
        try {
            // Build full API URL using AppUrls helper
            const registerUrl = window.AppUrls ? 
                window.AppUrls.buildApiUrl(window.AppUrls.api.auth.register) : 
                '/api/auth/register';
            
            const response = await fetch(registerUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(registrationData)
            });
            const data = await response.json();
            if (response.ok) {
                const displayEmailEl = document.getElementById('displayEmail');
                if (displayEmailEl) displayEmailEl.textContent = email;
                goToStep(2);
                startCountdown();
            } else {
                if (errorDiv) {
                    errorDiv.textContent = data.message || 'Registration failed. Please try again.';
                    errorDiv.classList.remove('d-none');
                }
                grecaptcha.reset();
            }
        } catch (error) {
            console.error('Registration error:', error);
            if (errorDiv) {
                errorDiv.textContent = 'An error occurred. Please try again.';
                errorDiv.classList.remove('d-none');
            }
            grecaptcha.reset();
        } finally {
            if (registerBtn) {
                registerBtn.disabled = false;
                registerBtn.innerHTML = '<i class="bi bi-person-check"></i> Create Account';
            }
        }
    });
}

function initializeOtpForm() {
    const otpInputs = document.querySelectorAll('.otp-input');
    otpInputs.forEach((input, index) => {
        input.addEventListener('input', function(e) {
            if (this.value.length === 1 && index < otpInputs.length - 1) {
                otpInputs[index + 1].focus();
            }
        });
        input.addEventListener('keydown', function(e) {
            if (e.key === 'Backspace' && this.value === '' && index > 0) {
                otpInputs[index - 1].focus();
            }
        });
        input.addEventListener('paste', function(e) {
            e.preventDefault();
            const pastedData = e.clipboardData.getData('text');
            const digits = pastedData.replace(/\D/g, '').split('');
            digits.forEach((digit, i) => {
                if (index + i < otpInputs.length) {
                    otpInputs[index + i].value = digit;
                }
            });
            if (index + digits.length < otpInputs.length) {
                otpInputs[index + digits.length].focus();
            }
        });
    });
    const otpForm = document.getElementById('otpForm');
    if (!otpForm) return;
    otpForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        const otp = Array.from(otpInputs).map(input => input.value).join('');
        const errorDiv = document.getElementById('errorMessage');
        const successDiv = document.getElementById('successMessage');
        const verifyBtn = document.getElementById('verifyBtn');
        
        if (errorDiv) errorDiv.classList.add('d-none');
        if (successDiv) successDiv.classList.add('d-none');
        if (otp.length !== 6) {
            if (errorDiv) {
                errorDiv.textContent = 'Please enter the complete 6-digit code';
                errorDiv.classList.remove('d-none');
            }
            return;
        }
        if (verifyBtn) {
            verifyBtn.disabled = true;
            verifyBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Verifying...';
        }
        try {
            const verifyUrl = window.AppUrls ? 
                window.AppUrls.buildApiUrl(window.AppUrls.api.auth.verifyEmail) : 
                '/api/auth/verify-email';
            
            const response = await fetch(verifyUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: registrationData.email, otp: otp })
            });
            const data = await response.json();
            if (response.ok) {
                clearInterval(countdownInterval);
                goToStep(3);
            } else {
                if (errorDiv) {
                    errorDiv.textContent = data.message || 'Invalid verification code';
                    errorDiv.classList.remove('d-none');
                }
                otpInputs.forEach(input => input.value = '');
                otpInputs[0].focus();
            }
        } catch (error) {
            console.error('Verification error:', error);
            if (errorDiv) {
                errorDiv.textContent = 'An error occurred. Please try again.';
                errorDiv.classList.remove('d-none');
            }
        } finally {
            if (verifyBtn) {
                verifyBtn.disabled = false;
                verifyBtn.innerHTML = '<i class="bi bi-check-circle"></i> Verify Email';
            }
        }
    });
}

function initializeResendOtp() {
    const resendBtn = document.getElementById('resendOtpBtn');
    if (!resendBtn) return;
    resendBtn.addEventListener('click', async function() {
        const errorDiv = document.getElementById('errorMessage');
        const successDiv = document.getElementById('successMessage');
        const btn = this;
        btn.disabled = true;
        btn.textContent = 'Sending...';
        try {
            const resendUrl = window.AppUrls ? 
                window.AppUrls.buildApiUrl(window.AppUrls.api.auth.resendOtp) : 
                '/api/auth/resend-otp';
            
            const response = await fetch(resendUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: registrationData.email })
            });
            if (response.ok) {
                if (successDiv) {
                    successDiv.textContent = 'Verification code resent successfully!';
                    successDiv.classList.remove('d-none');
                }
                if (errorDiv) errorDiv.classList.add('d-none');
                startCountdown();
                setTimeout(() => { if (successDiv) successDiv.classList.add('d-none'); }, 3000);
            } else {
                if (errorDiv) {
                    errorDiv.textContent = 'Failed to resend code. Please try again.';
                    errorDiv.classList.remove('d-none');
                }
            }
        } catch (error) {
            if (errorDiv) {
                errorDiv.textContent = 'An error occurred. Please try again.';
                errorDiv.classList.remove('d-none');
            }
        } finally {
            btn.disabled = false;
            btn.textContent = 'Resend';
        }
    });
}

function goToStep(stepNumber) {
    document.querySelectorAll('.form-step').forEach(step => step.classList.remove('active'));
    document.getElementById(`step${stepNumber}`).classList.add('active');
    document.querySelectorAll('.step').forEach((step, index) => {
        step.classList.remove('active', 'completed');
        if (index + 1 < stepNumber) {
            step.classList.add('completed');
        } else if (index + 1 === stepNumber) {
            step.classList.add('active');
        }
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function startCountdown() {
    let timeLeft = 300;
    const countdownEl = document.getElementById('countdown');
    if (!countdownEl) return;
    
    clearInterval(countdownInterval);
    countdownInterval = setInterval(() => {
        const minutes = Math.floor(timeLeft / 60);
        const seconds = timeLeft % 60;
        countdownEl.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;
        if (timeLeft === 0) {
            clearInterval(countdownInterval);
            countdownEl.textContent = 'Expired';
        }
        timeLeft--;
    }, 1000);
}
