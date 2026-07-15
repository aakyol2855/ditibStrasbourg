using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public enum DernekGorselTipi
    {
        [Display(Name = "Cami")]
        Cami = 0,

        [Display(Name = "Lojman")]
        Lojman = 1,

        [Display(Name = "Müştemilat")]
        Mustemilat = 2,

        [Display(Name = "Dış Cephe")]
        DisCephe = 3,

        [Display(Name = "Diğer")]
        Diger = 4
    }

    public class DernekGorsel : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Dernek")]
        public int DernekId { get; set; }

        [ForeignKey("DernekId")]
        public Kurum? Dernek { get; set; }

        [Required]
        [Display(Name = "Görsel Yolu")]
        [StringLength(500)]
        public string GorselYolu { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [StringLength(500)]
        public string? Aciklama { get; set; }

        [Display(Name = "Görsel Tipi")]
        public DernekGorselTipi GorselTipi { get; set; } = DernekGorselTipi.Diger;

        [Display(Name = "Yüklenme Tarihi")]
        public DateTime YuklenmeTarihi { get; set; } = DateTime.UtcNow;

        [Display(Name = "Yükleyen Kullanıcı")]
        [StringLength(200)]
        public string? YukleyenKullanici { get; set; }

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}
