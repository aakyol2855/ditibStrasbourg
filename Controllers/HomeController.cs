using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;

namespace DitibStasbourg.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var model = new DashboardViewModel
        {
            TotalGorevli = await _context.Gorevli.CountAsync(),
            ActiveGorevli = await _context.Gorevli.CountAsync(g => g.Gorevlendirmeler.Any(assignment => 
                today >= assignment.Tarih && (!assignment.BitisTarihi.HasValue || today <= assignment.BitisTarihi))),
            TotalKurum = await _context.Kurum.CountAsync(),
            TotalGorevlendirme = await _context.Gorevlendirme.CountAsync(),
            GorevlendirmeThisMonth = await _context.Gorevlendirme.CountAsync(g => g.Tarih.Month == DateTime.Now.Month && g.Tarih.Year == DateTime.Now.Year),
            GorevlendirmeThisYear = await _context.Gorevlendirme.CountAsync(g => g.Tarih.Year == DateTime.Now.Year),
            UpcomingAssignments = await _context.Gorevlendirme.CountAsync(g => g.Tarih >= DateTime.Now)
        };

        return View(model);
    }

    public IActionResult Privacy()
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