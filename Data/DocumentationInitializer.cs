using System.Text.Json;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DitibStasbourg.Data
{
    public static class DocumentationInitializer
    {
        public static async Task SeedHelpTopicsAsync(ApplicationDbContext context, ILogger? logger = null)
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "initial_docs.json");
            if (!File.Exists(jsonPath)) return;

            try
            {
                var jsonContent = await File.ReadAllTextAsync(jsonPath);
                var topics = JsonSerializer.Deserialize<List<HelpTopic>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (topics == null) return;

                foreach (var topic in topics)
                {
                    var exists = await context.HelpTopics.AnyAsync(h => h.Title == topic.Title);
                    if (!exists)
                    {
                        topic.CreatedAt = DateTime.Now;
                        context.HelpTopics.Add(topic);
                    }
                    else
                    {
                        // Sync existing content if updated in JSON (Optional strategy)
                        var existing = await context.HelpTopics.FirstAsync(h => h.Title == topic.Title);
                        existing.Content = topic.Content;
                        existing.Category = topic.Category;
                        existing.DisplayOrder = topic.DisplayOrder;
                        existing.RequiredClaim = topic.RequiredClaim;
                        existing.ImageUrl = topic.ImageUrl;
                        existing.UpdatedAt = DateTime.Now;
                    }
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Dokümantasyon seed işlemi sırasında hata oluştu");
            }
        }
    }
}
