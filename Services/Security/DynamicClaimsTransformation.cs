using System.Security.Claims;
using DitibStasbourg.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using DitibStasbourg.Services.Interfaces;

namespace DitibStasbourg.Services.Security
{
    public class DynamicClaimsTransformation : IClaimsTransformation
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;

        public DynamicClaimsTransformation(IServiceProvider serviceProvider, IMemoryCache cache)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // If user is not authenticated or lacks an ID, skip
            if (!principal.Identity?.IsAuthenticated ?? true)
                return principal;

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return principal;

            // Prevent adding claims multiple times if already transformed
            if (principal.HasClaim(c => c.Type == "DynamicClaimsInjected"))
                return principal;

            var cacheKey = $"UserClaims_{userId}";
            if (!_cache.TryGetValue(cacheKey, out List<string>? activePermissions))
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                activePermissions = new List<string>();

                // 1. Get Template Claims
                var userTemplate = await dbContext.UserRoleTemplates
                    .Include(urt => urt.RoleTemplate)
                    .ThenInclude(rt => rt.Claims)
                    .FirstOrDefaultAsync(urt => urt.UserId == userId);

                var rawPermissions = new List<string>();
                if (userTemplate?.RoleTemplate != null)
                {
                    rawPermissions.AddRange(userTemplate.RoleTemplate.Claims.Select(c => c.ClaimValue));
                }

                // 2. Get User Overrides (Add/Remove)
                var overrides = await dbContext.UserClaimOverrides
                    .Where(o => o.UserId == userId)
                    .ToListAsync();

                foreach (var ov in overrides)
                {
                    if (ov.IsDenied)
                    {
                        rawPermissions.RemoveAll(p => p == ov.ClaimValue);
                    }
                    else
                    {
                        if (!rawPermissions.Contains(ov.ClaimValue))
                            rawPermissions.Add(ov.ClaimValue);
                    }
                }

                // 3. WILDCARD EXPANSION (e.g., Account-*)
                var userManagementService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();
                var allAvailableClaims = userManagementService.GetAllSystemClaims().Values.SelectMany(x => x).ToList();

                foreach (var perm in rawPermissions.ToList())
                {
                    if (perm.EndsWith("-*"))
                    {
                        var prefix = perm.Substring(0, perm.Length - 1); // e.g., "Account-"
                        var expanded = allAvailableClaims.Where(c => c.StartsWith(prefix)).ToList();
                        foreach (var exp in expanded)
                        {
                            if (!activePermissions.Contains(exp))
                                activePermissions.Add(exp);
                        }
                    }
                    else
                    {
                        if (!activePermissions.Contains(perm))
                            activePermissions.Add(perm);
                    }
                }

                // Apply Denials again after expansion (to ensure a specific denied claim isn't caught by a wildcard)
                foreach (var ov in overrides.Where(o => o.IsDenied))
                {
                    activePermissions.RemoveAll(p => p == ov.ClaimValue);
                }

                // Cache for 5 minutes
                _cache.Set(cacheKey, activePermissions, TimeSpan.FromMinutes(5));
            }

            // Create a new identity and add the computed permissions
            var identity = new ClaimsIdentity();
            
            if (activePermissions != null)
            {
                foreach (var permission in activePermissions)
                {
                    identity.AddClaim(new Claim("Permission", permission));
                }
            }

            // Flag that we did this
            identity.AddClaim(new Claim("DynamicClaimsInjected", "true"));

            principal.AddIdentity(identity);
            return principal;
        }
    }
}
