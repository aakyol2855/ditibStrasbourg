using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class DernekUye
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Display(Name = "İletişim")]
        public string? Iletisim { get; set; }

        [Display(Name = "Aile Üye Sayısı")]
        public int AileUyeSayisi { get; set; } = 1;

        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.Date)]
        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        [Required]
        public int KurumId { get; set; }

        [ForeignKey("KurumId")]
        public Kurum? Kurum { get; set; }
    }
}
