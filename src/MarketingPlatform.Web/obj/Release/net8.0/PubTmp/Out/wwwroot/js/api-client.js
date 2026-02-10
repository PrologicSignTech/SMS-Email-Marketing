/**
 * API Client - Centralized API communication with JWT token handling
 */

class ApiClient {
    constructor() {
        this.baseUrl = (window.authConfig?.apiBaseUrl || window.appConfig?.apiBaseUrl)+'/api';
    }

    /**
     * Get authentication token from localStorage
     */
    getAuthToken() {
        return localStorage.getItem('authToken');
    }

    /**
     * Get refresh token from localStorage
     */
    getRefreshToken() {
        return localStorage.getItem('refreshToken');
    }

    /**
     * Set authentication token
     */
    setAuthToken(token) {
        localStorage.setItem('authToken', token);
    }

    /**
     * Clear authentication tokens
     */
    clearTokens() {
        localStorage.removeItem('authToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('userEmail');
    }

    /**
     * Check if user is authenticated
     */
    isAuthenticated() {
        return !!this.getAuthToken();
    }

    /**
     * Make an authenticated API request
     */
    async request(endpoint, options = {}) {
        const token = this.getAuthToken();
        const url = endpoint.startsWith('http') ? endpoint : `${this.baseUrl}${endpoint}`;
        
        const headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };

        // Add Authorization header if token exists
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const config = {
            ...options,
            headers
        };

        try {
            const response = await fetch(url, config);

            // Handle 401 Unauthorized - try to refresh token
            if (response.status === 401 && this.getRefreshToken()) {
                const refreshed = await this.refreshAuthToken();
                if (refreshed) {
                    // Retry the original request with new token
                    headers['Authorization'] = `Bearer ${this.getAuthToken()}`;
                    return await fetch(url, { ...config, headers });
                } else {
                    // Refresh failed, redirect to login
                    this.handleAuthError();
                    throw new Error('Session expired. Please login again.');
                }
            }

            return response;
        } catch (error) {
            console.error('API request failed:', error);
            throw error;
        }
    }

    /**
     * Refresh authentication token
     */
    async refreshAuthToken() {
        try {
            const refreshToken = this.getRefreshToken();
            if (!refreshToken) return false;

            const response = await fetch(`${this.baseUrl}/auth/refresh-token`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ refreshToken })
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success && data.data.token) {
                    this.setAuthToken(data.data.token);
                    
                    // Update server-side cookie
                    await fetch('/auth/login-callback', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            token: data.data.token,
                            refreshToken: data.data.refreshToken || refreshToken,
                            email: localStorage.getItem('userEmail'),
                            rememberMe: true
                        })
                    });

                    return true;
                }
            }

            return false;
        } catch (error) {
            console.error('Token refresh failed:', error);
            return false;
        }
    }

    /**
     * Handle authentication errors
     */
    handleAuthError() {
        this.clearTokens();
        window.location.href = window.AppUrls?.auth?.login || '/auth/login';
    }

    /**
     * GET request
     */
    async get(endpoint, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'GET'
        });
    }

    /**
     * POST request
     */
    async post(endpoint, data, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'POST',
            body: JSON.stringify(data)
        });
    }

    /**
     * PUT request
     */
    async put(endpoint, data, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'PUT',
            body: JSON.stringify(data)
        });
    }

    /**
     * DELETE request
     */
    async delete(endpoint, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'DELETE'
        });
    }
}

// Create global instance
window.apiClient = new ApiClient();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ApiClient;
}