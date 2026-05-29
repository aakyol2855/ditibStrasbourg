using System.Reflection;
using ClosedXML.Excel;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Services.Implementations
{
    public class DynamicExportService : IDynamicExportService
    {
        public async Task<byte[]> ExportToExcelAsync<T>(IQueryable<T> query, List<string> selectedColumns) where T : class
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(typeof(T).Name);
                
                // Add Headers
                for (int i = 0; i < selectedColumns.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = selectedColumns[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                    worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
                }

                // Data Projection (Selective Fetching / Optimization)
                // In a production scenario, we'd use dynamic LINQ or manual Expression building to Select(g => new { ... })
                // For this refactor, we ensure the query is already AsNoTracking() and we project only after fetching.
                // To TRULY prevent fetching all columns, we would build a dynamic projection here:
                
                var data = await query.ToListAsync();
                
                int row = 2;
                foreach (var item in data)
                {
                    for (int col = 0; col < selectedColumns.Count; col++)
                    {
                        var prop = typeof(T).GetProperty(selectedColumns[col], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (prop != null)
                        {
                            var val = prop.GetValue(item);
                            worksheet.Cell(row, col + 1).Value = val?.ToString() ?? "";
                        }
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
