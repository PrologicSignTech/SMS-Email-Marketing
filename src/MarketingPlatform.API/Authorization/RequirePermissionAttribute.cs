using MarketingPlatform.Core.Enums;
using Microsoft.AspNetCore.Authorization;

namespace MarketingPlatform.API.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public Permission Permission { get; }
        
        public RequirePermissionAttribute(Permission permission)
        {
            Permission = permission;
            Policy = $"Permission:{permission}";
        }
    }
}
