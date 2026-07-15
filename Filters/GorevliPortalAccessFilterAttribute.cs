using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Microsoft.AspNetCore.Http;

namespace DitibStasbourg.Filters
{
    /// <summary>
    /// Enforces row‑level security for users with the "GorevliUser" role/claim.
    /// Allows access only to their own record (identified by the claim "GorevliId")
    /// and to actions that do not reference other entity identifiers.
    /// </summary>
    public class GorevliPortalAccessFilterAttribute : IAsyncActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GorevliPortalAccessFilterAttribute(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if the user is a GorevliUser (role or claim)
            var isGorevliUser = user.IsInRole("GorevliUser") || user.HasClaim(c => c.Type == "GorevliUser");
            if (!isGorevliUser)
            {
                // Not a restricted user – proceed normally
                await next();
                return;
            }

            // Extract linked Gorevli identifier from claims – expectation: claim type "GorevliId"
            var idClaim = user.Claims.FirstOrDefault(c => c.Type.Equals("GorevliId", StringComparison.OrdinalIgnoreCase));
            if (idClaim == null || !int.TryParse(idClaim.Value, out var linkedGorevliId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Validate any incoming identifier arguments against the linked ID
            foreach (var arg in context.ActionArguments)
            {
                var key = arg.Key?.ToLowerInvariant();
                if (key == null) continue;

                // Keys that represent entity identifiers
                if (key.Contains("id") && int.TryParse(arg.Value?.ToString(), out var incomingId))
                {
                    // Allow if the action accesses the user's own profile or their own institution record only
                    // For simplicity we restrict any mismatched id to forbidden.
                    if (incomingId != linkedGorevliId)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
            }

            // All checks passed – continue to controller action
            await next();
        }
    }
}
