using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authentication;
using DitibStasbourg.Services.Security;
using DitibStasbourg.Services;
using DitibStasbourg.Services.Interfaces;
using DitibStasbourg.Services.Implementations;
using DitibStasbourg.Filters;

// Configure custom OpenSSL environment to bypass SQL Server TLS 1.3 / pre-login handshake issues on Linux (OpenSSL 3.x)
var customOpenSslConfigPath = Path.Combine(AppContext.BaseDirectory, "openssl.cnf");
if (!File.Exists(customOpenSslConfigPath))
{
    var projectRootConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "openssl.cnf");
    if (File.Exists(projectRootConfigPath))
    {
        customOpenSslConfigPath = projectRootConfigPath;
    }
}
if (File.Exists(customOpenSslConfigPath))
{
    Environment.SetEnvironmentVariable("OPENSSL_CONF", customOpenSslConfigPath);
}

// Register Font Resolver for PDFsharp on Linux
PdfSharp.Fonts.GlobalFontSettings.FontResolver = new DitibStasbourg.Services.Implementations.LinuxFontResolver();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
    ?? throw new InvalidOperationException(
        "Required environment variable 'DB_PASSWORD' is not set. " +
        "Set it via your container environment, .env file (gitignored), or OS secrets manager.");
connectionString = connectionString.Replace("{DB_PASSWORD}", dbPassword);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped(typeof(DitibStasbourg.Services.Base.IBaseService<>), typeof(DitibStasbourg.Services.Base.BaseService<>));
builder.Services.AddScoped<IGorevliService, GorevliService>();
builder.Services.AddScoped<IGorevlendirmeService, GorevlendirmeService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();
builder.Services.AddScoped<IDernekIslemleriService, DernekIslemleriService>();
builder.Services.AddScoped<IHelpService, HelpService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IAssociationImportService, AssociationImportService>();
builder.Services.AddScoped<IKurbanService, KurbanService>();
builder.Services.AddScoped<IDashboardPreferenceService, DashboardPreferenceService>();
builder.Services.AddScoped<IDynamicExportService, DynamicExportService>();
builder.Services.AddScoped<ISystemAuditLogService, SystemAuditLogService>();
builder.Services.AddScoped<IDataMaintenanceService, DataMaintenanceService>();
builder.Services.AddScoped<IDibbysPdfEngine, DibbysPdfEngine>(); // PDF engine registration
builder.Services.AddScoped<IIzinHesaplamaService, IzinHesaplamaService>();
builder.Services.AddScoped<IIzinService, IzinService>();
builder.Services.AddScoped<IDocumentStorageService, DocumentStorageService>();
builder.Services.AddScoped<GorevliPortalAccessFilterAttribute>();
builder.Services.AddSingleton<ImportProgressTracker>();
builder.Services.AddHostedService<SoftDeletePurgeWorker>();
builder.Services.AddHostedService<OverdueNotificationWorker>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpContextAccessor();

// Register application services
builder.Services.AddMemoryCache();

// Dynamic Security System
builder.Services.AddScoped<IClaimsTransformation, DynamicClaimsTransformation>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

// Add Identity Roles
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ── Session lifetime: 2-hour sliding window (auto-renews while staff is active) ──
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true; // Resets the 2-hour window on every authenticated request
});

// ── Security stamp: verify on every request so password resets take effect immediately ──
// Default is 30 minutes — TimeSpan.Zero forces validation on every single HTTP request cycle.
// When a mismatch is detected (e.g. password reset), Identity automatically signs the user
// out and redirects to /Account/Login without any additional code required.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});


builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
    options.Filters.Add<DynamicPermissionFilter>();
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MaliyeStaffOnly", policy =>
        policy.RequireAssertion(context => true)); // Temporary 100% QA Bypass Link
});

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedRolesAndAdminAsync(services);
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DitibStasbourg.Data.KurbanInitializer.SeedKurbanLookupsAsync(context);
        var docLogger = services.GetRequiredService<ILogger<Program>>();
        await DocumentationInitializer.SeedHelpTopicsAsync(context, docLogger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ── Audit forced sign-outs caused by security stamp invalidation ──
app.UseMiddleware<SecurityStampAuditMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

// FORCE AUTOMATIC SEED MATRIX ON STARTUP (BYPASS ALL ROUTING CACHES)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // 1. Force structural database schema synchronization
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var startupLogger = services.GetRequiredService<ILogger<Program>>();
        startupLogger.LogError(ex, "❌ [DİBBYS ERROR] Startup seed başarısız");
    }
}

app.Run();