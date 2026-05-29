using DitibStasbourg.Data;
using DitibStasbourg.Models.Dashboard;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Services.Implementations
{
    public class DashboardPreferenceService : IDashboardPreferenceService
    {
        private readonly ApplicationDbContext _context;

        public DashboardPreferenceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardPreference> GetPreferencesAsync(string userId)
        {
            var pref = await _context.DashboardPreferences.FindAsync(userId);
            if (pref == null)
            {
                return new DashboardPreference { UserId = userId };
            }
            return pref;
        }

        public async Task SavePreferencesAsync(string userId, DashboardPreference preferences)
        {
            var existing = await _context.DashboardPreferences.FindAsync(userId);
            if (existing == null)
            {
                preferences.UserId = userId;
                _context.DashboardPreferences.Add(preferences);
            }
            else
            {
                existing.ShowRegionMap = preferences.ShowRegionMap;
                existing.ShowPersonnelChart = preferences.ShowPersonnelChart;
                existing.ShowAssignmentChart = preferences.ShowAssignmentChart;
                existing.ShowKurbanChart = preferences.ShowKurbanChart;
            }
            await _context.SaveChangesAsync();
        }
    }
}
