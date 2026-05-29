namespace DitibStasbourg.Services.Interfaces
{
    public interface IDynamicExportService
    {
        Task<byte[]> ExportToExcelAsync<T>(IQueryable<T> query, List<string> selectedColumns) where T : class;
    }
}
