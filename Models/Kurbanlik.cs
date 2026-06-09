using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Attributes;

namespace DitibStasbourg.Models
{
    public class Kurbanlik
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [ExportColumn("Küpe No", Order = 1)]
        public string TagNumber { get; set; }

        [Required]
        [ExportColumn("Tür", Order = 2)]
        public string Species { get; set; }

        [ExportColumn("Tahmini Kilo (kg)", Order = 3, Format = "N2")]
        public decimal Weight { get; set; }

        [ExportColumn("Alış Fiyatı (€)", Order = 4, Format = "N2")]
        public decimal Price { get; set; }

        [ExportColumn("Toplam Hisse", Order = 5)]
        public int TotalShares { get; set; } = 7;

        [ExportColumn("Kalan Hisse", Order = 6)]
        public int RemainingShares { get; set; }

        [ExportColumn("Durum", Order = 7)]
        public string Status { get; set; } = "Available";

        [ExportColumn("Oluşturulma Tarihi", Order = 8, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Hissedar> Hissedarlar { get; set; } = new List<Hissedar>();
    }
}
