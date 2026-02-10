using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class Ref_EgitimDurumu
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Eğitim Durumu")]
        public string Ad { get; set; } = string.Empty;

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;
    }
}
