using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Services;

/// <summary>
/// HTTP client service for making requests to the Marketing Platform API
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configure base address from appsettings
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7011/api";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public void SetAuthorizationToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthorizationToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET request to {Endpoint}", endpoint);

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);

            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);

            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("PUT request to {Endpoint}", endpoint);

            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PUT request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("DELETE request to {Endpoint}", endpoint);

            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DELETE request to {Endpoint}", endpoint);
            throw;
        }
    }

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("API request failed with status {StatusCode}: {Content}",
                response.StatusCode, content);

            // Try to deserialize error response
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
                throw new HttpRequestException($"API Error: {errorResponse?.Message ?? response.ReasonPhrase}");
            }
            catch (JsonException)
            {
                throw new HttpRequestException($"API Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response: {Content}", content);
            throw;
        }
    }
}
