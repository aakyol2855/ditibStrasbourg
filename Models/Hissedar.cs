using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class Hissedar
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        public bool IsVekaletTaken { get; set; } = false; // Vekalet Alındı mı?

        public string PaymentStatus { get; set; } = "Pending"; // Ödendi, Bekliyor - From Lookup

        public int? KurbanlikId { get; set; }
        
        [ForeignKey("KurbanlikId")]
        public virtual Kurbanlik? Kurbanlik { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
