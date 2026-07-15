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

        // Kurban metrics (Aggregated exclusively from verified 2026 campaign records)
        var approvedCampaigns = await _context.KurbanCampaignRecords
            .Where(r => r.IsApproved && r.Yil == 2026)
            .ToListAsync();

        int totalShares = 500;
        var quotaSetting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == "KurbanHisseLimit");
        if (quotaSetting != null && int.TryParse(quotaSetting.Value, out var parsedQuota))
            totalShares = parsedQuota;

        int soldShares = approvedCampaigns.Sum(r => r.DigerAdet + r.TrAdet);
        int remainingShares = Math.Max(0, totalShares - soldShares);
        decimal totalKurbanCollected = approvedCampaigns.Sum(r => r.ToplamOdenen);
        decimal totalKurbanOverdue = approvedCampaigns.Sum(r => r.KalanBakiye);

        // Immigration alerts (documents expiring within 3 months)
        var threeMonthsLater = today.AddMonths(3);
        var expiringGorevliler = await _context.Gorevli
            .Where(g => g.IsDeleted == false && 
                       ((g.VisaExpirationDate.HasValue && g.VisaExpirationDate.Value <= threeMonthsLater) ||
                        (g.PassportExpirationDate.HasValue && g.PassportExpirationDate.Value <= threeMonthsLater) ||
                        (g.ResidencePermitExpirationDate.HasValue && g.ResidencePermitExpirationDate.Value <= threeMonthsLater)))
            .ToListAsync();

        var immigrationWarnings = new List<GorevliImmigrationWarningDto>();
        foreach (var g in expiringGorevliler)
        {
            if (g.VisaExpirationDate.HasValue && g.VisaExpirationDate.Value <= threeMonthsLater)
            {
                immigrationWarnings.Add(new GorevliImmigrationWarningDto
                {
                    GorevliId = g.Id,
                    AdSoyad = g.AdSoyad,
                    WarningType = "Vize",
                    ExpirationDate = g.VisaExpirationDate.Value,
                    RemainingDays = (g.VisaExpirationDate.Value - today).Days
                });
            }
            if (g.PassportExpirationDate.HasValue && g.PassportExpirationDate.Value <= threeMonthsLater)
            {
                immigrationWarnings.Add(new GorevliImmigrationWarningDto
                {
                    GorevliId = g.Id,
                    AdSoyad = g.AdSoyad,
                    WarningType = "Pasaport",
                    ExpirationDate = g.PassportExpirationDate.Value,
                    RemainingDays = (g.PassportExpirationDate.Value - today).Days
                });
            }
            if (g.ResidencePermitExpirationDate.HasValue && g.ResidencePermitExpirationDate.Value <= threeMonthsLater)
            {
                immigrationWarnings.Add(new GorevliImmigrationWarningDto
                {
                    GorevliId = g.Id,
                    AdSoyad = g.AdSoyad,
                    WarningType = "Oturum Kartı (Titre de Séjour)",
                    ExpirationDate = g.ResidencePermitExpirationDate.Value,
                    RemainingDays = (g.ResidencePermitExpirationDate.Value - today).Days
                });
            }
        }

        // Vacancies: active institutions with no active assignments
        var gorevlisiOlmayanKurumlar = await _context.Kurum
            .Where(k => k.AktifMi && !k.Gorevlendirmeler.Any(a => today >= a.Tarih && (!a.BitisTarihi.HasValue || today <= a.BitisTarihi)))
            .ToListAsync();

        // Expiring: active assignments ending within 3 months
        var suresiBitenGorevlendirmeler = await _context.Gorevlendirme
            .Include(g => g.Kurum)
            .Include(g => g.Gorevli)
            .Where(g => today >= g.Tarih && g.BitisTarihi.HasValue && today <= g.BitisTarihi && g.BitisTarihi.Value <= threeMonthsLater)
            .ToListAsync();

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
            RegionalDensities = new List<RegionalStaffDensityDto>(),
            TotalKurbanShares = totalShares,
            SoldKurbanShares = soldShares,
            RemainingKurbanShares = remainingShares,
            TotalKurbanCollected = totalKurbanCollected,
            TotalKurbanOverdue = totalKurbanOverdue,
            ImmigrationWarnings = immigrationWarnings.OrderBy(w => w.RemainingDays).ToList(),
            GorevlisiOlmayanKurumlar = gorevlisiOlmayanKurumlar,
            SuresiBitenGorevlendirmeler = suresiBitenGorevlendirmeler
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



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}