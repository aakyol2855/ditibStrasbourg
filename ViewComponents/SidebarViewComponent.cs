using DitibStasbourg.Services;
using DitibStasbourg.Models.Navigation;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            var sidebarMenu = await _menuService.GetSidebarMenuAsync(UserClaimsPrincipal);
            
            // Pass current route info for active state determination
            ViewBag.CurrentController = RouteData.Values["controller"]?.ToString();
            ViewBag.CurrentAction = RouteData.Values["action"]?.ToString();
            
            return View(sidebarMenu);
        }
    }
}
