using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class AppSetting
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; } = "";
        
        [Required]
        [MaxLength(500)]
        public string Value { get; set; } = "";
    }
}
