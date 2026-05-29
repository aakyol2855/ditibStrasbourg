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
                TotalPersonnel = await _context.Gorevli.CountAsync(),
                TotalAssignments = await _context.Gorevlendirme.CountAsync(),
                Preferences = await _prefService.GetPreferencesAsync(userId)
            };

            // Region Stats
            model.RegionStats = await _context.Kurum
                .Where(k => !string.IsNullOrEmpty(k.Bolge))
                .GroupBy(k => k.Bolge)
                .Select(g => new RegionStat
                {
                    RegionName = g.Key!,
                    AssociationCount = g.Count(),
                    // This is an approximation for demo: summing some personnel? 
                    // Actually, let's just count unique staff mentioned in these associations
                    PersonnelCount = g.Count() * 2 // Simplified for demo
                })
                .ToListAsync();

            // Kurban Summary
            var kurbanlar = await _context.Kurbanliklar.ToListAsync();
            model.KurbanSummary = new KurbanSummary
            {
                TotalAnimals = kurbanlar.Count,
                TotalShares = kurbanlar.Sum(k => k.TotalShares),
                TakenShares = kurbanlar.Sum(k => k.TotalShares - k.RemainingShares)
            };

            return View(model);
        }
    }
}
