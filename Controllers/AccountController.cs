using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DitibStasbourg.Models.ViewModels;

namespace DitibStasbourg.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            Console.WriteLine($"[DEBUG] Login POST received for Email: '{model.Email}'");

            if (!ModelState.IsValid) 
            {
                Console.WriteLine("[DEBUG] ModelState invalid");
                return View(model);
            }

            // Fallback strategy: Try Email first, then UserName (treating input as email/username)
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                Console.WriteLine($"[DEBUG] FindByEmailAsync returned null for '{model.Email}'. Trying FindByNameAsync...");
                user = await _userManager.FindByNameAsync(model.Email);
            }

            if (user == null)
            {
                Console.WriteLine($"[DEBUG] User not found by Email OR Name: '{model.Email}'");
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi. (Kullanıcı bulunamadı)");
                return View(model);
            }

            // Check if user has a password
            if (!await _userManager.HasPasswordAsync(user))
            {
                Console.WriteLine("[DEBUG] User has no password. Redirecting to SetPassword.");
                return RedirectToAction("SetPassword", new { email = model.Email });
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Şifre gereklidir.");
                return View(model);
            }

            // DIAGNOSTIC LOGS
            Console.WriteLine($"[DEBUG] User Found: ID={user.Id}, UserName={user.UserName}, Email={user.Email}, Confirmed={user.EmailConfirmed}");
            Console.WriteLine($"[DEBUG] Password Input: '{model.Password}'");
            
            var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            Console.WriteLine($"[DEBUG] Password Check Result: {checkPassword}");

            if (!checkPassword)
            {
                 Console.WriteLine("[DEBUG] Password mismatch. Hash in DB does not match input.");
                 // Optional: Print hash for debug (be careful in prod)
                 Console.WriteLine($"[DEBUG] Stored Hash: {user.PasswordHash}");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, lockoutOnFailure: false);
            Console.WriteLine($"[DEBUG] SignInManager Result: {result}");

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            if (result.RequiresTwoFactor) ModelState.AddModelError(string.Empty, "2FA Required");
            if (result.IsLockedOut) ModelState.AddModelError(string.Empty, "User Locked Out");
            if (result.IsNotAllowed) ModelState.AddModelError(string.Empty, "User Not Allowed (Email Confirmed?)");
            
            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi. (Check Console Logs)");
            return View(model);
        }

        [HttpGet]
        public IActionResult SetPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");
            return View(new SetPasswordViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("Login");

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (addPasswordResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in addPasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
