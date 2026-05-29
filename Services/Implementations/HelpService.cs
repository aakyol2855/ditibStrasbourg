using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Base;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DitibStasbourg.Services.Implementations
{
    public class HelpService : BaseService<HelpTopic>, IHelpService
    {
        public HelpService(ApplicationDbContext context, ILogger<HelpService> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<HelpTopic>> GetTopicsByCategoryAsync(string category, ClaimsPrincipal user)
        {
            var query = dbSet
                .Where(t => t.Category == category)
                .AsNoTracking();

            var topics = await query.ToListAsync();

            // Filter by claims
            return topics.Where(t => string.IsNullOrEmpty(t.RequiredClaim) || user.HasClaim("Permission", t.RequiredClaim))
                         .OrderBy(t => t.DisplayOrder);
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await dbSet
                .Select(t => t.Category)
                .Distinct()
                .ToListAsync();
        }

    }
}
