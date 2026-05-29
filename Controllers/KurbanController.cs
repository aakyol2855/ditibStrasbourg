using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DitibStasbourg.Controllers
{
    public class KurbanController : Controller
    {
        private readonly IKurbanService _kurbanService;

        public KurbanController(IKurbanService kurbanService)
        {
            _kurbanService = kurbanService;
        }

        public async Task<IActionResult> Index()
        {
            var kurbanlar = await _kurbanService.GetActiveKurbanlarAsync();
            return View(kurbanlar);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Kurbanlik kurbanlik)
        {
            if (ModelState.IsValid)
            {
                kurbanlik.RemainingShares = kurbanlik.TotalShares;
                await _kurbanService.AddAsync(kurbanlik);
                return RedirectToAction(nameof(Index));
            }
            return View(kurbanlik);
        }

        // Additional actions for Hissedar management could go here
    }
}
