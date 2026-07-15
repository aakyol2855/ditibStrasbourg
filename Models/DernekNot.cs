using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class DernekNot : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Dernek")]
        public int DernekId { get; set; }

        [ForeignKey("DernekId")]
        public Kurum? Dernek { get; set; }

        [Required]
        [Display(Name = "Not İçeriği")]
        [StringLength(2000)]
        public string NotIcerigi { get; set; } = string.Empty;

        [Display(Name = "Kayıt Tarihi")]
        public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

        [Display(Name = "Hatırlatma / Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BitisTarihi { get; set; }

        [Display(Name = "Ekleyen Kullanıcı")]
        [StringLength(200)]
        public string? EkleyenKullanici { get; set; }

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}
