using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Base;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace DitibStasbourg.Services.Implementations
{
    public class GorevlendirmeService : BaseService<Gorevlendirme>, IGorevlendirmeService
    {
        public GorevlendirmeService(ApplicationDbContext context, ILogger<GorevlendirmeService> logger) : base(context, logger)
        {
        }

        public IQueryable<Gorevlendirme> GetFilteredQueryable(GorevlendirmeFilterViewModel filter)
        {
            var query = dbSet
                .Include(g => g.Gorevli)
                    .ThenInclude(gov => gov.GorevliDurumBilgisi)
                .Include(g => g.Kurum)
                .Include(g => g.YerineGelecekGorevli)
                .AsNoTracking()
                .AsQueryable();

            if (filter.GorevliId.HasValue) query = query.Where(g => g.GorevliId == filter.GorevliId.Value);
            if (filter.KurumId.HasValue) query = query.Where(g => g.KurumId == filter.KurumId.Value);
            if (filter.BaslangicTarihi.HasValue) query = query.Where(g => g.Tarih >= filter.BaslangicTarihi.Value);
            if (filter.BitisTarihi.HasValue) query = query.Where(g => g.Tarih <= filter.BitisTarihi.Value);
            if (!string.IsNullOrEmpty(filter.Sehir)) query = query.Where(g => g.Kurum.Sehir == filter.Sehir);

            var today = DateTime.Today;
            if (!string.IsNullOrEmpty(filter.DurumFilter))
            {
                if (filter.DurumFilter == "aktif")
                    query = query.Where(g => g.Tarih <= today && (g.BitisTarihi == null || g.BitisTarihi >= today));
                else if (filter.DurumFilter == "pasif")
                    query = query.Where(g => g.BitisTarihi != null && g.BitisTarihi < today);
            }

            return query.OrderByDescending(g => g.Tarih);
        }

        public async Task<PaginatedList<Gorevlendirme>> GetFilteredGorevlendirmelerAsync(GorevlendirmeFilterViewModel filter, int pageSize)
        {
            var query = GetFilteredQueryable(filter);
            return await PaginatedList<Gorevlendirme>.CreateAsync(query, filter.PageNumber ?? 1, pageSize);
        }

        public async Task<Gorevlendirme?> GetGorevlendirmeDetailsAsync(int id)
        {
            return await dbSet
                .Include(g => g.Gorevli)
                .Include(g => g.Kurum)
                .Include(g => g.YerineGelecekGorevli)
                .Include(g => g.GorevlendirmeNotlari)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<byte[]> ExportToExcelAsync(int? year, KurumTip? tip, int? gorevliId, int? kurumId, DateTime? startDate, DateTime? endDate, List<string> columns)
        {
            var query = dbSet
                .Include(g => g.Gorevli)
                .Include(g => g.Kurum)
                .AsNoTracking()
                .AsQueryable();

            if (year.HasValue) query = query.Where(g => g.Tarih.Year == year.Value);
            if (tip.HasValue) query = query.Where(g => g.Kurum.Tip == tip.Value);
            if (gorevliId.HasValue) query = query.Where(g => g.GorevliId == gorevliId.Value);
            if (kurumId.HasValue) query = query.Where(g => g.KurumId == kurumId.Value);
            if (startDate.HasValue) query = query.Where(g => g.Tarih >= startDate.Value);
            if (endDate.HasValue) query = query.Where(g => g.Tarih <= endDate.Value);

            var assignments = await query.OrderByDescending(g => g.Tarih).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Görevlendirmeler");
            var currentRow = 1;
            int colIndex = 1;

            if (columns == null || !columns.Any()) 
            {
                columns = new List<string> { "BaslangicTarihi", "BitisTarihi", "Gorevli", "Kurum", "KurumTipi" };
            }

            if (columns.Contains("BaslangicTarihi")) worksheet.Cell(currentRow, colIndex++).Value = "Başlangıç Tarihi";
            if (columns.Contains("BitisTarihi")) worksheet.Cell(currentRow, colIndex++).Value = "Bitiş Tarihi";
            if (columns.Contains("Gorevli")) worksheet.Cell(currentRow, colIndex++).Value = "Görevli";
            if (columns.Contains("Kurum")) worksheet.Cell(currentRow, colIndex++).Value = "Kurum";
            if (columns.Contains("KurumTipi")) worksheet.Cell(currentRow, colIndex++).Value = "Kurum Tipi";
            if (columns.Contains("GorevliEmail")) worksheet.Cell(currentRow, colIndex++).Value = "Görevli Email";

            foreach (var item in assignments)
            {
                currentRow++;
                colIndex = 1;
                if (columns.Contains("BaslangicTarihi")) worksheet.Cell(currentRow, colIndex++).Value = item.Tarih;
                if (columns.Contains("BitisTarihi")) worksheet.Cell(currentRow, colIndex++).Value = item.BitisTarihi.HasValue ? item.BitisTarihi.Value : "Devam Ediyor";
                if (columns.Contains("Gorevli")) worksheet.Cell(currentRow, colIndex++).Value = item.Gorevli?.AdSoyad;
                if (columns.Contains("Kurum")) worksheet.Cell(currentRow, colIndex++).Value = item.Kurum?.Isim;
                if (columns.Contains("KurumTipi")) worksheet.Cell(currentRow, colIndex++).Value = item.Kurum?.Tip.ToString();
                if (columns.Contains("GorevliEmail")) worksheet.Cell(currentRow, colIndex++).Value = item.Gorevli?.Email;
            }
            
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task AddNoteAsync(int gorevlendirmeId, string notIcerik, string? userName)
        {
            var not = new GorevlendirmeNot
            {
                GorevlendirmeId = gorevlendirmeId,
                NotIcerik = notIcerik,
                Tarih = DateTime.Now,
                YazanKisiId = userName
            };

            _context.GorevlendirmeNotlari.Add(not);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(int noteId)
        {
            var not = await _context.GorevlendirmeNotlari.FindAsync(noteId);
            if (not != null)
            {
                _context.GorevlendirmeNotlari.Remove(not);
                await _context.SaveChangesAsync();
            }
        }
    }
}
