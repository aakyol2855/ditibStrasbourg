using DitibStasbourg.Models.Navigation;
using System.Security.Claims;

namespace DitibStasbourg.Services
{
    public interface IMenuService
    {
        Task<List<MenuItem>> GetUserMenuAsync(ClaimsPrincipal user);
        Task<List<MenuItem>> GetBreadcrumbsAsync(string controller, string action);
    }
}
