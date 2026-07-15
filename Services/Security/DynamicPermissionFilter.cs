using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DitibStasbourg.Services.Security
{
    public class DynamicPermissionFilter : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Allow if [AllowAnonymous] is present
            if (context.ActionDescriptor.EndpointMetadata.Any(em => em.GetType() == typeof(AllowAnonymousAttribute)))
                return;

            var routeData = context.RouteData.Values;
            var controller = routeData["controller"]?.ToString();
            var action = routeData["action"]?.ToString();

            // Ignore non-controller routes
            if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(action))
                return;

            var requiredPermission = $"{controller}-{action}";

            var user = context.HttpContext.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // SuperAdmin or Admin bypass to prevent authorization lockout loops
            if (user.IsInRole("SuperAdmin") || user.IsInRole("Admin"))
            {
                await Task.CompletedTask;
                return;
            }

            var hasClaim = user.HasClaim("Permission", requiredPermission);

            if (!hasClaim)
            {
                context.Result = new ForbidResult();
            }

            await Task.CompletedTask;
        }
    }
}
