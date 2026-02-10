using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public enum KurumTip
    {
        Cami,
        Dernek
    }

    public class Kurum
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "İsim")]
        public string Isim { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Required]
        [Display(Name = "Tip")]
        public KurumTip Tip { get; set; }

        [Display(Name = "Üst Kurum")]
        public int? UstKurumId { get; set; }

        [ForeignKey("UstKurumId")]
        public Ref_KurumTuru? UstKurum { get; set; }

        [Display(Name = "Şehir")]
        public string? Sehir { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool AktifMi { get; set; } = true;

        // New Fields for Dernek Operations
        [Display(Name = "Başkonsolosluk Bölgesi")]
        public string? BaskonsoloslukBolgesi { get; set; }

        [Display(Name = "CRM Üyelik Form Durumu")]
        public string? CrmUyelikFormDurumu { get; set; } // e.g., "Var", "Yok", "Beklemede"

        [Display(Name = "Kuruluş Kanunu")]
        public string? KurulusKanunu { get; set; }

        [Display(Name = "Bölge")]
        public string? Bolge { get; set; }

        [Display(Name = "Dernek Başkanı Adı")]
        public string? DernekBaskaniAd { get; set; }

        [Display(Name = "Dernek Başkanı İletişim")]
        public string? DernekBaskaniIletisim { get; set; }

        [Display(Name = "Din Görevlisi Adı")]
        public string? DinGorevlisiAd { get; set; }

        [Display(Name = "Din Görevlisi İletişim")]
        public string? DinGorevlisiIletisim { get; set; }

        // Navigation property for assignments
        public ICollection<Gorevlendirme> Gorevlendirmeler { get; set; } = new List<Gorevlendirme>();
        
        // Navigation for Members
        public ICollection<DernekUye> DernekUyeleri { get; set; } = new List<DernekUye>();
    }
}
