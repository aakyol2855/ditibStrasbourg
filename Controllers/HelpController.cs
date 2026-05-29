using Microsoft.AspNetCore.Mvc;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;
using System.Security.Claims;

namespace DitibStasbourg.Controllers
{
    public class HelpController : Controller
    {
        private readonly IHelpService _helpService;

        public HelpController(IHelpService helpService)
        {
            _helpService = helpService;
        }

        public async Task<IActionResult> Index(string? category)
        {
            var categories = await _helpService.GetCategoriesAsync();
            var selectedCategory = category ?? categories.FirstOrDefault();
            
            IEnumerable<HelpTopic> topics;
            if (string.IsNullOrEmpty(selectedCategory))
            {
                topics = Enumerable.Empty<HelpTopic>();
            }
            else
            {
                topics = await _helpService.GetTopicsByCategoryAsync(selectedCategory, User);
            }

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = selectedCategory;

            return View(topics);
        }

        public async Task<IActionResult> Details(int id)
        {
            var topic = await _helpService.GetByIdAsync(id);
            if (topic == null) return NotFound();

            // Check permission
            var userClaims = User.Claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
            if (!User.IsInRole("SuperAdmin") && 
                !string.IsNullOrEmpty(topic.RequiredClaim) && 
                !userClaims.Contains(topic.RequiredClaim))
            {
                return Forbid();
            }
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_HelpContentPartial", topic);
            }

            return View(topic);
        }

        public async Task<IActionResult> Admin()
        {
            var topics = await _helpService.GetAllAsync(orderBy: q => q.OrderBy(t => t.Category).ThenBy(t => t.DisplayOrder));
            return View(topics);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HelpTopic topic)
        {
            if (ModelState.IsValid)
            {
                await _helpService.AddAsync(topic);
                return RedirectToAction(nameof(Admin));
            }
            return View(topic);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var topic = await _helpService.GetByIdAsync(id);
            if (topic == null) return NotFound();
            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HelpTopic topic)
        {
            if (ModelState.IsValid)
            {
                topic.UpdatedAt = DateTime.Now;
                await _helpService.UpdateAsync(topic);
                return RedirectToAction(nameof(Admin));
            }
            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _helpService.DeleteAsync(id);
            return RedirectToAction(nameof(Admin));
        }
    }
}
