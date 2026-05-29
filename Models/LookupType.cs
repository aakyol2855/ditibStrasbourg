using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class LookupType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tür Adı")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Kod")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        public ICollection<LookupValue> Values { get; set; } = new List<LookupValue>();
    }
}
