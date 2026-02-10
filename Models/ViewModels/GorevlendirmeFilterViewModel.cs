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
    }
}
