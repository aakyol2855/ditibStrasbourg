using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Models.ViewModels;

namespace DitibStasbourg.Controllers
{
    [Authorize(Roles = "SuperAdmin")] // Only SuperAdmins can manage users
    public class UserManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsSuperAdmin = roles.Contains("SuperAdmin"),
                IsAdmin = roles.Contains("Admin")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Update basic info
            user.UserName = model.UserName;
            user.Email = model.Email;
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Manage SuperAdmin Role
            if (model.IsSuperAdmin)
            {
                if (!await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                    await _userManager.AddToRoleAsync(user, "SuperAdmin");
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                    await _userManager.RemoveFromRoleAsync(user, "SuperAdmin");
            }

            // Manage Admin Role (optional, if you want distinct Admin vs User)
             if (model.IsAdmin)
            {
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                    await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            
            if (string.IsNullOrEmpty(newPassword))
            {
                 TempData["Error"] = "Şifre boş olamaz.";
                 return RedirectToAction(nameof(Edit), new { id = userId });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

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
    }
}
