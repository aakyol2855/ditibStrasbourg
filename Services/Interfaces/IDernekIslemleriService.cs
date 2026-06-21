using DitibStasbourg.Models;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IDernekIslemleriService
    {
        IQueryable<Kurum> GetFilteredQueryable(string? search = null, string? sehir = null, string? bolge = null);
        Task<List<Kurum>> GetActiveDerneklerAsync();
        Task<List<string>> GetSehirlerAsync();
        Task<List<Ref_KurumTuru>> GetUstKurumlarAsync();
        Task<Kurum?> GetDernekDetayAsync(int id);
        Task<Kurum> CreateDernekAsync(Kurum dernek);
        Task UpdateBaskanAsync(int id, string ad, string iletisim, string? baskanMail);
        Task UpdateDinGorevlisiAsync(int id, string ad, string iletisim);
        Task AddUyeAsync(DernekUye uye);
        Task DeleteUyeAsync(int id);
        Task<bool> UpdateUyeAsync(int id, string adSoyad, string iletisim, int aileUyeSayisi);
        Task<bool> UpdateDernekAsync(int id, string isim, string? sehir, string? adres, string? kurulusKanunu, string? baskonsoloslukBolgesi, string? bolge, string? crmUyelikFormDurumu, int? ustKurumId, string? iletisimNumarasi, string? maili, double? latitude, double? longitude, int? cemaatCount, string? frenchRegistrationName, List<KurumYonetimKuruluUyesi>? yonetimKurulu);
        Task<bool> SoftDeleteDernekAsync(int id);
        Task<PaginatedList<Kurum>> GetPaginatedDerneklerAsync(string? search, string? sehir, string? bolge, int pageIndex, int pageSize);
        Task<List<Ref_YonetimRol>> GetYonetimRolleriAsync();
    }
}
