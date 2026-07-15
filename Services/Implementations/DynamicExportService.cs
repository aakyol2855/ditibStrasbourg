using System.Reflection;
using ClosedXML.Excel;
using DitibStasbourg.Models.Attributes;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;

namespace DitibStasbourg.Services.Implementations
{
    public class DynamicExportService : IDynamicExportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IIzinHesaplamaService _izinEngine;

        public DynamicExportService(ApplicationDbContext context, IIzinHesaplamaService izinEngine)
        {
            _context = context;
            _izinEngine = izinEngine;
        }
        // ─── Descriptor Cache ─────────────────────────────────────────────────────
        // Reflection is expensive on hot paths. Cache results per entity type.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, IReadOnlyList<ExportColumnDescriptor>>
            _descriptorCache = new();

        // ─── Style Constants ──────────────────────────────────────────────────────
        private static readonly XLColor _headerBg     = XLColor.FromHtml("#1E3A5F");  // DITIB navy
        private static readonly XLColor _headerFg     = XLColor.White;
        private static readonly XLColor _altRowBg     = XLColor.FromHtml("#F0F5FF");  // Light blue tint
        private static readonly XLColor _borderColor  = XLColor.FromHtml("#CBD5E1");

        // ─── IExportService: legacy overload (backwards compat) ──────────────────

        /// <summary>
        /// Legacy overload retained so existing GorevliController.CustomExport compiles
        /// without modification. Uses the same reflection pipeline internally.
        /// </summary>
        public async Task<byte[]> ExportToExcelAsync<T>(IQueryable<T> query, List<string> selectedColumns) where T : class
            => await ExportFilteredAsync(query, selectedColumns, typeof(T).Name);

        // ─── Column Descriptor Discovery ─────────────────────────────────────────

        /// <inheritdoc/>
        public IReadOnlyList<ExportColumnDescriptor> GetColumnDescriptors<T>() where T : class
            => GetColumnDescriptors(typeof(T));

