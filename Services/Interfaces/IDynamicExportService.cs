using DitibStasbourg.Models.Attributes;

namespace DitibStasbourg.Services.Interfaces
{
    /// <summary>
    /// Describes a single exportable column discovered via reflection.
    /// Used by the advanced export panel to render the dynamic checklist UI.
    /// </summary>
    public sealed class ExportColumnDescriptor
    {
        /// <summary>C# property name on the entity (used as the key sent from the UI).</summary>
        public string PropertyName { get; init; } = "";

        /// <summary>User-facing Turkish label rendered in the checklist and as the Excel column header.</summary>
        public string DisplayName { get; init; } = "";

        /// <summary>Render order in the exported spreadsheet.</summary>
        public int Order { get; init; }

        /// <summary>Whether this column is pre-selected in the "Quick Export" preset.</summary>
        public bool IncludeInQuickExport { get; init; }

        /// <summary>Optional formatting hint (e.g. "dd.MM.yyyy").</summary>
        public string? Format { get; init; }

        /// <summary>Fixed column width (0 = auto).</summary>
        public int FixedWidth { get; init; }
    }

    /// <summary>
    /// Metadata-driven, generic Excel export engine.
    /// Fully decoupled from entity types via C# Generics + Reflection.
    /// Enforces Open/Closed Principle: new entities gain export support
    /// simply by decorating their properties with [ExportColumn], zero code changes here.
    /// </summary>
    public interface IDynamicExportService
    {
        /// <summary>
        /// Legacy overload — kept for backwards compatibility with existing GorevliController.CustomExport.
        /// </summary>
        Task<byte[]> ExportToExcelAsync<T>(IQueryable<T> query, List<string> selectedColumns) where T : class;

        /// <summary>
        /// Reflectively inspects <typeparamref name="T"/> and returns all properties decorated with
        /// [ExportColumn] sorted by their Order value. Powers the dynamic checklist UI.
        /// </summary>
        IReadOnlyList<ExportColumnDescriptor> GetColumnDescriptors<T>() where T : class;

        /// <summary>
        /// Reflectively inspects <typeparamref name="T"/> and returns all properties decorated with
        /// [ExportColumn] sorted by their Order value from the entity Type object at runtime.
        /// Overload for cases where T is not known at compile-time (used by the generic endpoint).
        /// </summary>
        IReadOnlyList<ExportColumnDescriptor> GetColumnDescriptors(Type entityType);

        /// <summary>
        /// Core export pipeline. Selectively maps only <paramref name="selectedPropertyNames"/> onto
        /// the spreadsheet rows. Stream is written to memory and returned as a raw byte array.
        /// </summary>
        Task<byte[]> ExportFilteredAsync<T>(
            IQueryable<T> query,
            IEnumerable<string> selectedPropertyNames,
            string worksheetTitle = "",
            bool maskSensitiveData = false) where T : class;

        /// <summary>
        /// Quick-export preset: automatically includes all columns where IncludeInQuickExport == true.
        /// </summary>
        Task<byte[]> QuickExportAllAsync<T>(IQueryable<T> query, string worksheetTitle = "", bool maskSensitiveData = false) where T : class;
    }
}
