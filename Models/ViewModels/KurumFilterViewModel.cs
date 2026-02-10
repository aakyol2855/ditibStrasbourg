using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models.ViewModels
{
    public class KurumFilterViewModel
    {
        public string? SearchString { get; set; }
        public KurumTip? Tip { get; set; }
        public string? Sehir { get; set; }
        public bool? AktifMi { get; set; }
        public int? UstKurumId { get; set; }
    }
}
