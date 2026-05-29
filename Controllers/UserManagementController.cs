using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using DitibStasbourg.Services.Base;
using DitibStasbourg.Models.Security;

namespace DitibStasbourg.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly IUserManagementService _userService;
        private readonly IBaseService<RoleTemplate> _roleTemplateService;

        public UserManagementController(IUserManagementService userService, IBaseService<RoleTemplate> roleTemplateService)
        {
            _userService = userService;
            _roleTemplateService = roleTemplateService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var model = await _userService.GetUserForEditAsync(id);
            if (model == null) return NotFound();

            var templates = await _roleTemplateService.GetAllAsync();
            ViewBag.RoleTemplates = templates.ToList();
            ViewBag.AllClaims = _userService.GetAllSystemClaims();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid) 
            {
                var templates = await _roleTemplateService.GetAllAsync();
                ViewBag.RoleTemplates = templates.ToList();
                ViewBag.AllClaims = _userService.GetAllSystemClaims();
                return View(model);
            }

            var result = await _userService.UpdateUserAsync(model);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                    
                var templates = await _roleTemplateService.GetAllAsync();
                ViewBag.RoleTemplates = templates.ToList();
                ViewBag.AllClaims = _userService.GetAllSystemClaims();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                 TempData["Error"] = "Şifre boş olamaz.";
                 return RedirectToAction(nameof(Edit), new { id = userId });
            }

            var result = await _userService.ResetUserPasswordAsync(userId, newPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Şifre başarıyla sıfırlandı.";
            }
            else
            {
                foreach (var error in result.Errors)
                    TempData["Error"] += error.Description + " ";
            }
            return RedirectToAction(nameof(Edit), new { id = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result.Succeeded)
            {
                TempData["Success"] = "Kullanıcı başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
