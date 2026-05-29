using DitibStasbourg.Services;
using Microsoft.AspNetCore.Mvc;

namespace DitibStasbourg.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;

        public SidebarViewComponent(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuItems = await _menuService.GetUserMenuAsync(UserClaimsPrincipal);
            
            // Pass current route info for active state determination
            ViewBag.CurrentController = RouteData.Values["controller"]?.ToString();
            ViewBag.CurrentAction = RouteData.Values["action"]?.ToString();
            
            return View(menuItems);
        }
    }
}
