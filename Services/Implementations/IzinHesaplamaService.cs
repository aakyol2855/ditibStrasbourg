using System;
using DitibStasbourg.Services.Interfaces;

namespace DitibStasbourg.Services.Implementations
{
    public class IzinHesaplamaService : IIzinHesaplamaService
    {
        // Subtracts Sundays automatically from the date boundary range
        public int CalculateJoursOuvrables(DateTime start, DateTime end)
        {
            if (start > end) return 0;
            
            int totalDays = 0;
            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                // DayOfWeek.Sunday is strictly skipped as per standard French legal definitions
                if (date.DayOfWeek != DayOfWeek.Sunday)
                {
                    totalDays++;
                }
            }
            return totalDays;
        }

        // Calculates the accumulated rights matrix at 2.5 days per effective month
        public decimal CalculateTotalAccruedDays(DateTime? fransaGirisTarihi, DateTime? istenCikisTarihi = null)
        {
            if (!fransaGirisTarihi.HasValue) return 0m;

            DateTime endDate = istenCikisTarihi ?? DateTime.Today;
            DateTime startDate = fransaGirisTarihi.Value;
            if (startDate > endDate) return 0m;

            int totalMonths = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;
            
            // Add 1 month if start date is on or before the 15th of the start month
            if (startDate.Day <= 15) { totalMonths += 1; }

            return totalMonths * 2.5m; 
        }
    }
}
