using DitibStasbourg.Services.Interfaces;

namespace DitibStasbourg.Services.Security;

/// <summary>
/// Intercepts forced sign-out redirects caused by <see cref="Microsoft.AspNetCore.Identity.SecurityStampValidator{TUser}"/>.
///
/// When <c>SecurityStampValidatorOptions.ValidationInterval = TimeSpan.Zero</c>, Identity verifies
/// the security stamp on every request. If the stamp has changed (e.g. password reset, role change),
/// Identity signs the user out and issues a 302 redirect to /Account/Login.
///
/// This middleware detects that pattern — "was authenticated → now redirected to login" — and
/// writes a mandatory <c>Warning</c> entry to <see cref="ISystemAuditLogService"/>.
/// </summary>
public class SecurityStampAuditMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityStampAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Capture the authentication state BEFORE the pipeline runs
        var wasAuthenticated = context.User.Identity?.IsAuthenticated == true;
        var username = context.User.Identity?.Name;

        await _next(context);

        // Detect forced sign-out: user was authenticated, response is a redirect to login
        if (wasAuthenticated
            && !string.IsNullOrEmpty(username)
            && context.Response.StatusCode == StatusCodes.Status302Found)
        {
            var location = context.Response.Headers.Location.ToString();
            if (location.StartsWith("/Account/Login", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Resolve the scoped audit service without capturing it in the constructor
                    // (middleware is singleton — scoped services must be resolved per-request)
                    var auditService = context.RequestServices.GetService<ISystemAuditLogService>();
                    if (auditService != null)
                    {
                        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Bilinmeyen IP";
                        await auditService.LogAsync(
                            level: "Warning",
                            username: username,
                            action: $"Güvenlik damgası uyuşmazlığı: Kullanıcı {username} oturumu sonlandırıldı.",
                            ipAddress: ip,
                            component: "SecurityStampAuditMiddleware"
                        );
                    }
                }
                catch
                {
                    // Audit failures must never crash the response pipeline
                }
            }
        }
    }
}
