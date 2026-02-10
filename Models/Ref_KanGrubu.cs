using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class Ref_KanGrubu
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kan Grubu")]
        public string Ad { get; set; } = string.Empty;

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;
    }
}
