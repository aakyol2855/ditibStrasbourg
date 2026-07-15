using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Models;
using DitibStasbourg.Models.Security;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace DitibStasbourg.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        // Create Roles
        string[] roleNames = { "SuperAdmin", "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create Admin User
        var adminEmail = "aakyol28@outlook.com";
        var devEmail = "aakyol28@gmail.com";
        
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null) adminUser = await userManager.FindByNameAsync(adminEmail);
        
        var devUser = await userManager.FindByEmailAsync(devEmail);
        if (devUser == null) devUser = await userManager.FindByNameAsync(devEmail);

        if (adminUser == null)
        {
            var newAdmin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            
            try 
            {
                var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin123!");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "SuperAdmin");
                    adminUser = newAdmin; // Set for next block
                }
                else
                {
                    foreach (var error in createPowerUser.Errors)
                    {
                        logger.LogError("Admin oluşturma hatası: {Description}", error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                 logger.LogError(ex, "Admin oluşturma sırasında istisna");
                 adminUser = await userManager.FindByNameAsync(adminEmail);
            }
        }
        
        if (adminUser != null)
        {
            // If user exists, ensure EmailConfirmed is true
            if (!adminUser.EmailConfirmed)
            {
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);
            }

            // Fix UserName mismatch if exists (e.g. 'admin' vs 'AAKYOL28@OUTLOOK.COM')
            if (adminUser.UserName != adminEmail)
            {
                adminUser.UserName = adminEmail;
                await userManager.UpdateNormalizedUserNameAsync(adminUser);
                await userManager.SetUserNameAsync(adminUser, adminEmail);
                logger.LogInformation("[SEEDER] UserName güncellendi: {Email}", adminEmail);
            }

            // FORCE RESET PASSWORD
            try {
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                 // Note: ResetPasswordAsync might check policy. Forcing known password.
                var result = await userManager.ResetPasswordAsync(adminUser, token, "Admin123!");
                if (!result.Succeeded)
                {
                   foreach (var error in result.Errors)
                   {
                       logger.LogError("Şifre sıfırlama başarısız: {Description}", error.Description);
                   }
                }
                else
                {
                    logger.LogInformation("Parola başarıyla sıfırlandı.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Parola sıfırlama sırasında istisna");
            }

            // Ensure Admin has SuperAdmin role
            if (!await userManager.IsInRoleAsync(adminUser, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
            
            // --- DYNAMIC SECURITY SEEDING ---
            var superAdminTemplate = await context.RoleTemplates.Include(t => t.Claims)
                .FirstOrDefaultAsync(t => t.Name == "SuperAdmin Template");
                
            if (superAdminTemplate == null)
            {
                superAdminTemplate = new RoleTemplate { Name = "SuperAdmin Template" };
                context.RoleTemplates.Add(superAdminTemplate);
                await context.SaveChangesAsync();
            }

            // Reflect all controllers and actions
            var controllers = typeof(Program).Assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var controller in controllers)
            {
                var actions = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName && !m.GetCustomAttributes(typeof(NonActionAttribute), true).Any())
                    .Select(m => m.Name)
                    .Distinct();

                var controllerName = controller.Name.Replace("Controller", "");
                foreach (var action in actions)
                {
                    var claimValue = $"{controllerName}-{action}";
                    if (!superAdminTemplate.Claims.Any(c => c.ClaimValue == claimValue))
                    {
                        superAdminTemplate.Claims.Add(new RoleTemplateClaim { ClaimValue = claimValue });
                    }
                }
            }
            await context.SaveChangesAsync();

            // Force Link Admin to SuperAdmin Template
            var userTemplate = await context.UserRoleTemplates.FirstOrDefaultAsync(u => u.UserId == adminUser.Id);
            if (userTemplate == null)
            {
                context.UserRoleTemplates.Add(new UserRoleTemplate { UserId = adminUser.Id, RoleTemplateId = superAdminTemplate.Id });
            }
            else
            {
                userTemplate.RoleTemplateId = superAdminTemplate.Id; // Force correct template
            }

            // Force Link Dev to SuperAdmin Template
            if (devUser != null)
            {
                if (!await userManager.IsInRoleAsync(devUser, "SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(devUser, "SuperAdmin");
                }
                
                var devTemplate = await context.UserRoleTemplates.FirstOrDefaultAsync(u => u.UserId == devUser.Id);
                if (devTemplate == null)
                {
                    context.UserRoleTemplates.Add(new UserRoleTemplate { UserId = devUser.Id, RoleTemplateId = superAdminTemplate.Id });
                }
                else
                {
                    devTemplate.RoleTemplateId = superAdminTemplate.Id; // Force correct template
                }
            }
        }
        await context.SaveChangesAsync();

        // Seed AppSettings
        var settingExists = await context.AppSettings.AnyAsync(s => s.Key == "SoftDeleteRetentionDays");
        if (!settingExists)
        {
            context.AppSettings.Add(new AppSetting { Key = "SoftDeleteRetentionDays", Value = "30" });
            await context.SaveChangesAsync();
        }
    }
}
