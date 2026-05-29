using DitibStasbourg.Models.Dashboard;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IDashboardPreferenceService
    {
        Task<DashboardPreference> GetPreferencesAsync(string userId);
        Task SavePreferencesAsync(string userId, DashboardPreference preferences);
    }
}
