using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DitibStasbourg.Controllers
{
    public class KurbanController : Controller
    {
        private readonly IKurbanService _kurbanService;
        private readonly ISystemAuditLogService _auditLogService;
        private readonly IMemoryCache _cache;

        public KurbanController(
            IKurbanService kurbanService,
            ISystemAuditLogService auditLogService,
            IMemoryCache cache)
        {
            _kurbanService = kurbanService;
            _auditLogService = auditLogService;
            _cache = cache;
        }

        // ── Kurbanlik CRUD ─────────────────────────────────────────────────────

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

                var username = User.Identity?.Name ?? "System_Daemon";
                await _auditLogService.LogAsync(
                    "Information",
                    username,
                    $"Kullanıcı {username} '{kurbanlik.TagNumber}' küpeli yeni bir kurbanlık hayvan ekledi.",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    "KurbanController");

                TempData["Success"] = $"'{kurbanlik.TagNumber}' küpeli hayvan başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(kurbanlik);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var kurbanlik = await _kurbanService.GetKurbanlikByIdAsync(id.Value);
            if (kurbanlik == null) return NotFound();
            return View(kurbanlik);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Kurbanlik kurbanlik)
        {
            if (id != kurbanlik.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _kurbanService.UpdateKurbanlikAsync(kurbanlik);

                await _auditLogService.LogAsync(
                    "Information",
                    User.Identity?.Name ?? "System",
                    $"'{kurbanlik.TagNumber}' küpeli kurbanlık güncellendi.",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    "KurbanController");

                TempData["Success"] = "Kurbanlık başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(kurbanlik);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _kurbanService.SoftDeleteKurbanlikAsync(id);
            TempData["Success"] = "Kurbanlık pasife alındı.";
            return RedirectToAction(nameof(Index));
        }

        // ── Hissedar (Shareholder) CRUD ────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHissedar(Hissedar hissedar)
        {
            ModelState.Remove("Kurbanlik");

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Form verileri hatalı." });

            var (success, errorReason) = await _kurbanService.AddHissedarAsync(hissedar, _cache);

            if (!success)
                return Json(new { success = false, message = errorReason });

            await _auditLogService.LogAsync(
                "Information",
                User.Identity?.Name ?? "System",
                $"Yeni hissedar eklendi: '{hissedar.Name}' → Kurbanlık ID: {hissedar.KurbanlikId}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                "KurbanController");

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetHissedarlar(int kurbanlikId)
        {
            var hissedarlar = await _kurbanService.GetHissedarlarAsync(kurbanlikId);
            return Json(hissedarlar.Select(h => new
            {
                h.Id,
                h.Name,
                h.Phone,
                h.PaymentStatus,
                h.IsVekaletTaken,
                joinedAt = h.JoinedAt.ToString("dd.MM.yyyy HH:mm")
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHissedar(int id, string name, string phone, string paymentStatus, bool isVekaletTaken)
        {
            var success = await _kurbanService.UpdateHissedarAsync(new Hissedar
            {
                Id              = id,
                Name            = name,
                Phone           = phone,
                PaymentStatus   = paymentStatus,
                IsVekaletTaken  = isVekaletTaken
            });
            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHissedar(int id)
        {
            var success = await _kurbanService.DeleteHissedarAsync(id);
            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoAssign(int shareholderId)
        {
            var success = await _kurbanService.AutoAssignShareholderAsync(shareholderId);
            return Json(new { success, message = success ? "Otomatik atama başarılı." : "Atama yapılamadı: uygun kurbanlık bulunamadı." });
        }
    }
}
