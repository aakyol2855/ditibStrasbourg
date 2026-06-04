using System.Security.Claims;
using System.Text.Json;
using DitibStasbourg.Models.Navigation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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

        private async Task<NavigationConfig> GetNavigationConfigAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out NavigationConfig? config))
            {
                var filePath = Path.Combine(_env.ContentRootPath, "navigation.json");
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    config = JsonSerializer.Deserialize<NavigationConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new NavigationConfig();
                }
                else
                {
                    config = new NavigationConfig();
                }

                _cache.Set(CacheKey, config, TimeSpan.FromMinutes(30));
            }

            return config ?? new NavigationConfig();
        }

        public async Task<SidebarViewModel> GetSidebarMenuAsync(ClaimsPrincipal user)
        {
            var config = await GetNavigationConfigAsync();
            
            return new SidebarViewModel
            {
                MainMenu = FilterMenuForUser(config.MainMenu, user),
                AdminMenu = FilterMenuForUser(config.AdminMenu, user)
            };
        }

        private List<MenuItem> FilterMenuForUser(List<MenuItem> items, ClaimsPrincipal user)
        {
            var result = new List<MenuItem>();

            foreach (var item in items)
            {
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
            var config = await GetNavigationConfigAsync();
            var allItems = new List<MenuItem>();
            allItems.AddRange(config.MainMenu);
            allItems.AddRange(config.AdminMenu);
            
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

        private class NavigationConfig
        {
            public List<MenuItem> MainMenu { get; set; } = new();
            public List<MenuItem> AdminMenu { get; set; } = new();
        }
    }
}