        /// <inheritdoc/>
        public IReadOnlyList<ExportColumnDescriptor> GetColumnDescriptors(Type entityType)
        {
            return _descriptorCache.GetOrAdd(entityType, t =>
            {
                var descriptors = t
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.GetCustomAttribute<ExportColumnAttribute>() != null)
                    .Select(p =>
                    {
                        var attr = p.GetCustomAttribute<ExportColumnAttribute>()!;
                        return new ExportColumnDescriptor
                        {
                            PropertyName       = p.Name,
                            DisplayName        = attr.DisplayName,
                            Order              = attr.Order,
                            IncludeInQuickExport = attr.IncludeInQuickExport,
                            Format             = attr.Format,
                            FixedWidth         = attr.FixedWidth
                        };
                    })
                    .OrderBy(d => d.Order)
                    .ToList()
                    .AsReadOnly();

                return descriptors;
            });
        }

        // ─── Quick Export ─────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<byte[]> QuickExportAllAsync<T>(IQueryable<T> query, string worksheetTitle = "", bool maskSensitiveData = false) where T : class
        {
            var allDescriptors = GetColumnDescriptors<T>()
                .Where(d => d.IncludeInQuickExport)
                .Select(d => d.PropertyName);

            return await ExportFilteredAsync(query, allDescriptors, worksheetTitle.Length > 0 ? worksheetTitle : typeof(T).Name, maskSensitiveData);
        }

        // ─── Core Export Pipeline ─────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<byte[]> ExportFilteredAsync<T>(
            IQueryable<T> query,
            IEnumerable<string> selectedPropertyNames,
            string worksheetTitle = "",
            bool maskSensitiveData = false) where T : class
        {
            // 1. Resolve which descriptors correspond to the requested property names
            var allDescriptors = GetColumnDescriptors<T>();
            var selectedNames  = new HashSet<string>(selectedPropertyNames, StringComparer.OrdinalIgnoreCase);

            var activeDescriptors = allDescriptors
                .Where(d => selectedNames.Contains(d.PropertyName))
                .OrderBy(d => d.Order)
                .ToList();

            // Fall back to all quick-export columns if nothing was selected
            if (!activeDescriptors.Any())
                activeDescriptors = allDescriptors.Where(d => d.IncludeInQuickExport).ToList();

            // 2. Pre-compute PropertyInfo array (avoid per-row reflection lookup)
            var type  = typeof(T);
            var props = activeDescriptors
                .Select(d => (Descriptor: d, PropInfo: type.GetProperty(d.PropertyName, BindingFlags.Public | BindingFlags.Instance)!))
                .Where(x => x.PropInfo != null)
                .ToArray();

            // 3. Materialise data from EF Core
            var data = await query.AsNoTracking().ToListAsync();

            if (typeof(T) == typeof(Gorevli))
            {
                var gorevliler = data.Cast<Gorevli>().ToList();
                var ids = gorevliler.Select(g => g.Id).ToList();

                if (ids.Any())
                {
                    // 1. Fetch active assignments and previous assignments
                    var assignments = await _context.Gorevlendirme
                        .Include(g => g.Kurum)
                        .Where(g => ids.Contains(g.GorevliId) && !g.IsDeleted)
                        .ToListAsync();

                    // 2. Fetch previous duties (GorevGecmisleri)
                    var histories = await _context.GorevGecmisleri
                        .Include(h => h.Kurum)
                        .Where(h => ids.Contains(h.GorevliId))
                        .ToListAsync();

                    // 3. Fetch approved annual leaves (YillikIzin)
                    var leaves = await _context.GorevliIzinler
                        .Where(l => ids.Contains(l.GorevliId) && !l.IsDeleted && l.IzinTuru == DitibStasbourg.Models.Enums.IzinTuru.YillikIzin && l.OnayDurumu == DitibStasbourg.Models.Enums.OnayDurumu.Onaylandi)
                        .ToListAsync();

                    // 4. Fetch notes (GorevliNotlari)
                    var notes = await _context.GorevliNotlari
                        .Where(n => ids.Contains(n.GorevliId) && !n.IsDeleted)
                        .ToListAsync();

                    var today = DateTime.Today;

                    foreach (var g in gorevliler)
                    {
                        // Active Assignment
                        var active = assignments
                            .Where(a => a.GorevliId == g.Id)
                            .FirstOrDefault(a => today >= a.Tarih && (!a.BitisTarihi.HasValue || today <= a.BitisTarihi.Value));
                        g.ExportAktifGorevYeri = active?.Kurum?.Isim ?? "Görevlendirilmemiş";

                        // Previous duties
                        var prevAssignments = assignments
                            .Where(a => a.GorevliId == g.Id && a != active)
                            .Select(a => $"{a.Kurum?.Isim} ({a.Tarih:dd.MM.yyyy} - {(a.BitisTarihi.HasValue ? a.BitisTarihi.Value.ToString("dd.MM.yyyy") : "Devam Ediyor")})");
                        var prevHistories = histories
                            .Where(h => h.GorevliId == g.Id)
                            .Select(h => $"{h.Kurum?.Isim} ({h.BaslangicTarihi:dd.MM.yyyy} - {(h.BitisTarihi.HasValue ? h.BitisTarihi.Value.ToString("dd.MM.yyyy") : "")})");
                        var allPrev = prevAssignments.Concat(prevHistories).Distinct().ToList();
                        g.ExportOncekiGorevYerleri = allPrev.Any() ? string.Join(", ", allPrev) : "-";

                        // Leave balance: Toplam Kazanılan, Kullanılan, Kalan Bakiye
                        var startDate = g.FransaGirisTarihi 
                            ?? assignments.Where(a => a.GorevliId == g.Id && !a.IsDeleted).OrderBy(a => a.Tarih).Select(a => (DateTime?)a.Tarih).FirstOrDefault();
                        var totalAccrued = _izinEngine.CalculateTotalAccruedDays(startDate, null);
                        var totalUsed = leaves.Where(l => l.GorevliId == g.Id).Sum(l => l.ToplamGun);
                        var netBalance = totalAccrued - totalUsed;
                        g.ExportMevcutIzinBilgileri = $"Kazanılan: {totalAccrued:F1}, Kullanılan: {totalUsed}, Bakiye: {netBalance:F1}";

                        // System notes formatted as a single merged text block
                        var gNotes = notes
                            .Where(n => n.GorevliId == g.Id)
                            .OrderByDescending(n => n.Tarih)
                            .Select(n => $"[{n.Tarih:dd.MM.yyyy}] {n.NotIcerik}");
                        g.ExportSistemNotlari = gNotes.Any() ? string.Join(" | ", gNotes) : "-";
                    }
                }
            }

            // 4. Build workbook with premium styling
            using var workbook  = new XLWorkbook();
            var title           = (worksheetTitle.Length > 0 ? worksheetTitle : type.Name).Truncate(31); // Excel sheet name limit
            var worksheet       = workbook.Worksheets.Add(title);

            // ── Header Row ──────────────────────────────────────────────────────
            for (int col = 0; col < props.Length; col++)
            {
                var cell = worksheet.Cell(1, col + 1);
                cell.Value                         = props[col].Descriptor.DisplayName;
                cell.Style.Font.Bold               = true;
                cell.Style.Font.FontSize           = 11;
                cell.Style.Font.FontColor          = _headerFg;
                cell.Style.Fill.BackgroundColor    = _headerBg;
                cell.Style.Alignment.Horizontal    = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical      = XLAlignmentVerticalValues.Center;
                cell.Style.Border.BottomBorder     = XLBorderStyleValues.Thick;
                cell.Style.Border.BottomBorderColor = XLColor.FromHtml("#3B82F6");
            }

            worksheet.Row(1).Height = 22;

            // ── Data Rows ───────────────────────────────────────────────────────
            int rowIndex = 2;
            foreach (var item in data)
            {
                bool isAltRow = (rowIndex % 2 == 0);

                for (int col = 0; col < props.Length; col++)
                {
                    var (descriptor, propInfo) = props[col];
                    var rawValue = propInfo.GetValue(item);
                    rawValue = GetMaskedValueIfNeeded(descriptor.PropertyName, rawValue, maskSensitiveData);
                    var cell     = worksheet.Cell(rowIndex, col + 1);

                    // Apply formatted value
                    SetCellValue(cell, rawValue, descriptor.Format);

                    // Alternating row tint
                    if (isAltRow)
                        cell.Style.Fill.BackgroundColor = _altRowBg;

                    // Subtle border
                    cell.Style.Border.BottomBorder     = XLBorderStyleValues.Thin;
                    cell.Style.Border.BottomBorderColor = _borderColor;
                    cell.Style.Border.RightBorder      = XLBorderStyleValues.Thin;
                    cell.Style.Border.RightBorderColor  = _borderColor;
                }
                rowIndex++;
            }

            // ── Column Sizing ──────────────────────────────────────────────────
            for (int col = 0; col < props.Length; col++)
            {
                var colRange = worksheet.Column(col + 1);
                int fixedW   = props[col].Descriptor.FixedWidth;
                if (fixedW > 0)
                    colRange.Width = fixedW;
                else
                    colRange.AdjustToContents(minWidth: 12, maxWidth: 60);
            }

            // ── Freeze Header + Auto-filter ─────────────────────────────────
            worksheet.SheetView.FreezeRows(1);
            if (data.Any() && props.Length > 0)
                worksheet.Range(1, 1, 1, props.Length).SetAutoFilter();

            // ── Footer metadata row ─────────────────────────────────────────
            int footerRow = rowIndex + 1;
            var footerCell = worksheet.Cell(footerRow, 1);
            footerCell.Value = $"DİTİB Strasbourg CoreNexus • Dışa aktarıldı: {DateTime.Now:dd.MM.yyyy HH:mm} • Toplam: {data.Count} kayıt";
            footerCell.Style.Font.Italic = true;
            footerCell.Style.Font.FontSize = 9;
            footerCell.Style.Font.FontColor = XLColor.Gray;
            if (props.Length > 1)
                worksheet.Range(footerRow, 1, footerRow, props.Length).Merge();

            // 5. Stream to byte array
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static void SetCellValue(IXLCell cell, object? rawValue, string? format)
        {
            if (rawValue == null)
            {
                cell.Value = "";
                return;
            }

            switch (rawValue)
            {
                case DateTime dt:
                    if (!string.IsNullOrEmpty(format))
                    {
                        cell.Value = dt.ToString(format);
                    }
                    else
                    {
                        cell.Value = dt;
                        cell.Style.DateFormat.Format = "dd.MM.yyyy";
                    }
                    break;

                case decimal d:
                    cell.Value = d;
                    if (!string.IsNullOrEmpty(format))
                        cell.Style.NumberFormat.Format = format;
                    break;

                case bool b:
                    cell.Value = b ? "Evet" : "Hayır";
                    break;

                case Enum e:
                    // Use Display attribute name if available
                    var memberInfo = e.GetType().GetMember(e.ToString()).FirstOrDefault();
                    var displayAttr = memberInfo?.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                    cell.Value = displayAttr?.Name ?? e.ToString();
                    break;

                default:
                    cell.Value = rawValue.ToString() ?? "";
                    break;
            }
        }

        private static object? GetMaskedValueIfNeeded(string propertyName, object? rawValue, bool maskSensitiveData)
        {
            if (!maskSensitiveData || rawValue == null) return rawValue;

            var propLower = propertyName.ToLowerInvariant();
            if (propLower == "tckimlikno")
            {
                var tcStr = rawValue.ToString();
                if (string.IsNullOrEmpty(tcStr)) return tcStr;
                if (tcStr.Length == 11)
                    return tcStr.Substring(0, 3) + "******" + tcStr.Substring(9, 2);
                return "******";
            }
            else if (propLower == "ceptelefonu" || propLower == "evtelefonu")
            {
                var phoneStr = rawValue.ToString();
                if (string.IsNullOrEmpty(phoneStr)) return phoneStr;
                if (phoneStr.Length > 6)
                    return phoneStr.Substring(0, 3) + "******" + phoneStr.Substring(phoneStr.Length - 2);
                return "***-***";
            }

            return rawValue;
        }
    }

    internal static class StringExtensions
    {
        internal static string Truncate(this string s, int maxLength)
            => s.Length <= maxLength ? s : s[..maxLength];
    }
}
