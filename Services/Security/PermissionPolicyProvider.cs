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

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // If the policy name matches our convention, create it dynamically
            // (You can also add a prefix like "Permission_" if you want to be explicit,
            // but simply treating all unknown policies as permission checks works too)
            
            var policy = new AuthorizationPolicyBuilder();
            policy.RequireClaim("Permission", policyName);
            
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }
    }
}
