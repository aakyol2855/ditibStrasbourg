using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class LookupAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILookupService _lookupService;

        public LookupAdminController(ApplicationDbContext context, ILookupService lookupService)
        {
            _context = context;
            _lookupService = lookupService;
        }

        // --- LOOKUP TYPES ---
        public async Task<IActionResult> Index()
        {
            var types = await _context.LookupTypes.ToListAsync();
            return View(types);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateType(LookupType type)
        {
            if (ModelState.IsValid)
            {
                type.Code = type.Code.ToUpperInvariant().Replace(" ", "_");
                _context.LookupTypes.Add(type);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditType(LookupType type)
        {
            var existing = await _context.LookupTypes.FindAsync(type.Id);
            if (existing != null)
            {
                existing.Name = type.Name;
                existing.Code = type.Code.ToUpperInvariant().Replace(" ", "_");
                existing.IsActive = type.IsActive;
                await _context.SaveChangesAsync();
                _lookupService.ClearDynamicCache(existing.Code);
            }
            return RedirectToAction(nameof(Index));
        }

        // --- LOOKUP VALUES ---
        public async Task<IActionResult> Values(int typeId)
        {
            var type = await _context.LookupTypes
                .Include(t => t.Values)
                .FirstOrDefaultAsync(t => t.Id == typeId);
            
            if (type == null) return NotFound();

            return View(type);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateValue(LookupValue model)
        {
            if (ModelState.IsValid)
            {
                _context.LookupValues.Add(model);
                await _context.SaveChangesAsync();

                var type = await _context.LookupTypes.FindAsync(model.LookupTypeId);
                if (type != null) _lookupService.ClearDynamicCache(type.Code);
            }
            return RedirectToAction(nameof(Values), new { typeId = model.LookupTypeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditValue(LookupValue model)
        {
            var existing = await _context.LookupValues.Include(v => v.LookupType).FirstOrDefaultAsync(v => v.Id == model.Id);
            if (existing != null)
            {
                existing.Name = model.Name;
                existing.Value = model.Value;
                existing.SortOrder = model.SortOrder;
                existing.IsActive = model.IsActive;
                await _context.SaveChangesAsync();

                if (existing.LookupType != null)
                    _lookupService.ClearDynamicCache(existing.LookupType.Code);
            }
            return RedirectToAction(nameof(Values), new { typeId = existing?.LookupTypeId ?? model.LookupTypeId });
        }
    }
}
