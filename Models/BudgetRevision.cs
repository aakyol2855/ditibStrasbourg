using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    /// <summary>
    /// Bütçe Revizyonu / Ek Ödenek kaydı.
    /// Yıl içi enflasyon, acil tadilat veya idari masraf artışlarında
    /// mevcut bütçeyi ezmeden ek bütçe kaydı girmeye olanak tanır.
    /// </summary>
    public class BudgetRevision
    {
        public int Id { get; set; }

        [ForeignKey("KurumButce")]
        public int KurumButceId { get; set; }
        public KurumButce KurumButce { get; set; } = null!;

        [Required]
        [Display(Name = "Revizyon Gerekçesi")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Ek Bütçe Miktarı")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalAmount { get; set; }

        [Display(Name = "Revizyon Türü")]
        [StringLength(50)]
        public string RevisionType { get; set; } = "EkOdenek"; // "EkOdenek", "IndirimKesinti", "EnflasyonDuzeltme"

        [Display(Name = "Onay Durumu")]
        [StringLength(30)]
        public string ApprovalStatus { get; set; } = "Beklemede"; // "Beklemede", "Onaylandı", "Reddedildi"

        [Display(Name = "Onaylayan Kişi")]
        [StringLength(100)]
        public string? ApprovedBy { get; set; }

        [Display(Name = "Onay Tarihi")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "Talep Eden")]
        [StringLength(100)]
        public string? RequestedBy { get; set; }

        [Display(Name = "Talep Tarihi")]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
