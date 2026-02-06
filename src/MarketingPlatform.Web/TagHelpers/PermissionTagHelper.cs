using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace MarketingPlatform.Web.TagHelpers
{
    [HtmlTargetElement("*", Attributes = "require-permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("require-permission")]
        public string? RequirePermission { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(RequirePermission))
            {
                return;
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                output.SuppressOutput();
                return;
            }

            // Check if user is SuperAdmin (has all permissions)
            if (user.IsInRole("SuperAdmin"))
            {
                return;
            }

            // Get user's permissions from claims
            var permissionsClaim = user.FindFirst("Permissions")?.Value;
            if (string.IsNullOrEmpty(permissionsClaim))
            {
                output.SuppressOutput();
                return;
            }

            // Parse required permission
            if (!Enum.TryParse<MarketingPlatform.Core.Enums.Permission>(RequirePermission, out var requiredPermission))
            {
                output.SuppressOutput();
                return;
            }

            // Check if user has the required permission
            if (long.TryParse(permissionsClaim, out long userPermissions))
            {
                if ((userPermissions & (long)requiredPermission) == 0)
                {
                    output.SuppressOutput();
                }
            }
            else
            {
                output.SuppressOutput();
            }
        }
    }
}
