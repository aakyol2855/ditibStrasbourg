using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class KurbanCampaignRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Bolge { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Cami { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string FysSorumlusu { get; set; } = null!;

        // Diğer Ülkeler
        public int DigerAdet { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DigerMiktar { get; set; }

        // Türkiye
        public int TrAdet { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TrMiktar { get; set; }

        // Ödeme Şekilleri
        [Column(TypeName = "decimal(18,2)")]
        public decimal Havale { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cek { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Nakit { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Stripe { get; set; } // Online
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cihaz { get; set; } // Cihaz

        [Column(TypeName = "decimal(18,2)")]
        public decimal ToplamOdenen { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal KalanBakiye { get; set; }

        [StringLength(100)]
        public string? TutanakNo { get; set; }

        public int Yil { get; set; } // e.g. 2026, 2025, 2024 for Multi-Year Comparison

        public int? KurumId { get; set; }

        [ForeignKey("KurumId")]
        public virtual Kurum? Kurum { get; set; }

        public bool IsApproved { get; set; } = false;
    }
}
