using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DitibStasbourg.Data;

public static class AuthDiagnostics
{
    public static async Task RunDiagnostics(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var email = "AAKYOL28@OUTLOOK.COM";

        logger.LogInformation("============= AUTH DIAGNOSTICS START =============");

        var userByEmail = await userManager.FindByEmailAsync(email);
        logger.LogInformation("FindByEmailAsync('{Email}'): {Result}", email, userByEmail != null ? "FOUND" : "NULL");

        var userByName = await userManager.FindByNameAsync(email);
        logger.LogInformation("FindByNameAsync('{Email}'): {Result}", email, userByName != null ? "FOUND" : "NULL");

        if (userByEmail != null)
        {
            PrintUserDetails(logger, userByEmail, "Found By Email");
        }

        if (userByName != null && (userByEmail == null || userByName.Id != userByEmail.Id))
        {
            PrintUserDetails(logger, userByName, "Found By Name");
        }

        if (userByEmail != null)
        {
            var isPasswordValid = await userManager.CheckPasswordAsync(userByEmail, "Admin123!");
            logger.LogInformation("CheckPasswordAsync('Admin123!'): {IsValid}", isPasswordValid);
        }

        logger.LogInformation("============= AUTH DIAGNOSTICS END =============");
    }

    private static void PrintUserDetails(ILogger logger, IdentityUser user, string source)
    {
        logger.LogInformation("--- User Details ({Source}) --- Id={Id} UserName='{UserName}' NormalizedUserName='{NormUserName}' Email='{Email}' NormalizedEmail='{NormEmail}' EmailConfirmed={EmailConfirmed} SecurityStamp={SecurityStamp} LockoutEnabled={LockoutEnabled} LockoutEnd={LockoutEnd}",
            source,
            user.Id,
            user.UserName,
            user.NormalizedUserName,
            user.Email,
            user.NormalizedEmail,
            user.EmailConfirmed,
            user.SecurityStamp,
            user.LockoutEnabled,
            user.LockoutEnd);
    }
}
