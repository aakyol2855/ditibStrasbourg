using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Enums;

namespace DitibStasbourg.Models
{
    public class KurumKasaOdenek : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kurum")]
        public int KurumId { get; set; }
        [ForeignKey("KurumId")]
        public Kurum? Kurum { get; set; }

        [Required]
        [Display(Name = "Transfer Tarihi")]
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Tutar (EUR)")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Tahsis Tipi")]
        public AllocationType AllocationType { get; set; }

        [Display(Name = "Hedef Görevli (İsteğe Bağlı)")]
        public int? TargetGorevliId { get; set; }
        [ForeignKey("TargetGorevliId")]
        public Gorevli? TargetGorevli { get; set; }

        [Display(Name = "Tutanak No")]
        [StringLength(50)]
        public string? TutanakNo { get; set; }

        [Display(Name = "İşlem Yapan")]
        public string? IslemYapan { get; set; }

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
