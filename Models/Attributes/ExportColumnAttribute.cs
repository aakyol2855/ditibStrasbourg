namespace DitibStasbourg.Models.Attributes
{
    /// <summary>
    /// Marks a property as exportable to Excel/CSV reports.
    /// The engine reads this attribute reflectively — no entity-specific switch-case needed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ExportColumnAttribute : Attribute
    {
        /// <summary>User-facing column header label (Turkish UI).</summary>
        public string DisplayName { get; }

        /// <summary>Render order in the exported sheet. Lower = further left.</summary>
        public int Order { get; set; } = 999;

        /// <summary>
        /// Optional format string applied when the value is a DateTime or decimal.
        /// Examples: "dd.MM.yyyy", "N2", "0.00".
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// Width hint for ClosedXML column auto-sizing (characters).
        /// 0 means AdjustToContents().
        /// </summary>
        public int FixedWidth { get; set; } = 0;

        /// <summary>
        /// Whether this column is included in the "Quick Export (All Columns)" preset.
        /// Set to false for sensitive/internal fields you never want in bulk exports.
        /// </summary>
        public bool IncludeInQuickExport { get; set; } = true;

        public ExportColumnAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
