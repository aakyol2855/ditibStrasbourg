using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class Gorevlendirme
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
    }
}
