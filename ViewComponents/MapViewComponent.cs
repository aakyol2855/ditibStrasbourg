using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DitibStasbourg.Models.ViewModels;

namespace DitibStasbourg.ViewComponents
{
    public class MapViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new MapViewModel
            {
                RegionName = "DITIB Doğu Fransa Bölgesi",
                ViewBox = "0 0 800 600"
            };

            return await Task.FromResult<IViewComponentResult>(View(model));
        }
    }
}
