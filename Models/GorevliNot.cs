using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DitibStasbourg.Models
{
    public class GorevliNot
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Görevli")]
        public int GorevliId { get; set; }

        [ForeignKey("GorevliId")]
        public Gorevli? Gorevli { get; set; }

        [Required]
        [Display(Name = "Not İçeriği")]
        [StringLength(1000)]
        public string NotIcerik { get; set; } = string.Empty;

        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;

        [Display(Name = "Yazan Kişi")]
        public string? YazanKisiId { get; set; }

        [ForeignKey("YazanKisiId")]
        public IdentityUser? YazanKisi { get; set; }
    }
}
