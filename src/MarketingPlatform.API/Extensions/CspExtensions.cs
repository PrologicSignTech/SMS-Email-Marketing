namespace MarketingPlatform.API.Extensions
{
    /// <summary>
    /// Extension methods for Content Security Policy (CSP) nonce access in views and controllers.
    /// </summary>
    public static class CspExtensions
    {
        private const string CspNonceKey = "csp-nonce";

        /// <summary>
        /// Retrieves the CSP nonce for the current request.
        /// This nonce should be used in inline script and style tags to comply with CSP.
        /// </summary>
        /// <param name="context">The HttpContext for the current request</param>
        /// <returns>The CSP nonce string, or empty string if not available</returns>
        /// <example>
        /// Usage in Razor views:
        /// <code>
        /// &lt;script nonce="@Context.GetCspNonce()"&gt;
        ///     // Your inline JavaScript code
        /// &lt;/script&gt;
        /// 
        /// &lt;style nonce="@Context.GetCspNonce()"&gt;
        ///     /* Your inline CSS */
        /// &lt;/style&gt;
        /// </code>
        /// 
        /// Or access directly via Items:
        /// <code>
        /// &lt;script nonce="@Context.Items["csp-nonce"]"&gt;
        ///     // Your inline JavaScript code
        /// &lt;/script&gt;
        /// </code>
        /// </example>
        public static string GetCspNonce(this HttpContext context)
        {
            if (context?.Items != null && context.Items.TryGetValue(CspNonceKey, out var nonce))
            {
                return nonce?.ToString() ?? string.Empty;
            }
            
            return string.Empty;
        }

        /// <summary>
        /// Checks if a CSP nonce is available for the current request.
        /// </summary>
        /// <param name="context">The HttpContext for the current request</param>
        /// <returns>True if a nonce is available, false otherwise</returns>
        public static bool HasCspNonce(this HttpContext context)
        {
            return context?.Items?.ContainsKey(CspNonceKey) ?? false;
        }
    }
}
