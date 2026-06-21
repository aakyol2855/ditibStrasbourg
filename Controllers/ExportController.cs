using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DitibStasbourg.Controllers
{
    /// <summary>
    /// Generic, entity-agnostic Excel export controller.
    ///
    /// Architecture:
    ///   - Module registry maps string keys → (EntityType, typed IQueryable factory)
    ///   - All dispatch is via the typed factory delegates; zero switch-case, zero entity coupling
    ///   - Reflection is used only for the service call dispatch, not for EF query building
    /// </summary>
    [Authorize]
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDynamicExportService _exportService;
        private readonly ISystemAuditLogService _auditService;
        private readonly ILogger<ExportController> _logger;

        // ─── Module Registry ────────────────────────────────────────────────────
        // Each entry is a self-contained, typed export function.
        // Adding a new module = ONE new entry here, zero other changes.
        // The Func<> returns Task<byte[]> so it's fully async, typed, and EF-compatible.
        private delegate Task<byte[]> ExportDelegate(
            IDynamicExportService svc,
            ApplicationDbContext ctx,
            List<int>? ids,
            List<string>? columns,
            bool isQuick,
            bool maskSensitiveData);

        private static readonly Dictionary<string, (Type EntityType, ExportDelegate Execute)>
            _moduleRegistry = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Gorevli"] = (typeof(Gorevli), async (svc, ctx, ids, cols, quick, mask) =>
                {
                    var q = ctx.Gorevli.AsNoTracking();
                    if (ids?.Count > 0) q = q.Where(e => ids.Contains(e.Id));
                    return quick
                        ? await svc.QuickExportAllAsync(q, "Görevliler", mask)
                        : await svc.ExportFilteredAsync(q, cols ?? [], "Görevliler", mask);
                }),

                ["KurumFinansalDonem"] = (typeof(KurumFinansalDonem), async (svc, ctx, ids, cols, quick, mask) =>
                {
                    var q = ctx.KurumFinansalDonemler.AsNoTracking();
                    if (ids?.Count > 0) q = q.Where(e => ids.Contains(e.Id));
                    return quick
                        ? await svc.QuickExportAllAsync(q, "FinansalDonemler", mask)
                        : await svc.ExportFilteredAsync(q, cols ?? [], "FinansalDonemler", mask);
                }),

                ["Dernek"] = (typeof(Kurum), async (svc, ctx, ids, cols, quick, mask) =>
                {
                    var q = ctx.Kurum.Where(k => k.Tip == KurumTip.Dernek).AsNoTracking();
                    if (ids?.Count > 0) q = q.Where(e => ids.Contains(e.Id));
                    return quick
                        ? await svc.QuickExportAllAsync(q, "Dernekler", mask)
                        : await svc.ExportFilteredAsync(q, cols ?? [], "Dernekler", mask);
                }),

                ["Hissedar"] = (typeof(Hissedar), async (svc, ctx, ids, cols, quick, mask) =>
                {
                    var q = ctx.Hissedarlar.AsNoTracking();
                    if (ids?.Count > 0) q = q.Where(e => ids.Contains(e.Id));
                    return quick
                        ? await svc.QuickExportAllAsync(q, "Hissedarlar", mask)
                        : await svc.ExportFilteredAsync(q, cols ?? [], "Hissedarlar", mask);
                }),

                ["Kurbanlik"] = (typeof(Kurbanlik), async (svc, ctx, ids, cols, quick, mask) =>
                {
                    var q = ctx.Kurbanliklar.AsNoTracking();
                    if (ids?.Count > 0) q = q.Where(e => ids.Contains(e.Id));
                    return quick
                        ? await svc.QuickExportAllAsync(q, "Kurbanlıklar", mask)
                        : await svc.ExportFilteredAsync(q, cols ?? [], "Kurbanlıklar", mask);
                }),
            };

        public ExportController(
            ApplicationDbContext context,
            IDynamicExportService exportService,
            ISystemAuditLogService auditService,
            ILogger<ExportController> logger)
        {
            _context       = context;
            _exportService = exportService;
            _auditService  = auditService;
            _logger        = logger;
        }

        // ─── GET /Export/Columns?module=Gorevli ─────────────────────────────────
        /// <summary>
        /// Returns column metadata for the given module as JSON.
        /// Called lazily by the export panel via fetch() when first expanded.
        /// </summary>
        [HttpGet]
        public IActionResult Columns(string module)
        {
            if (!_moduleRegistry.TryGetValue(module, out var reg))
                return BadRequest(new { error = $"Bilinmeyen modül: {module}" });

            var descriptors = _exportService.GetColumnDescriptors(reg.EntityType);

            return Json(descriptors.Select(d => new
            {
                d.PropertyName,
                d.DisplayName,
                d.Order,
                d.IncludeInQuickExport,
            }));
        }

        // ─── POST /Export/Quick ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Quick(
            [FromForm] string module,
            [FromForm] string? selectedIds)
        {
            if (!_moduleRegistry.TryGetValue(module, out var reg))
                return BadRequest();

            return await DispatchExport(module, reg, null, selectedIds, isQuick: true);
        }

        // ─── POST /Export/Filtered ───────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Filtered(
            [FromForm] string module,
            [FromForm] List<string> columns,
            [FromForm] string? selectedIds)
        {
            if (!_moduleRegistry.TryGetValue(module, out var reg))
                return BadRequest();

            return await DispatchExport(module, reg, columns, selectedIds, isQuick: false);
        }

        // ─── Shared dispatch ─────────────────────────────────────────────────────
        private async Task<IActionResult> DispatchExport(
            string module,
            (Type EntityType, ExportDelegate Execute) reg,
            List<string>? columns,
            string? selectedIds,
            bool isQuick)
        {
            try
            {
                var ids = !string.IsNullOrEmpty(selectedIds)
                    ? JsonSerializer.Deserialize<List<int>>(selectedIds)
                    : null;

                bool hasAccess = User.HasClaim(c => c.Type == "privateInfoRead" || c.Value == "privateInfoRead" || (c.Type == "Permission" && c.Value == "privateInfoRead"));
                if (!hasAccess)
                {
                    return Forbid();
                }
                bool maskSensitive = false;
                var fileBytes = await reg.Execute(_exportService, _context, ids, columns, isQuick, maskSensitive);

                var username = User.Identity?.Name ?? "anonymous";
                string modeLabel = isQuick ? "Hızlı" : "Filtreli";
                await _auditService.LogAsync(
                    "Information",
                    username,
                    $"{modeLabel} Excel dışa aktarımı: Modül={module}, Sütun sayısı={columns?.Count ?? 0}, ID filtresi={(ids?.Count > 0 ? ids.Count.ToString() + " satır" : "tümü")}",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    "ExportController");

                var fileName = $"DITIB_{module}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel export failed for module {Module}", module);
                return StatusCode(500, new { error = "Dışa aktarım sırasında sunucu hatası oluştu." });
            }
        }
    }
}
