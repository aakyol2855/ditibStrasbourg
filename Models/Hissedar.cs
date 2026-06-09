using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Attributes;

namespace DitibStasbourg.Models
{
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

        public int? KurbanlikId { get; set; }
        
        [ForeignKey("KurbanlikId")]
        public virtual Kurbanlik? Kurbanlik { get; set; }

        [ExportColumn("Kayıt Tarihi", Order = 5, Format = "dd.MM.yyyy HH:mm")]
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
