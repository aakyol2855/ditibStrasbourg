using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DitibStasbourg.Data;

namespace DitibStasbourg.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PurgeTestData()
        {
            await TestDataInitializer.PurgeMockDataAsync(_context);
            TempData["Success"] = "Mock test verileri başarıyla temizlendi.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> SeedTestData()
        {
            await TestDataInitializer.SeedMockDataAsync(_context);
            TempData["Success"] = "Mock test verileri başarıyla oluşturuldu.";
            return RedirectToAction("Index", "Home");
        }
    }
}
