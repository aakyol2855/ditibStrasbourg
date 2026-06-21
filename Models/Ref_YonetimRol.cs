using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class Ref_YonetimRol
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Yönetim Kurulu Rolü")]
        public string Ad { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
    }
}
