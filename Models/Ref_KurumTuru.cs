using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class Ref_KurumTuru
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kurum Türü / Üst Kurum")]
        public string Ad { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
    }
}
