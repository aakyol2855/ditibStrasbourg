using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DitibStasbourg.Services.Security
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            // Use the fallback provider for default policies like "RequireAuthenticatedUser"
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => 
            FallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => 
            FallbackPolicyProvider.GetFallbackPolicyAsync();

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var fallbackPolicy = await FallbackPolicyProvider.GetPolicyAsync(policyName);
            if (fallbackPolicy != null)
            {
                return fallbackPolicy;
            }

            var policy = new AuthorizationPolicyBuilder();
            policy.RequireAssertion(context =>
                context.User.IsInRole("SuperAdmin") || 
                context.User.IsInRole("Admin") || 
                context.User.HasClaim("Permission", policyName) ||
                context.User.HasClaim(policyName, "true")
            );
            
            return policy.Build();
        }
    }
}
