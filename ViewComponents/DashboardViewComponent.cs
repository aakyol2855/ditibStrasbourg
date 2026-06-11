using DitibStasbourg.Data;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DitibStasbourg.ViewComponents
{
    public class DashboardViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IDashboardPreferenceService _prefService;

        public DashboardViewComponent(ApplicationDbContext context, IDashboardPreferenceService prefService)
        {
            _context = context;
            _prefService = prefService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var model = new DashboardStatsViewModel
            {
                TotalAssociations = await _context.Kurum.CountAsync(k => k.Tip == Models.KurumTip.Dernek),
                TotalCami         = await _context.Kurum.CountAsync(k => k.Tip == Models.KurumTip.Cami),
                TotalPersonnel    = await _context.Gorevli.CountAsync(),
                TotalAssignments  = await _context.Gorevlendirme.CountAsync(),
                Preferences       = await _prefService.GetPreferencesAsync(userId)
            };

            // ── Region Stats: real gorevlendirme count per region (no approximation) ──
            var regionRaw = await _context.Kurum
                .Where(k => !string.IsNullOrEmpty(k.Bolge))
                .Select(k => new { k.Bolge, AssignmentCount = k.Gorevlendirmeler.Count() })
                .ToListAsync();

            model.RegionStats = regionRaw
                .GroupBy(r => r.Bolge!)
                .Select(g => new RegionStat
                {
                    RegionName       = g.Key,
                    AssociationCount = g.Count(),
                    PersonnelCount   = g.Sum(r => r.AssignmentCount) // live active assignment count
                })
                .OrderByDescending(r => r.PersonnelCount)
                .ToList();

            // ── Kurban Summary ──
            var kurbanlar = await _context.Kurbanliklar.ToListAsync();
            model.KurbanSummary = new KurbanSummary
            {
                TotalAnimals = kurbanlar.Count,
                TotalShares  = kurbanlar.Sum(k => k.TotalShares),
                TakenShares  = kurbanlar.Sum(k => k.TotalShares - k.RemainingShares)
            };

            return View(model);
        }
    }
}
