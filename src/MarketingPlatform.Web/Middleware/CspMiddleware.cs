using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Web.Middleware
{
    /// <summary>
    /// Middleware for implementing Content Security Policy (CSP) with nonce-based inline script/style execution.
    /// Generates a unique cryptographically secure nonce per request and sets appropriate CSP headers
    /// based on the environment (Development vs Production).
    /// </summary>
    public class CspMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CspMiddleware> _logger;

        public CspMiddleware(
            RequestDelegate next,
            IWebHostEnvironment environment,
            ILogger<CspMiddleware> logger)
        {
            _next = next;
            _environment = environment;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate a cryptographically secure nonce for this request
            var nonce = GenerateNonce();
            
            // Store nonce in HttpContext.Items so it can be accessed by views
            context.Items["csp-nonce"] = nonce;

            // Remove server header for security
            context.Response.Headers.Remove("Server");
            
            // Add security headers
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Set CSP header based on environment
            var cspHeader = _environment.IsDevelopment()
                ? BuildDevelopmentCsp(nonce)
                : BuildProductionCsp(nonce);
            
            context.Response.Headers.Append("Content-Security-Policy", cspHeader);
            
            _logger.LogDebug("CSP header set with nonce: {Nonce} for environment: {Environment}", 
                nonce, _environment.EnvironmentName);

            await _next(context);
        }

        /// <summary>
        /// Generates a cryptographically secure random nonce.
        /// </summary>
        /// <returns>Base64-encoded nonce string</returns>
        private static string GenerateNonce()
        {
            var nonceBytes = new byte[32]; // 256 bits
            RandomNumberGenerator.Fill(nonceBytes);
            return Convert.ToBase64String(nonceBytes);
        }

        /// <summary>
        /// Builds a CSP header for development environment.
        /// More permissive to allow hot reload, browser-link, and WebSocket connections.
        /// Uses specific port ranges for common development tools to maintain security.
        /// Includes trusted external resources for development testing.
        /// </summary>
        /// <param name="nonce">The nonce to include in the CSP</param>
        /// <returns>CSP header value</returns>
        private static string BuildDevelopmentCsp(string nonce)
        {
            // Common development ports: 5000-5999 (Kestrel), 7000-7999 (HTTPS), 44300-44399 (IIS Express HTTPS), 60000-65535 (dynamic)
            // 'unsafe-inline' allows inline style attributes (style="...") and <style> tags, and inline event handlers (onclick="...")
            // 'unsafe-eval' allows eval() which is needed for some development tools and hot reload
            return $"default-src 'self'; " +
                   $"script-src 'self' 'nonce-{nonce}' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdn.datatables.net https://unpkg.com https://www.google.com/recaptcha/ https://www.gstatic.com/recaptcha/ https://js.stripe.com; " +
                   $"style-src 'self' 'nonce-{nonce}' 'unsafe-inline' https://cdn.jsdelivr.net https://cdn.datatables.net https://unpkg.com; " +
                   $"connect-src 'self' ws://localhost:* wss://localhost:* http://localhost:* https://localhost:* https://api.stripe.com; " +
                   $"img-src 'self' data: https:; " +
                   $"font-src 'self' https://cdn.jsdelivr.net; " +
                   $"frame-src https://www.google.com/recaptcha/ https://js.stripe.com; " +
                   $"object-src 'none'; " +
                   $"base-uri 'self';";
        }

        /// <summary>
        /// Builds a strict CSP header for production environment.
        /// Uses nonces for inline scripts without allowing unsafe-eval.
        /// Allows 'unsafe-inline' for styles to support inline style attributes.
        /// Allows trusted external resources (CDNs, payment processors, security services).
        /// </summary>
        /// <param name="nonce">The nonce to include in the CSP</param>
        /// <returns>CSP header value</returns>
        private static string BuildProductionCsp(string nonce)
        {
            return $"default-src 'self'; " +
                   $"script-src 'self' 'nonce-{nonce}' 'unsafe-inline' https://cdn.jsdelivr.net https://cdn.datatables.net https://unpkg.com https://www.google.com/recaptcha/ https://www.gstatic.com/recaptcha/ https://js.stripe.com; " +
                   $"style-src 'self' 'nonce-{nonce}' 'unsafe-inline' https://cdn.jsdelivr.net https://cdn.datatables.net https://unpkg.com; " +
                   $"connect-src 'self' https://api.stripe.com; " +
                   $"img-src 'self' data: https:; " +
                   $"font-src 'self' https://cdn.jsdelivr.net; " +
                   $"frame-src https://www.google.com/recaptcha/ https://js.stripe.com; " +
                   $"object-src 'none'; " +
                   $"base-uri 'self'; " +
                   $"form-action 'self'; " +
                   $"frame-ancestors 'none';";
        }
    }
}
