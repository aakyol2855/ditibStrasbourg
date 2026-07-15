using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models.Security;
using DitibStasbourg.Models.ViewModels;

using Microsoft.AspNetCore.Authorization;

namespace DitibStasbourg.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class RoleTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoleTemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _context.RoleTemplates.Include(t => t.Claims).ToListAsync();
            return View(templates);
        }

        public IActionResult Create()
        {
            ViewBag.AvailableClaims = GetAllSystemClaims();
            return View(new RoleTemplate());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleTemplate model, string[] selectedClaims)
        {
            if (ModelState.IsValid)
            {
                var template = new RoleTemplate { Name = model.Name };
                foreach (var claim in selectedClaims)
                {
                    template.Claims.Add(new RoleTemplateClaim { ClaimValue = claim });
                }
                _context.RoleTemplates.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.AvailableClaims = GetAllSystemClaims();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var template = await _context.RoleTemplates.Include(t => t.Claims).FirstOrDefaultAsync(t => t.Id == id);
            if (template == null) return NotFound();

            ViewBag.AvailableClaims = GetAllSystemClaims();
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleTemplate model, string[] selectedClaims)
        {
            var template = await _context.RoleTemplates.Include(t => t.Claims).FirstOrDefaultAsync(t => t.Id == id);
            if (template == null) return NotFound();

            template.Name = model.Name;

            // Remove unselected
            var toRemove = template.Claims.Where(c => !selectedClaims.Contains(c.ClaimValue)).ToList();
            foreach (var rm in toRemove) template.Claims.Remove(rm);

            // Add new
            foreach (var claim in selectedClaims)
            {
                if (!template.Claims.Any(c => c.ClaimValue == claim))
                {
                    template.Claims.Add(new RoleTemplateClaim { ClaimValue = claim });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper: Reflect all controllers/actions
        private Dictionary<string, List<string>> GetAllSystemClaims()
        {
            var claims = new Dictionary<string, List<string>>();
            var controllers = typeof(Program).Assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var ctrl in controllers)
            {
                var actions = ctrl.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName && !m.GetCustomAttributes(typeof(NonActionAttribute), true).Any())
                    .Select(m => m.Name)
                    .Distinct()
                    .ToList();

                var ctrlName = ctrl.Name.Replace("Controller", "");
                if (actions.Any())
                {
                    claims[ctrlName] = actions.Select(a => $"{ctrlName}-{a}").ToList();
                }
            }

            // Explicitly inject custom security tokens into the template configuration matrix
            if (!claims.ContainsKey("Maliye"))
            {
                claims["Maliye"] = new List<string>();
            }
            if (!claims["Maliye"].Contains("maliyeRead")) claims["Maliye"].Add("maliyeRead");
            if (!claims["Maliye"].Contains("maliyeWrite")) claims["Maliye"].Add("maliyeWrite");

            return claims;
        }
    }
}
