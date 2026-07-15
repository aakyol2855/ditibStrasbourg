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

            // Dynamic column sorting
            query = (filter.SortBy?.ToLower(), filter.IsDescending) switch
            {
                ("gorevli", false)  => query.OrderBy(g => g.Gorevli!.Ad).ThenBy(g => g.Gorevli!.Soyad),
                ("gorevli", true)   => query.OrderByDescending(g => g.Gorevli!.Ad).ThenByDescending(g => g.Gorevli!.Soyad),
                ("kurum", false)    => query.OrderBy(g => g.Kurum!.Isim),
                ("kurum", true)     => query.OrderByDescending(g => g.Kurum!.Isim),
                ("bitis", false)    => query.OrderBy(g => g.BitisTarihi),
                ("bitis", true)     => query.OrderByDescending(g => g.BitisTarihi),
                // default: başlangıç tarihi azalan
                _                   => query.OrderByDescending(g => g.Tarih)
            };

            return query;
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
                not.IsDeleted = true;
                _context.Entry(not).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task<string?> CheckOverlapAsync(int gorevliId, DateTime tarih, DateTime? bitisTarihi, int? excludeId = null)
        {
            // Treat open-ended assignments (no BitisTarihi) as running forever.
            var proposedEnd = bitisTarihi ?? DateTime.MaxValue;

            var conflict = await _context.Gorevlendirme
                .Include(g => g.Kurum)
                .Where(g => g.GorevliId == gorevliId
                         && (excludeId == null || g.Id != excludeId)
                         // Interval overlap: existing.Start <= proposed.End AND proposed.Start <= existing.End
                         && g.Tarih <= proposedEnd
                         && tarih <= (g.BitisTarihi ?? DateTime.MaxValue))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return conflict?.Kurum?.Isim;
        }

        public async Task<Dictionary<int, string>> GetActiveAssignmentsLookupAsync()
        {
            var today = DateTime.Today;
            var activeAssignments = await _context.Gorevlendirme
                .AsNoTracking()
                .Include(a => a.Kurum)
                .Where(a => a.Tarih <= today && (a.BitisTarihi == null || a.BitisTarihi >= today))
                .ToListAsync();

            return activeAssignments
                .GroupBy(a => a.GorevliId)
                .ToDictionary(g => g.Key, g => g.First().Kurum?.Isim ?? string.Empty);
        }

        /// <inheritdoc />
        public async Task<byte[]> ExportSelectedPlacementsAsync(int[] ids, string[]? columns)
        {
            var assignments = await dbSet
                .Include(g => g.Gorevli)
                .Include(g => g.Kurum)
                .AsNoTracking()
                .Where(g => ids.Contains(g.Id))
                .OrderByDescending(g => g.Tarih)
                .ToListAsync();

            var activeColumns = (columns == null || columns.Length == 0)
                ? new[] { "BaslangicTarihi", "BitisTarihi", "Gorevli", "Kurum", "KurumTipi" }
                : columns;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Seçili Görevlendirmeler");
            int col = 1;

            if (activeColumns.Contains("BaslangicTarihi")) ws.Cell(1, col++).Value = "Başlangıç Tarihi";
            if (activeColumns.Contains("BitisTarihi"))    ws.Cell(1, col++).Value = "Bitiş Tarihi";
            if (activeColumns.Contains("Gorevli"))        ws.Cell(1, col++).Value = "Görevli";
            if (activeColumns.Contains("Kurum"))          ws.Cell(1, col++).Value = "Kurum";
            if (activeColumns.Contains("KurumTipi"))      ws.Cell(1, col++).Value = "Kurum Tipi";
            if (activeColumns.Contains("Sehir"))          ws.Cell(1, col++).Value = "Şehir";
            if (activeColumns.Contains("GorevliEmail"))   ws.Cell(1, col++).Value = "Görevli E-posta";

            var headerRange = ws.Range(1, 1, 1, col - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

            int row = 2;
            foreach (var item in assignments)
            {
                col = 1;
                if (activeColumns.Contains("BaslangicTarihi")) ws.Cell(row, col++).Value = item.Tarih.ToString("dd.MM.yyyy");
                if (activeColumns.Contains("BitisTarihi"))    ws.Cell(row, col++).Value = item.BitisTarihi.HasValue ? item.BitisTarihi.Value.ToString("dd.MM.yyyy") : "Devam Ediyor";
                if (activeColumns.Contains("Gorevli"))        ws.Cell(row, col++).Value = item.Gorevli?.AdSoyad ?? "";
                if (activeColumns.Contains("Kurum"))          ws.Cell(row, col++).Value = item.Kurum?.Isim ?? "";
                if (activeColumns.Contains("KurumTipi"))      ws.Cell(row, col++).Value = item.Kurum?.Tip.ToString() ?? "";
                if (activeColumns.Contains("Sehir"))          ws.Cell(row, col++).Value = item.Kurum?.Sehir ?? "";
                if (activeColumns.Contains("GorevliEmail"))   ws.Cell(row, col++).Value = item.Gorevli?.Email ?? "";
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <inheritdoc />
        public async Task<bool> BulkSoftDeletePlacementsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0) return false;

            var records = await dbSet
                .Where(g => ids.Contains(g.Id) && !g.IsDeleted)
                .ToListAsync();

            if (!records.Any()) return false;

            var now = DateTime.Now;
            foreach (var rec in records)
            {
                rec.IsDeleted = true;
                rec.DeletedAt = now;
                _context.Entry(rec).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
