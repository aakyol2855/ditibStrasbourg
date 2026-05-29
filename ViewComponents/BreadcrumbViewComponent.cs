using DitibStasbourg.Services;
using Microsoft.AspNetCore.Mvc;

namespace DitibStasbourg.ViewComponents
{
    public class BreadcrumbViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;

        public BreadcrumbViewComponent(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var controller = RouteData.Values["controller"]?.ToString();
            var action = RouteData.Values["action"]?.ToString();

            if (string.IsNullOrEmpty(controller)) return Content("");

            var breadcrumbs = await _menuService.GetBreadcrumbsAsync(controller, action!);
            
            return View(breadcrumbs);
        }
    }
}
