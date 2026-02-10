using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public enum KurumTip
    {
        Cami,
        Dernek
    }

    public class Kurum
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "İsim")]
        public string Isim { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Required]
        [Display(Name = "Tip")]
        public KurumTip Tip { get; set; }

        // Navigation property for assignments
        public ICollection<Gorevlendirme> Gorevlendirmeler { get; set; } = new List<Gorevlendirme>();
    }
}
