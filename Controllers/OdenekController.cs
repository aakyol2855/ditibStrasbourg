using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.Enums;
using DitibStasbourg.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Controllers
{
    public class OdenekController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemAuditLogService _auditService;

        public OdenekController(ApplicationDbContext context, ISystemAuditLogService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(int? kurumId, AllocationType? type)
        {
            var query = _context.KurumKasaOdenekler
                .Include(o => o.Kurum)
                .Include(o => o.TargetGorevli)
                .AsQueryable();

            if (kurumId.HasValue)
                query = query.Where(o => o.KurumId == kurumId.Value);

            if (type.HasValue)
                query = query.Where(o => o.AllocationType == type.Value);

            ViewBag.Kurumlar = await _context.Kurum.OrderBy(k => k.Isim).ToListAsync();
            ViewBag.CurrentKurumId = kurumId;
            ViewBag.CurrentType = type;

            var records = await query.OrderByDescending(o => o.TransferDate).ToListAsync();
            return View(records);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Kurumlar = new SelectList(await _context.Kurum.OrderBy(k => k.Isim).ToListAsync(), "Id", "Isim");
            ViewBag.Gorevliler = new SelectList(await _context.Gorevli.OrderBy(g => g.Ad).Select(g => new { g.Id, Isim = g.Ad + " " + g.Soyad }).ToListAsync(), "Id", "Isim");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KurumKasaOdenek model)
        {
            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Tutar sıfırdan büyük olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                model.IslemYapan = User.Identity?.Name ?? "System";
                _context.KurumKasaOdenekler.Add(model);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    "Information",
                    model.IslemYapan,
                    $"Yeni ödenek kaydı: Kurum={model.KurumId}, Tutar={model.Amount:C2}, Tip={model.AllocationType}",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    "OdenekController");

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Kurumlar = new SelectList(await _context.Kurum.OrderBy(k => k.Isim).ToListAsync(), "Id", "Isim");
            ViewBag.Gorevliler = new SelectList(await _context.Gorevli.OrderBy(g => g.Ad).Select(g => new { g.Id, Isim = g.Ad + " " + g.Soyad }).ToListAsync(), "Id", "Isim");
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var record = await _context.KurumKasaOdenekler
                .Include(o => o.Kurum)
                .Include(o => o.TargetGorevli)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (record == null) return NotFound();
            return View(record);
        }
    }
}
