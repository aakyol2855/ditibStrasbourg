using System.Security.Claims;
using System.Text.Json;
using DitibStasbourg.Models.Navigation;
using Microsoft.Extensions.Caching.Memory;

namespace DitibStasbourg.Services
{
    public class MenuService : IMenuService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "SystemMenuConfig";

        public MenuService(IWebHostEnvironment env, IMemoryCache cache)
        {
            _env = env;
            _cache = cache;
        }

        private async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out List<MenuItem>? menuItems))
            {
                var filePath = Path.Combine(_env.ContentRootPath, "navigation.json");
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    menuItems = JsonSerializer.Deserialize<List<MenuItem>>(json) ?? new List<MenuItem>();
                }
                else
                {
                    menuItems = new List<MenuItem>();
                }

                _cache.Set(CacheKey, menuItems, TimeSpan.FromMinutes(30));
            }

            return menuItems ?? new List<MenuItem>();
        }

        public async Task<List<MenuItem>> GetUserMenuAsync(ClaimsPrincipal user)
        {
            var allItems = await GetAllMenuItemsAsync();
            return FilterMenuForUser(allItems, user);
        }

        private List<MenuItem> FilterMenuForUser(List<MenuItem> items, ClaimsPrincipal user)
        {
            var result = new List<MenuItem>();

            foreach (var item in items)
            {
                // If it requires a claim and user doesn't have it, skip entirely
                // Special case: If RequiredClaim is "SuperAdmin", check role too
                bool hasAccess = false;
                if (string.IsNullOrEmpty(item.RequiredClaim))
                {
                    hasAccess = true;
                }
                else if (user.HasClaim("Permission", item.RequiredClaim) || user.IsInRole("SuperAdmin"))
                {
                    hasAccess = true;
                }

                if (!hasAccess)
                {
                    continue;
                }

                var clone = new MenuItem
                {
                    Title = item.Title,
                    Icon = item.Icon,
                    Controller = item.Controller,
                    Action = item.Action,
                    RequiredClaim = item.RequiredClaim
                };

                // Process children
                if (item.Children != null && item.Children.Any())
                {
                    clone.Children = FilterMenuForUser(item.Children, user);
                    
                    // If it's a parent menu (no direct link) and all children were filtered out, hide the parent too
                    if (string.IsNullOrEmpty(clone.Controller) && !clone.Children.Any())
                    {
                        continue;
                    }
                }

                result.Add(clone);
            }

            return result;
        }

        public async Task<List<MenuItem>> GetBreadcrumbsAsync(string controller, string action)
        {
            var allItems = await GetAllMenuItemsAsync();
            var trail = new List<MenuItem>();
            FindTrail(allItems, controller, action, trail);
            return trail;
        }

        private bool FindTrail(List<MenuItem> items, string controller, string action, List<MenuItem> currentTrail)
        {
            foreach (var item in items)
            {
                currentTrail.Add(item);

                // Exact match
                if (string.Equals(item.Controller, controller, StringComparison.OrdinalIgnoreCase) && 
                    (string.Equals(item.Action, action, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(action)))
                {
                    return true;
                }

                // Check children
                if (item.Children != null && item.Children.Any())
                {
                    if (FindTrail(item.Children, controller, action, currentTrail))
                    {
                        return true;
                    }
                }

                // Not found in this branch, backtrack
                currentTrail.RemoveAt(currentTrail.Count - 1);
            }

            return false;
        }
    }
}
