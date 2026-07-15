using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Enums;

namespace DitibStasbourg.Models
{
    public class GorevliIzin : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Görevli")]
        public int GorevliId { get; set; }

        [ForeignKey("GorevliId")]
        public Gorevli? Gorevli { get; set; }

        [Required]
        [Display(Name = "İzin Türü")]
        public IzinTuru IzinTuru { get; set; }

        [Required]
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BaslangicTarihi { get; set; }

        [Required]
        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BitisTarihi { get; set; }

        [Display(Name = "Toplam Gün")]
        public int ToplamGun { get; set; }

        [Display(Name = "İzin Adresi")]
        [StringLength(500)]
        public string? IzinAdresi { get; set; }

        [Display(Name = "İzin Telefonu")]
        [StringLength(30)]
        public string? IzinTelefonu { get; set; }

        [Required]
        [Display(Name = "Durum")]
        public OnayDurumu OnayDurumu { get; set; } = OnayDurumu.Beklemede;

        [Display(Name = "Manuel Giriş Mi?")]
        public bool IsManualEntryByAdmin { get; set; } = false;

        [Display(Name = "Evrak No")]
        [StringLength(100)]
        public string? EvrakNo { get; set; }

        [Display(Name = "Talep Tarihi")]
        [DataType(DataType.Date)]
        public DateTime TalepTarihi { get; set; } = DateTime.UtcNow;

        [Display(Name = "Onaylayan Yetkili")]
        public string? OnaylayanKisi { get; set; }

        [Display(Name = "Onay Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? OnayTarihi { get; set; }

        [Display(Name = "Açıklama / Not")]
        [StringLength(1000)]
        public string? Aciklama { get; set; }

        [Display(Name = "Evrak Dosya Yolu")]
        [StringLength(500)]
        public string? EvrakDosyaYolu { get; set; } // Stores path to uploaded PDF/Scanned image form

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}
