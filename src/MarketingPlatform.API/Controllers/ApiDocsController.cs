using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MarketingPlatform.Application.DTOs.Common;
using System.Reflection;

namespace MarketingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class ApiDocsController : ControllerBase
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly ILogger<ApiDocsController> _logger;

        public ApiDocsController(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ILogger<ApiDocsController> logger)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _logger = logger;
        }

        [HttpGet("endpoints")]
        public ActionResult<ApiResponse<List<ApiEndpointInfo>>> GetAllEndpoints()
        {
            try
            {
                var endpoints = new List<ApiEndpointInfo>();
                var routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                    .OfType<ControllerActionDescriptor>()
                    .Where(x => x.AttributeRouteInfo != null)
                    .OrderBy(x => x.ControllerName)
                    .ThenBy(x => x.ActionName);

                foreach (var route in routes)
                {
                    var httpMethods = route.ActionConstraints?
                        .OfType<Microsoft.AspNetCore.Mvc.ActionConstraints.HttpMethodActionConstraint>()
                        .FirstOrDefault()?.HttpMethods ?? new[] { "GET" };

                    var parameters = route.Parameters
                        .Select(p => new ApiParameterInfo
                        {
                            Name = p.Name,
                            Type = p.ParameterType.Name,
                            IsRequired = !IsNullable(p.ParameterType)
                        }).ToList();

                    // Get authorization requirements
                    var authorizeAttribute = route.MethodInfo.GetCustomAttribute<AuthorizeAttribute>()
                        ?? route.ControllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>();

                    var allowAnonymous = route.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null
                        || route.ControllerTypeInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null;

                    var endpoint = new ApiEndpointInfo
                    {
                        Controller = route.ControllerName,
                        Action = route.ActionName,
                        Method = string.Join(", ", httpMethods),
                        Path = "/" + route.AttributeRouteInfo.Template,
                        Parameters = parameters,
                        RequiresAuth = !allowAnonymous,
                        Roles = authorizeAttribute?.Roles ?? "Any Authenticated User",
                        Description = GetActionDescription(route)
                    };

                    endpoints.Add(endpoint);
                }

                return Ok(ApiResponse<List<ApiEndpointInfo>>.SuccessResponse(endpoints, "Endpoints retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API endpoints");
                return BadRequest(ApiResponse<List<ApiEndpointInfo>>.ErrorResponse("Failed to retrieve endpoints"));
            }
        }

        [HttpGet("controllers")]
        public ActionResult<ApiResponse<List<ApiControllerInfo>>> GetAllControllers()
        {
            try
            {
                var controllers = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                    .OfType<ControllerActionDescriptor>()
                    .GroupBy(x => x.ControllerName)
                    .Select(g => new ApiControllerInfo
                    {
                        Name = g.Key,
                        EndpointCount = g.Count(),
                        Actions = g.Select(a => a.ActionName).Distinct().ToList()
                    })
                    .OrderBy(c => c.Name)
                    .ToList();

                return Ok(ApiResponse<List<ApiControllerInfo>>.SuccessResponse(controllers, "Controllers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API controllers");
                return BadRequest(ApiResponse<List<ApiControllerInfo>>.ErrorResponse("Failed to retrieve controllers"));
            }
        }

        private bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true; // Reference types are nullable
            return Nullable.GetUnderlyingType(type) != null;
        }

        private string GetActionDescription(ControllerActionDescriptor action)
        {
            // Try to get description from XML comments or attributes
            var displayName = action.DisplayName;

            // Simple heuristic based on action name
            if (action.ActionName.Contains("Get", StringComparison.OrdinalIgnoreCase))
                return $"Get {action.ControllerName} data";
            if (action.ActionName.Contains("Post", StringComparison.OrdinalIgnoreCase) ||
                action.ActionName.Contains("Create", StringComparison.OrdinalIgnoreCase))
                return $"Create new {action.ControllerName}";
            if (action.ActionName.Contains("Put", StringComparison.OrdinalIgnoreCase) ||
                action.ActionName.Contains("Update", StringComparison.OrdinalIgnoreCase))
                return $"Update {action.ControllerName}";
            if (action.ActionName.Contains("Delete", StringComparison.OrdinalIgnoreCase))
                return $"Delete {action.ControllerName}";

            return displayName ?? $"{action.ActionName} on {action.ControllerName}";
        }
    }

    public class ApiEndpointInfo
    {
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public List<ApiParameterInfo> Parameters { get; set; } = new();
        public bool RequiresAuth { get; set; }
        public string Roles { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ApiParameterInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
    }

    public class ApiControllerInfo
    {
        public string Name { get; set; } = string.Empty;
        public int EndpointCount { get; set; }
        public List<string> Actions { get; set; } = new();
    }
}
