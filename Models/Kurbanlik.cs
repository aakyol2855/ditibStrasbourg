using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class Kurbanlik
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TagNumber { get; set; } // Küpe No

        [Required]
        public string Species { get; set; } // Tür (Büyükbaş, Küçükbaş) - From Lookup

        public decimal Weight { get; set; } // Tahmini Kilo

        public decimal Price { get; set; } // Alış Fiyatı

        public int TotalShares { get; set; } = 7; // Toplam Hisse (Büyükbaş için 7)

        public int RemainingShares { get; set; } // Kalan Hisse

        public string Status { get; set; } = "Available"; // Satışta, Dolu, Kesildi

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Hissedar> Hissedarlar { get; set; } = new List<Hissedar>();
    }
}
