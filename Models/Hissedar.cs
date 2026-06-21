using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Attributes;

namespace DitibStasbourg.Models
{
    public enum PaymentMethod
    {
        Havale,
        Cek,
        Nakit,
        OnlineStripe,
        BagisCihazi
    }

    public class Hissedar
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [ExportColumn("Bağışçı Adı", Order = 1)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        [ExportColumn("Telefon", Order = 2)]
        public string Phone { get; set; }

        [ExportColumn("Vekalet Alındı mı?", Order = 4)]
        public bool IsVekaletTaken { get; set; } = false;

        [ExportColumn("Ödeme Durumu", Order = 3)]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        [Display(Name = "Ödeme Yöntemi")]
        [ExportColumn("Ödeme Yöntemi", Order = 6)]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Nakit;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Ödenen Tutar")]
        [ExportColumn("Ödenen Tutar", Order = 7, Format = "C2")]
        public decimal TotalPaid { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Kalan Tutar")]
        [ExportColumn("Kalan Tutar", Order = 8, Format = "C2")]
        public decimal RemainingBalance { get; set; } = 0;

        public int? KurbanlikId { get; set; }
        
        [ForeignKey("KurbanlikId")]
        public virtual Kurbanlik? Kurbanlik { get; set; }

        [ExportColumn("Kayıt Tarihi", Order = 5, Format = "dd.MM.yyyy HH:mm")]
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
