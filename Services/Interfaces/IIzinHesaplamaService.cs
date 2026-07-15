using System;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IIzinHesaplamaService
    {
        int CalculateJoursOuvrables(DateTime start, DateTime end);
        decimal CalculateTotalAccruedDays(DateTime? fransaGirisTarihi, DateTime? istenCikisTarihi = null);
    }
}

