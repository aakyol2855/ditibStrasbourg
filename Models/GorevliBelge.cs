using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public enum BelgeTipi
    {
        [Display(Name = "Oturum Kartı")]
        OturumKarti = 0,

        [Display(Name = "Dil Belgesi")]
        DilBelgesi = 1,

        [Display(Name = "Laiklik Belgesi")]
        LaiklikBelgesi = 2,

        [Display(Name = "Pasaport")]
        Pasaport = 3,

        [Display(Name = "Vize")]
        Vize = 4,

        [Display(Name = "Sözleşme")]
        Sozlesme = 5,

        [Display(Name = "Diploma / Mezuniyet")]
        Diploma = 6,

        [Display(Name = "Diğer")]
        Diger = 7
    }

    public class GorevliBelge : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Görevli")]
        public int GorevliId { get; set; }

        [ForeignKey("GorevliId")]
        public Gorevli? Gorevli { get; set; }

        [Required]
        [Display(Name = "Belge Tipi")]
        public BelgeTipi BelgeTipi { get; set; } = BelgeTipi.Diger;

        [Display(Name = "Seri No")]
        [StringLength(100)]
        public string? SeriNo { get; set; }

        [Display(Name = "Geçerlilik Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? GecerlilikTarihi { get; set; }

        [Required]
        [Display(Name = "Dosya Yolu")]
        [StringLength(500)]
        public string DosyaYolu { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [StringLength(500)]
        public string? Aciklama { get; set; }

        [Display(Name = "Yükleyen Kullanıcı")]
        [StringLength(200)]
        public string? YukleyenKullanici { get; set; }

        [Display(Name = "Yüklenme Tarihi")]
        public DateTime YuklenmeTarihi { get; set; } = DateTime.UtcNow;

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Computed helpers
        [NotMapped]
        public bool IsExpiringSoon =>
            GecerlilikTarihi.HasValue &&
            GecerlilikTarihi.Value <= DateTime.Today.AddMonths(3) &&
            GecerlilikTarihi.Value >= DateTime.Today;

        [NotMapped]
        public bool IsExpired =>
            GecerlilikTarihi.HasValue &&
            GecerlilikTarihi.Value < DateTime.Today;
    }
}
