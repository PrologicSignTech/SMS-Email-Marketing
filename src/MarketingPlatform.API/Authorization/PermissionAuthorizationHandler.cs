using MarketingPlatform.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MarketingPlatform.API.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Check if user has SuperAdmin role (has all permissions)
            var isSuperAdmin = context.User.IsInRole("SuperAdmin");
            if (isSuperAdmin)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            
            // Get user's permissions from claims
            var permissionsClaim = context.User.FindFirst("Permissions")?.Value;
            if (string.IsNullOrEmpty(permissionsClaim))
            {
                return Task.CompletedTask;
            }
            
            if (long.TryParse(permissionsClaim, out long userPermissions))
            {
                // Check if user has the required permission
                if ((userPermissions & (long)requirement.Permission) != 0)
                {
                    context.Succeed(requirement);
                }
            }
            
            return Task.CompletedTask;
        }
    }
    
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public Permission Permission { get; }
        
        public PermissionRequirement(Permission permission)
        {
            Permission = permission;
        }
    }
}
