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
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
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
            _logger.LogInformation("Kullanıcı giriş denemesi: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login: ModelState geçersiz. Email={Email}", model.Email);
                return View(model);
            }

            // Fallback strategy: Try Email first, then UserName (treating input as email/username)
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogDebug("FindByEmailAsync sonuç döndürmedi: '{Email}'. UserName ile deneniyor...", model.Email);
                user = await _userManager.FindByNameAsync(model.Email);
            }

            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı: '{Email}'", model.Email);
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi. (Kullanıcı bulunamadı)");
                return View(model);
            }

            // Check if user has a password
            if (!await _userManager.HasPasswordAsync(user))
            {
                _logger.LogInformation("Kullanıcının şifresi yok, SetPassword'a yönlendiriliyor. Email={Email}", model.Email);
                return RedirectToAction("SetPassword", new { email = model.Email });
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Şifre gereklidir.");
                return View(model);
            }

            _logger.LogDebug("Kullanıcı bulundu: ID={Id}, UserName={UserName}, EmailConfirmed={Confirmed}",
                user.Id, user.UserName, user.EmailConfirmed);

            var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!checkPassword)
            {
                _logger.LogWarning("Şifre doğrulama başarısız. Email={Email}", model.Email);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, false, lockoutOnFailure: false);
            _logger.LogInformation("SignInManager sonucu: {Result} — Email={Email}", result, model.Email);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            if (result.RequiresTwoFactor) ModelState.AddModelError(string.Empty, "2FA Required");
            if (result.IsLockedOut) ModelState.AddModelError(string.Empty, "User Locked Out");
            if (result.IsNotAllowed) ModelState.AddModelError(string.Empty, "User Not Allowed (Email Confirmed?)");

            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi. Lütfen bilgilerinizi kontrol edin.");
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
