using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models.ViewModels
{
    public class GorevlendirmeFilterViewModel
    {
        public int? GorevliId { get; set; }
        public int? KurumId { get; set; }
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public bool? AktifMi { get; set; } // Currently active assignments
        public string? Sehir { get; set; }
        public string? DurumFilter { get; set; } // "aktif", "pasif", "tumunu"
        public int? PageNumber { get; set; }
        public string? Bolge { get; set; }
        public string? Dernek { get; set; }
        public string? Gorevli { get; set; }

        // Sorting support (passed via URL query string)
        public string? SortBy { get; set; }       // e.g. "Gorevli", "Kurum", "BaslangicTarihi", "BitisTarihi"
        public bool IsDescending { get; set; } = true;
    }
}
