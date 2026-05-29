using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class LookupValue
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tür")]
        public int LookupTypeId { get; set; }

        [ForeignKey("LookupTypeId")]
        public LookupType? LookupType { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Değer Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Opsiyonel Değer / Slug")]
        public string? Value { get; set; }

        [Display(Name = "Sıra No")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;
    }
}
