using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Services.Interfaces;
using DitibStasbourg.Models;

namespace DitibStasbourg.Controllers
{
    [AllowAnonymous]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGeocodingService _geocodingService;

        public DashboardController(ApplicationDbContext context, IGeocodingService geocodingService)
        {
            _context = context;
            _geocodingService = geocodingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMapMarkers()
        {
            var activeKurumlar = await _context.Kurum
                .Where(k => k.Tip == KurumTip.Dernek && k.AktifMi == true)
                .ToListAsync();

            var markers = new List<object>();
            bool updatedAny = false;

            foreach (var k in activeKurumlar)
            {
                if (k.Latitude == null || k.Longitude == null)
                {
                    var coords = await _geocodingService.GeocodeAddressAsync(k.Adres, k.Sehir);
                    if (coords.Latitude.HasValue && coords.Longitude.HasValue)
                    {
                        k.Latitude = coords.Latitude;
                        k.Longitude = coords.Longitude;
                        _context.Update(k);
                        updatedAny = true;
                    }
                }

                markers.Add(new
                {
                    k.Id,
                    k.Isim,
                    k.Sehir,
                    k.Adres,
                    k.Latitude,
                    k.Longitude,
                    k.DernekBaskaniAd
                });
            }

            if (updatedAny)
            {
                await _context.SaveChangesAsync();
            }

            return Json(markers);
        }
    }
}
