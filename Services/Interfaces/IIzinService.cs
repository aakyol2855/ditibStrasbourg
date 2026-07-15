using DitibStasbourg.Models;
using DitibStasbourg.Models.Enums;
using DitibStasbourg.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IIzinService
    {
        Task<List<IzinListDto>> GetIzinsAsync(int? gorevliId);
        Task<List<int>> GetAvailableYearsAsync();
        Task<GorevliIzin?> GetByIdAsync(int id);
        Task<GorevliIzin?> GetDetailsAsync(int id);
        Task AddAsync(GorevliIzin request);
        Task UpdateStatusAsync(int id, OnayDurumu durum, string? username);
        Task SaveChangesAsync();
        Task<List<dynamic>> GetGorevlilerSelectListAsync();
        Task<List<Gorevli>> GetMerkezStaffAsync(int? year);
        Task<List<Gorevli>> GetOtherStaffAsync(int? year);
    }

    public class IzinListDto
    {
        public int Id { get; set; }
        public int GorevliId { get; set; }
        public string GorevliAdSoyad { get; set; }
        public IzinTuru IzinTuru { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public int ToplamGun { get; set; }
        public decimal AccruedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public bool IsManualEntryByAdmin { get; set; }
        public string? EvrakNo { get; set; }
        public OnayDurumu OnayDurumu { get; set; }
        public string? OnaylayanKisi { get; set; }
    }
}
