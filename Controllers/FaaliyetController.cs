using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.Enums;
using DitibStasbourg.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Controllers
{
    public class FaaliyetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemAuditLogService _auditService;

        public FaaliyetController(ApplicationDbContext context, ISystemAuditLogService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(int? gorevliId, int? kurumId)
        {
            var query = _context.GorevliFaaliyetRaporlari
                .Include(f => f.Gorevli)
                .Include(f => f.Kurum)
                .AsQueryable();

            if (gorevliId.HasValue)
                query = query.Where(f => f.GorevliId == gorevliId.Value);

            if (kurumId.HasValue)
                query = query.Where(f => f.KurumId == kurumId.Value);

            ViewBag.CurrentGorevliId = gorevliId;
            ViewBag.CurrentKurumId = kurumId;

            var records = await query.OrderByDescending(f => f.RaporTarihi).ToListAsync();
            return View(records);
        }

        public async Task<IActionResult> Create(int? gorevliId)
        {
            ViewBag.Gorevliler = new SelectList(
                await _context.Gorevli.OrderBy(g => g.Ad).Select(g => new { g.Id, Isim = g.Ad + " " + g.Soyad }).ToListAsync(),
                "Id", "Isim", gorevliId);
            ViewBag.Kurumlar = new SelectList(
                await _context.Kurum.OrderBy(k => k.Isim).ToListAsync(),
                "Id", "Isim");

            var model = new GorevliFaaliyetRaporu { RaporTarihi = DateTime.Today };
            if (gorevliId.HasValue)
                model.GorevliId = gorevliId.Value;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GorevliFaaliyetRaporu model)
        {
            if (ModelState.IsValid)
            {
                _context.GorevliFaaliyetRaporlari.Add(model);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    "Information",
                    User.Identity?.Name ?? "System",
                    $"Yeni faaliyet raporu: Görevli={model.GorevliId}, Kurum={model.KurumId}, Kurs={model.KursTuru}, Katılımcı={model.KatilimciSayisi}",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    "FaaliyetController");

                return RedirectToAction(nameof(Index), new { gorevliId = model.GorevliId });
            }

            ViewBag.Gorevliler = new SelectList(
                await _context.Gorevli.OrderBy(g => g.Ad).Select(g => new { g.Id, Isim = g.Ad + " " + g.Soyad }).ToListAsync(),
                "Id", "Isim", model.GorevliId);
            ViewBag.Kurumlar = new SelectList(
                await _context.Kurum.OrderBy(k => k.Isim).ToListAsync(),
                "Id", "Isim", model.KurumId);
            return View(model);
        }
    }
}
