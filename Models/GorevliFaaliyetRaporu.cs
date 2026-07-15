using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Enums;

namespace DitibStasbourg.Models
{
    public class GorevliFaaliyetRaporu : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Görevli")]
        public int GorevliId { get; set; }

        [ForeignKey("GorevliId")]
        public Gorevli? Gorevli { get; set; }

        [Required]
        [Display(Name = "Kurum")]
        public int KurumId { get; set; }

        [ForeignKey("KurumId")]
        public Kurum? Kurum { get; set; }

        [Required]
        [Display(Name = "Rapor Tarihi")]
        [DataType(DataType.Date)]
        public DateTime RaporTarihi { get; set; }

        [Required]
        [Display(Name = "Kurs Türü")]
        public KursTuru KursTuru { get; set; }

        [Display(Name = "Katılımcı Sayısı")]
        [Range(0, 10000)]
        public int KatilimciSayisi { get; set; }

        [Display(Name = "Faaliyet Detayı")]
        [StringLength(2000)]
        public string? FaaliyetDetayi { get; set; }

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}
