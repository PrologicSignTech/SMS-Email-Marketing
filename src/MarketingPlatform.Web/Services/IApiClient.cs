using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Services;

/// <summary>
/// Generic API client interface for making HTTP requests to the Marketing Platform API
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Make a GET request to the API
    /// </summary>
    Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Make a POST request to the API
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Make a PUT request to the API
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Make a DELETE request to the API
    /// </summary>
    Task<TResponse?> DeleteAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Make a POST request without expecting a response body
    /// </summary>
    Task<bool> PostAsync<TRequest>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the authorization token for subsequent requests
    /// </summary>
    void SetAuthorizationToken(string token);

    /// <summary>
    /// Clear the authorization token
    /// </summary>
    void ClearAuthorizationToken();
}
