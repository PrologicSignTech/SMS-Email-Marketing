using System.Text.RegularExpressions;

namespace MarketingPlatform.Web.Extensions
{
    /// <summary>
    /// Transforms route values to use kebab-case (lowercase with hyphens)
    /// Converts PascalCase controller and action names to kebab-case
    /// Example: ForgotPassword -> forgot-password
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null)
                return null;

            // Convert PascalCase to kebab-case
            var str = value.ToString();
            if (string.IsNullOrEmpty(str))
                return str;

            // Insert a hyphen before each uppercase letter (except the first one)
            // and convert to lowercase
            return Regex.Replace(str, "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
}
