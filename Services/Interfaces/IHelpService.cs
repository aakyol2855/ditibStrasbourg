using DitibStasbourg.Models;
using DitibStasbourg.Services.Base;
using System.Security.Claims;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IHelpService : IBaseService<HelpTopic>
    {
        Task<IEnumerable<HelpTopic>> GetTopicsByCategoryAsync(string category, ClaimsPrincipal user);
        Task<IEnumerable<string>> GetCategoriesAsync();
    }
}
