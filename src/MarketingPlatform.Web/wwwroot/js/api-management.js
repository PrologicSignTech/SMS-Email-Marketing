// API Management - Swagger-like interface for testing APIs
(function() {
    'use strict';

    let allEndpoints = [];
    let allControllers = [];
    let currentEndpoint = null;

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function() {
        loadEndpoints();
        setupEventListeners();
    });

    function setupEventListeners() {
        // Search
        document.getElementById('search-endpoints')?.addEventListener('input', filterEndpoints);

        // Filters
        document.getElementById('filter-method')?.addEventListener('change', filterEndpoints);
        document.getElementById('filter-controller')?.addEventListener('change', filterEndpoints);

        // Refresh button
        document.getElementById('refresh-endpoints')?.addEventListener('click', loadEndpoints);

        // Send request button
        document.getElementById('send-request')?.addEventListener('click', sendRequest);

        // Add header button
        document.getElementById('add-header')?.addEventListener('click', addHeaderRow);
    }

    async function loadEndpoints() {
        try {
            showLoading(true);

            // Load endpoints
            const endpointsResponse = await fetch(window.appConfig.apiBaseUrl + '/api/apidocs/endpoints', {
                credentials: 'include'
            });

            if (!endpointsResponse.ok) {
                throw new Error('Failed to load endpoints');
            }

            const endpointsData = await endpointsResponse.json();
            allEndpoints = endpointsData.data || [];

            // Load controllers
            const controllersResponse = await fetch(window.appConfig.apiBaseUrl + '/api/apidocs/controllers', {
                credentials: 'include'
            });

            if (controllersResponse.ok) {
                const controllersData = await controllersResponse.json();
                allControllers = controllersData.data || [];
                populateControllerFilter();
            }

            updateStats();
            renderEndpoints(allEndpoints);
            showLoading(false);

        } catch (error) {
            console.error('Error loading endpoints:', error);
            showError('Failed to load API endpoints. Please check your connection.');
            showLoading(false);
        }
    }

    function updateStats() {
        const totalEndpoints = allEndpoints.length;
        const protectedEndpoints = allEndpoints.filter(e => e.requiresAuth).length;
        const publicEndpoints = totalEndpoints - protectedEndpoints;
        const totalControllers = allControllers.length;

        document.getElementById('total-endpoints').textContent = totalEndpoints;
        document.getElementById('total-controllers').textContent = totalControllers;
        document.getElementById('protected-endpoints').textContent = protectedEndpoints;
        document.getElementById('public-endpoints').textContent = publicEndpoints;
    }

    function populateControllerFilter() {
        const filterSelect = document.getElementById('filter-controller');
        if (!filterSelect) return;

        // Clear existing options (except "All Controllers")
        filterSelect.innerHTML = '<option value="">All Controllers</option>';

        // Add controller options
        allControllers.forEach(controller => {
            const option = document.createElement('option');
            option.value = controller.name;
            option.textContent = `${controller.name} (${controller.endpointCount})`;
            filterSelect.appendChild(option);
        });
    }

    function filterEndpoints() {
        const searchTerm = document.getElementById('search-endpoints')?.value.toLowerCase() || '';
        const methodFilter = document.getElementById('filter-method')?.value || '';
        const controllerFilter = document.getElementById('filter-controller')?.value || '';

        let filtered = allEndpoints;

        // Apply search filter
        if (searchTerm) {
            filtered = filtered.filter(e =>
                e.path.toLowerCase().includes(searchTerm) ||
                e.controller.toLowerCase().includes(searchTerm) ||
                e.action.toLowerCase().includes(searchTerm) ||
                e.description.toLowerCase().includes(searchTerm)
            );
        }

        // Apply method filter
        if (methodFilter) {
            filtered = filtered.filter(e => e.method.includes(methodFilter));
        }

        // Apply controller filter
        if (controllerFilter) {
            filtered = filtered.filter(e => e.controller === controllerFilter);
        }

        renderEndpoints(filtered);
    }

    function renderEndpoints(endpoints) {
        const container = document.getElementById('endpoints-container');
        if (!container) return;

        if (endpoints.length === 0) {
            container.innerHTML = `
                <div class="text-center py-5">
                    <i class="bi bi-search fs-1 text-muted"></i>
                    <p class="text-muted mt-3">No endpoints found matching your filters</p>
                </div>
            `;
            container.style.display = 'block';
            return;
        }

        // Group by controller
        const grouped = endpoints.reduce((acc, endpoint) => {
            if (!acc[endpoint.controller]) {
                acc[endpoint.controller] = [];
            }
            acc[endpoint.controller].push(endpoint);
            return acc;
        }, {});

        let html = '';

        Object.keys(grouped).sort().forEach(controller => {
            html += `
                <div class="controller-group">
                    <div class="controller-header p-3 bg-light border-bottom">
                        <h6 class="mb-0">
                            <i class="bi bi-folder"></i> ${controller}
                            <span class="badge bg-secondary ms-2">${grouped[controller].length}</span>
                        </h6>
                    </div>
            `;

            grouped[controller].forEach(endpoint => {
                const methods = endpoint.method.split(', ');
                const methodBadges = methods.map(m =>
                    `<span class="method-badge ${m.trim()}">${m.trim()}</span>`
                ).join('');

                const authBadge = endpoint.requiresAuth
                    ? `<span class="auth-badge required"><i class="bi bi-lock"></i> ${endpoint.roles}</span>`
                    : `<span class="auth-badge public"><i class="bi bi-unlock"></i> Public</span>`;

                const parametersHtml = endpoint.parameters.length > 0
                    ? `<div class="mt-2"><small class="text-muted">
                        Parameters: ${endpoint.parameters.map(p => `<code>${p.name}: ${p.type}</code>`).join(', ')}
                       </small></div>`
                    : '';

                html += `
                    <div class="endpoint-item" data-endpoint='${JSON.stringify(endpoint)}'>
                        <div class="d-flex justify-content-between align-items-start">
                            <div class="flex-grow-1">
                                <div class="mb-2">
                                    ${methodBadges}
                                    <code class="text-primary">${endpoint.path}</code>
                                    ${authBadge}
                                </div>
                                <p class="mb-0 text-muted">${endpoint.description}</p>
                                ${parametersHtml}
                            </div>
                            <button class="btn btn-sm btn-outline-primary test-endpoint-btn">
                                <i class="bi bi-play-circle"></i> Test
                            </button>
                        </div>
                    </div>
                `;
            });

            html += '</div>';
        });

        container.innerHTML = html;
        container.style.display = 'block';

        // Add click handlers for test buttons
        container.querySelectorAll('.test-endpoint-btn').forEach(btn => {
            btn.addEventListener('click', function(e) {
                e.stopPropagation();
                const endpointItem = this.closest('.endpoint-item');
                const endpointData = JSON.parse(endpointItem.dataset.endpoint);
                openTestModal(endpointData);
            });
        });
    }

    function openTestModal(endpoint) {
        currentEndpoint = endpoint;

        // Set modal title
        const methods = endpoint.method.split(', ');
        const primaryMethod = methods[0].trim();

        document.getElementById('modal-method').textContent = primaryMethod;
        document.getElementById('modal-method').className = `method-badge ${primaryMethod}`;
        document.getElementById('modal-path').textContent = endpoint.path;

        // Show/hide request body section for POST/PUT/PATCH
        const requestBodySection = document.getElementById('request-body-section');
        if (['POST', 'PUT', 'PATCH'].includes(primaryMethod)) {
            requestBodySection.style.display = 'block';

            // Generate sample request body
            if (endpoint.parameters.length > 0) {
                const sampleBody = {};
                endpoint.parameters.forEach(param => {
                    sampleBody[param.name] = getSampleValue(param.type);
                });
                document.getElementById('request-body').value = JSON.stringify(sampleBody, null, 2);
            }
        } else {
            requestBodySection.style.display = 'none';
        }

        // Reset response
        document.getElementById('response-container').style.display = 'none';
        document.getElementById('response-placeholder').style.display = 'block';

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('apiTestModal'));
        modal.show();
    }

    async function sendRequest() {
        if (!currentEndpoint) return;

        const methods = currentEndpoint.method.split(', ');
        const method = methods[0].trim();

        try {
            // Build request
            const headers = {};
            document.querySelectorAll('#request-headers .header-row').forEach(row => {
                const nameInput = row.querySelector('input:nth-of-type(1)');
                const valueInput = row.querySelector('input:nth-of-type(2)');
                if (nameInput && valueInput && nameInput.value && valueInput.value) {
                    headers[nameInput.value] = valueInput.value;
                }
            });

            const requestOptions = {
                method: method,
                headers: headers,
                credentials: 'include'
            };

            // Add body for POST/PUT/PATCH
            if (['POST', 'PUT', 'PATCH'].includes(method)) {
                const bodyText = document.getElementById('request-body').value;
                if (bodyText) {
                    requestOptions.body = bodyText;
                }
            }

            // Build URL
            let url = window.appConfig.apiBaseUrl + currentEndpoint.path;

            // Replace path parameters if any
            // For now, we'll just use the URL as-is

            // Send request
            const startTime = Date.now();
            const response = await fetch(url, requestOptions);
            const endTime = Date.now();
            const responseTime = endTime - startTime;

            // Get response body
            const contentType = response.headers.get('content-type');
            let responseBody;

            if (contentType && contentType.includes('application/json')) {
                responseBody = await response.json();
            } else {
                responseBody = await response.text();
            }

            // Display response
            displayResponse(response.status, response.statusText, responseBody, responseTime);

        } catch (error) {
            console.error('Error sending request:', error);
            displayResponse(0, 'Error', { error: error.message }, 0);
        }
    }

    function displayResponse(status, statusText, body, responseTime) {
        document.getElementById('response-placeholder').style.display = 'none';
        document.getElementById('response-container').style.display = 'block';

        // Status badge
        const statusBadge = document.getElementById('response-status');
        statusBadge.textContent = `${status} ${statusText}`;
        statusBadge.className = 'badge';

        if (status >= 200 && status < 300) {
            statusBadge.classList.add('bg-success');
        } else if (status >= 400 && status < 500) {
            statusBadge.classList.add('bg-warning');
        } else if (status >= 500) {
            statusBadge.classList.add('bg-danger');
        } else {
            statusBadge.classList.add('bg-secondary');
        }

        // Response time
        document.getElementById('response-time').textContent = `${responseTime}ms`;

        // Response body
        let formattedBody;
        if (typeof body === 'object') {
            formattedBody = JSON.stringify(body, null, 2);
        } else {
            formattedBody = body;
        }

        document.getElementById('response-body').textContent = formattedBody;
    }

    function addHeaderRow() {
        const container = document.getElementById('request-headers');
        const row = document.createElement('div');
        row.className = 'header-row mb-2';
        row.innerHTML = `
            <div class="row">
                <div class="col-5">
                    <input type="text" class="form-control form-control-sm" placeholder="Header name">
                </div>
                <div class="col-6">
                    <input type="text" class="form-control form-control-sm" placeholder="Header value">
                </div>
                <div class="col-1">
                    <button class="btn btn-sm btn-outline-danger remove-header-btn">
                        <i class="bi bi-x"></i>
                    </button>
                </div>
            </div>
        `;

        container.appendChild(row);

        // Add remove handler
        row.querySelector('.remove-header-btn').addEventListener('click', function() {
            row.remove();
        });
    }

    function getSampleValue(type) {
        switch (type.toLowerCase()) {
            case 'string': return 'string';
            case 'int32':
            case 'int64':
            case 'integer': return 0;
            case 'boolean': return true;
            case 'datetime': return new Date().toISOString();
            case 'decimal':
            case 'double': return 0.0;
            default: return null;
        }
    }

    function showLoading(show) {
        const loading = document.getElementById('endpoints-loading');
        const container = document.getElementById('endpoints-container');

        if (show) {
            loading.style.display = 'block';
            container.style.display = 'none';
        } else {
            loading.style.display = 'none';
        }
    }

    function showError(message) {
        const container = document.getElementById('endpoints-container');
        container.innerHTML = `
            <div class="alert alert-danger m-3" role="alert">
                <i class="bi bi-exclamation-triangle"></i> ${message}
            </div>
        `;
        container.style.display = 'block';
    }

})();
