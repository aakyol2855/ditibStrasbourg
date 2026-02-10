using DitibStasbourg.Models;

namespace DitibStasbourg.Services
{
    public interface ILookupService
    {
        // Gorevli Durum
        Task<List<Ref_GorevliDurum>> GetGorevliDurumlariAsync(bool activeOnly = true);
        Task<Ref_GorevliDurum?> GetGorevliDurumByIdAsync(int id);
        Task AddGorevliDurumAsync(Ref_GorevliDurum durum);
        Task UpdateGorevliDurumAsync(Ref_GorevliDurum durum);
        Task DeleteGorevliDurumAsync(int id);

        // Sozlesme Tip
        Task<List<Ref_SozlesmeTip>> GetSozlesmeTipleriAsync(bool activeOnly = true);
        Task<Ref_SozlesmeTip?> GetSozlesmeTipByIdAsync(int id);
        Task AddSozlesmeTipAsync(Ref_SozlesmeTip tip);
        Task UpdateSozlesmeTipAsync(Ref_SozlesmeTip tip);
        Task DeleteSozlesmeTipAsync(int id);

        // Kurum Turu
        Task<List<Ref_KurumTuru>> GetKurumTurleriAsync(bool activeOnly = true);
        Task<Ref_KurumTuru?> GetKurumTuruByIdAsync(int id);
        Task AddKurumTuruAsync(Ref_KurumTuru tur);
        Task UpdateKurumTuruAsync(Ref_KurumTuru tur);
        Task DeleteKurumTuruAsync(int id);

        // New Reference Tables
        Task<List<Ref_Unvan>> GetUnvanlarAsync(bool activeOnly = true);
        Task<List<Ref_EgitimDurumu>> GetEgitimDurumlariAsync(bool activeOnly = true);
        Task<List<Ref_HafizlikDurumu>> GetHafizlikDurumlariAsync(bool activeOnly = true);
        Task<List<Ref_KanGrubu>> GetKanGruplariAsync(bool activeOnly = true);
        Task<List<Ref_AskerlikDurumu>> GetAskerlikDurumlariAsync(bool activeOnly = true);
        Task<List<Ref_KadroTuru>> GetKadroTurleriAsync(bool activeOnly = true);
    }
}
