using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class KurumYonetimKuruluUyesi
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kurum")]
        public int KurumId { get; set; }

        [ForeignKey("KurumId")]
        public Kurum? Kurum { get; set; }

        [Required]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "İletişim Telefonu")]
        public string? ContactPhone { get; set; }

        [Required]
        [Display(Name = "Yönetim Rolü")]
        public int YonetimRolId { get; set; }

        [ForeignKey("YonetimRolId")]
        public Ref_YonetimRol? YonetimRol { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
