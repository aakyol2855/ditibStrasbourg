using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class Gorevlendirme : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Görevli")]
        public int GorevliId { get; set; }

        [ForeignKey("GorevliId")]
        public Gorevli? Gorevli { get; set; }

        [Required]
        [Display(Name = "Kurum")]
        public int KurumId { get; set; }

        [ForeignKey("KurumId")]
        public Kurum? Kurum { get; set; }

        [Required]
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BitisTarihi { get; set; }

        // Replacement Planning
        [Display(Name = "Yerine Gelecek Görevli")]
        public int? YerineGelecekGorevliId { get; set; }

        [ForeignKey("YerineGelecekGorevliId")]
        public Gorevli? YerineGelecekGorevli { get; set; }

        [Display(Name = "Yerine Geliş Planlanan Tarih")]
        [DataType(DataType.Date)]
        public DateTime? YerineGelisPlanlananTarih { get; set; }

        [Display(Name = "Yerine Gelen Görev Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? YerineGelisPlanlananBitisTarih { get; set; }

        // Navigation
        public ICollection<GorevlendirmeNot>? GorevlendirmeNotlari { get; set; }

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}
