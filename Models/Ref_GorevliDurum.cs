using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class Ref_GorevliDurum
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Durum Adı")]
        public string Ad { get; set; } = string.Empty;

        [Display(Name = "Renk Kodu (Hex veya Class)")]
        public string? Renk { get; set; } // Örn: #FF0000 veya 'bg-danger'

        [Display(Name = "Aktif mi?")]
        public bool AktifMi { get; set; } = true;

        [Display(Name = "Sıra")]
        public int Sira { get; set; } = 0;

        public bool IsDeleted { get; set; } = false;
    }
}
