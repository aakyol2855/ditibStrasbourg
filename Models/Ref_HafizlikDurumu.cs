using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class Ref_HafizlikDurumu
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Hafızlık Durumu")]
        public string Ad { get; set; } = string.Empty;

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;
    }
}
