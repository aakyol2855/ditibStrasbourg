using DitibStasbourg.Data;
using DitibStasbourg.Models.Security;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Services.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserManagementService(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Roles = roles
                });
            }

            return userViewModels;
        }

        public async Task<UserEditViewModel?> GetUserForEditAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var userTemplate = await _context.UserRoleTemplates.FirstOrDefaultAsync(u => u.UserId == userId);
            var overrides = await _context.UserClaimOverrides.Where(o => o.UserId == userId).ToListAsync();

            return new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                RoleTemplateId = userTemplate?.RoleTemplateId,
                AddedClaims = overrides.Where(o => !o.IsDenied).Select(o => o.ClaimValue).ToList(),
                DeniedClaims = overrides.Where(o => o.IsDenied).Select(o => o.ClaimValue).ToList()
            };
        }

        public async Task<IdentityResult> UpdateUserAsync(UserEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı bulunamadı" });

            // LAST SUPERADMIN PROTECTION
            var superAdminRole = "SuperAdmin";
            var superAdminTemplate = await _context.RoleTemplates.FirstOrDefaultAsync(t => t.Name == "SuperAdmin Template");
            
            if (superAdminTemplate != null)
            {
                var isSuperAdmin = await _userManager.IsInRoleAsync(user, superAdminRole);
                var hasSuperTemplate = await _context.UserRoleTemplates.AnyAsync(ut => ut.UserId == user.Id && ut.RoleTemplateId == superAdminTemplate.Id);

                if (isSuperAdmin || hasSuperTemplate)
                {
                    var otherSuperAdminsCount = await _context.UserRoleTemplates.CountAsync(ut => ut.RoleTemplateId == superAdminTemplate.Id && ut.UserId != user.Id);
                    if (otherSuperAdminsCount == 0 && (!model.RoleTemplateId.HasValue || model.RoleTemplateId != superAdminTemplate.Id))
                    {
                        return IdentityResult.Failed(new IdentityError { Description = "Sistemdeki son SuperAdmin'in yetkisini kaldıramazsınız!" });
                    }
                }
            }

            // Update Basic Info
            user.UserName = model.UserName;
            user.Email = model.Email;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return result;

            // HARD RESET: Remove all existing overrides first
            var existingOverrides = await _context.UserClaimOverrides.Where(o => o.UserId == model.Id).ToListAsync();
            _context.UserClaimOverrides.RemoveRange(existingOverrides);

            // Template Update
            var existingTemplateLink = await _context.UserRoleTemplates.FirstOrDefaultAsync(u => u.UserId == model.Id);
            if (model.RoleTemplateId.HasValue)
            {
                if (existingTemplateLink == null)
                    _context.UserRoleTemplates.Add(new UserRoleTemplate { UserId = model.Id, RoleTemplateId = model.RoleTemplateId.Value });
                else
                    existingTemplateLink.RoleTemplateId = model.RoleTemplateId.Value;
            }
            else if (existingTemplateLink != null)
            {
                _context.UserRoleTemplates.Remove(existingTemplateLink);
            }

            // Sync ASP.NET Identity Roles based on Template (Policy: SuperAdmin Template = SuperAdmin Role)
            if (superAdminTemplate != null)
            {
                if (model.RoleTemplateId == superAdminTemplate.Id)
                {
                    if (!await _userManager.IsInRoleAsync(user, superAdminRole))
                        await _userManager.AddToRoleAsync(user, superAdminRole);
                }
                else
                {
                    if (await _userManager.IsInRoleAsync(user, superAdminRole))
                        await _userManager.RemoveFromRoleAsync(user, superAdminRole);
                }
            }

            // Apply new overrides (Manual additions/denials)
            foreach (var added in model.AddedClaims ?? new List<string>())
                _context.UserClaimOverrides.Add(new UserClaimOverride { UserId = model.Id, ClaimValue = added, IsDenied = false });

            foreach (var denied in model.DeniedClaims ?? new List<string>())
                _context.UserClaimOverrides.Add(new UserClaimOverride { UserId = model.Id, ClaimValue = denied, IsDenied = true });

            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı bulunamadı" });

            // LAST SUPERADMIN PROTECTION
            var superAdminTemplate = await _context.RoleTemplates.FirstOrDefaultAsync(t => t.Name == "SuperAdmin Template");
            if (superAdminTemplate != null)
            {
                var hasSuperTemplate = await _context.UserRoleTemplates.AnyAsync(ut => ut.UserId == user.Id && ut.RoleTemplateId == superAdminTemplate.Id);
                if (hasSuperTemplate)
                {
                    var otherSuperAdminsCount = await _context.UserRoleTemplates.CountAsync(ut => ut.RoleTemplateId == superAdminTemplate.Id && ut.UserId != user.Id);
                    if (otherSuperAdminsCount == 0)
                    {
                        return IdentityResult.Failed(new IdentityError { Description = "Sistemdeki son SuperAdmin'i silemezsiniz!" });
                    }
                }
            }

            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> ResetUserPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public Dictionary<string, List<string>> GetAllSystemClaims()
        {
            var claims = new Dictionary<string, List<string>>();
            var controllers = typeof(Program).Assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var ctrl in controllers)
            {
                var actions = ctrl.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly)
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
            return claims;
        }
    }
}
