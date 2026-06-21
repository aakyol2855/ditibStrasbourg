using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Attributes;

namespace DitibStasbourg.Models
{
    public enum CampaignType
    {
        Fitre,
        Zekat,
        Fidye,
        GenelNakit
    }

    public class KurumFinansalDonem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kurum")]
        public int KurumId { get; set; }

        [ForeignKey("KurumId")]
        public virtual Kurum? Kurum { get; set; }

        [Required]
        [Display(Name = "Yıl")]
        [ExportColumn("Yıl", Order = 1)]
        public int Year { get; set; }

        [Required]
        [Display(Name = "Kampanya Türü")]
        [ExportColumn("Kampanya Türü", Order = 2)]
        public CampaignType CampaignType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Toplanan Tutar")]
        [ExportColumn("Toplanan Tutar", Order = 3, Format = "C2")]
        public decimal CollectedAmount { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Para Birimi")]
        [ExportColumn("Para Birimi", Order = 4)]
        public string Currency { get; set; } = "EUR";

        [StringLength(500)]
        [Display(Name = "Dahili Notlar")]
        [ExportColumn("Dahili Notlar", Order = 5)]
        public string? InternalNotes { get; set; }
    }
}
