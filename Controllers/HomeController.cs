using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Models.Dashboard;
using DitibStasbourg.Services.Interfaces;
using System.Security.Claims;

namespace DitibStasbourg.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDashboardPreferenceService _prefService;

    public HomeController(ApplicationDbContext context, IDashboardPreferenceService prefService)
    {
        _context = context;
        _prefService = prefService;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        // Core counters
        var totalGorevli = await _context.Gorevli.CountAsync();
        var activeGorevli = await _context.Gorevli.CountAsync(g => g.Gorevlendirmeler.Any(assignment => 
            today >= assignment.Tarih && (!assignment.BitisTarihi.HasValue || today <= assignment.BitisTarihi)));
        var totalKurum = await _context.Kurum.CountAsync();
        var totalGorevlendirme = await _context.Gorevlendirme.CountAsync();
        var gorevlendirmeThisMonth = await _context.Gorevlendirme.CountAsync(g => g.Tarih.Month == DateTime.Now.Month && g.Tarih.Year == DateTime.Now.Year);
        var gorevlendirmeThisYear = await _context.Gorevlendirme.CountAsync(g => g.Tarih.Year == DateTime.Now.Year);
        var upcomingAssignments = await _context.Gorevlendirme.CountAsync(g => g.Tarih >= DateTime.Now);

        // Financial Campaign Insights
        var campaignSummaries = await _context.KurumFinansalDonemler
            .GroupBy(c => new { c.Year, c.CampaignType })
            .Select(g => new FinancialCampaignSummaryDto
            {
                Year = g.Key.Year,
                CampaignType = g.Key.CampaignType.ToString(),
                TotalAmount = g.Sum(c => c.CollectedAmount)
            })
            .OrderByDescending(c => c.Year)
            .ThenBy(c => c.CampaignType)
            .ToListAsync();

        // Regional Staff Density
        var activeAssignments = await _context.Gorevlendirme
            .Include(g => g.Kurum)
            .Where(g => today >= g.Tarih && (!g.BitisTarihi.HasValue || today <= g.BitisTarihi))
            .ToListAsync();

        var allStaff = await _context.Gorevli.ToListAsync();
        var activeStaffIds = activeAssignments.Select(a => a.GorevliId).ToHashSet();
        var totalUnassigned = allStaff.Count(s => !activeStaffIds.Contains(s.Id));

        var regionalDensities = activeAssignments
            .Where(a => a.Kurum != null && !string.IsNullOrEmpty(a.Kurum.Bolge))
            .GroupBy(a => a.Kurum.Bolge)
            .Select(g => new RegionalStaffDensityDto
            {
                Region = g.Key ?? string.Empty,
                ActiveCount = g.Select(a => a.GorevliId).Distinct().Count(),
                UnassignedCount = 0
            })
            .ToList();

        // Ensure base regions (57, 67, 68) exist
        var baseRegions = new[] { "57", "67", "68" };
        foreach (var r in baseRegions)
        {
            if (!regionalDensities.Any(rd => rd.Region == r))
            {
                regionalDensities.Add(new RegionalStaffDensityDto { Region = r, ActiveCount = 0, UnassignedCount = 0 });
            }
        }

        foreach (var rd in regionalDensities)
        {
            rd.UnassignedCount = totalUnassigned;
        }

        regionalDensities = regionalDensities.OrderBy(rd => rd.Region).ToList();

        // Kurban metrics
        var kurbanlar = await _context.Kurbanliklar.ToListAsync();
        var totalShares = kurbanlar.Sum(k => k.TotalShares);
        var remainingShares = kurbanlar.Sum(k => k.RemainingShares);
        var soldShares = totalShares - remainingShares;

        var hissedarlar = await _context.Hissedarlar.ToListAsync();
        var totalKurbanCollected = hissedarlar.Sum(h => h.TotalPaid);
        var totalKurbanOverdue = hissedarlar.Sum(h => h.RemainingBalance);

        var model = new DashboardViewModel
        {
            TotalGorevli = totalGorevli,
            ActiveGorevli = activeGorevli,
            TotalKurum = totalKurum,
            TotalGorevlendirme = totalGorevlendirme,
            GorevlendirmeThisMonth = gorevlendirmeThisMonth,
            GorevlendirmeThisYear = gorevlendirmeThisYear,
            UpcomingAssignments = upcomingAssignments,
            CampaignSummaries = campaignSummaries,
            RegionalDensities = regionalDensities,
            TotalKurbanShares = totalShares,
            SoldKurbanShares = soldShares,
            RemainingKurbanShares = remainingShares,
            TotalKurbanCollected = totalKurbanCollected,
            TotalKurbanOverdue = totalKurbanOverdue
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SaveDashboardPreferences([FromBody] DashboardPreference preferences)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            await _prefService.SavePreferencesAsync(userId, preferences);
            return Ok();
        }
        return BadRequest();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Rehber()
    {
        return View();
    }

    public async Task<IActionResult> SeedTestData()
    {
        try
        {
            await TestDataSeeder.SeedTestDataAsync(_context);
            return Content("✅ Test verileri başarıyla eklendi!\n\n" +
                "5 Cami, 8 Görevli, 8 Görevlendirme, 3 Dernek + Üyeler\n\n" +
                "Sistemi test edebilirsiniz!");
        }
        catch (Exception ex)
        {
            return Content($"❌ Hata: {ex.Message}");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}