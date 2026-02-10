using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class Ref_SozlesmeTip
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Sözleşme Tipi")]
        public string Ad { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
    }
}
