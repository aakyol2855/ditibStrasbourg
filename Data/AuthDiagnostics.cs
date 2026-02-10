using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Data;

public static class AuthDiagnostics
{
    public static async Task RunDiagnostics(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var email = "AAKYOL28@OUTLOOK.COM";
        
        Console.WriteLine("============= AUTH DIAGNOSTICS START =============");
        
        var userByEmail = await userManager.FindByEmailAsync(email);
        Console.WriteLine($"FindByEmailAsync('{email}'): {(userByEmail != null ? "FOUND" : "NULL")}");

        var userByName = await userManager.FindByNameAsync(email);
        Console.WriteLine($"FindByNameAsync('{email}'): {(userByName != null ? "FOUND" : "NULL")}");

        // Raw SQL check if possible or simplified
        if (userByEmail != null)
        {
            PrintUserDetails(userByEmail, "Found By Email");
        }
        
        if (userByName != null && (userByEmail == null || userByName.Id != userByEmail.Id))
        {
            PrintUserDetails(userByName, "Found By Name");
        }
        
        // Check Password
        if (userByEmail != null)
        {
            var isPasswordValid = await userManager.CheckPasswordAsync(userByEmail, "Admin123!");
            Console.WriteLine($"CheckPasswordAsync('Admin123!'): {isPasswordValid}");
        }

        Console.WriteLine("============= AUTH DIAGNOSTICS END =============");
    }

    private static void PrintUserDetails(IdentityUser user, string source)
    {
        Console.WriteLine($"--- User Details ({source}) ---");
        Console.WriteLine($"Id: {user.Id}");
        Console.WriteLine($"UserName: '{user.UserName}'");
        Console.WriteLine($"NormalizedUserName: '{user.NormalizedUserName}'");
        Console.WriteLine($"Email: '{user.Email}'");
        Console.WriteLine($"NormalizedEmail: '{user.NormalizedEmail}'");
        Console.WriteLine($"EmailConfirmed: {user.EmailConfirmed}");
        Console.WriteLine($"PasswordHash: {user.PasswordHash}");
        Console.WriteLine($"SecurityStamp: {user.SecurityStamp}");
        Console.WriteLine($"ConcurrencyStamp: {user.ConcurrencyStamp}");
        Console.WriteLine($"LockoutEnabled: {user.LockoutEnabled}");
        Console.WriteLine($"LockoutEnd: {user.LockoutEnd}");
        Console.WriteLine("-----------------------------------");
    }
}
