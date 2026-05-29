using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class HelpTopic
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "General"; // Genel, Personel, Görevlendirme, Sistem Yönetimi

        [StringLength(100)]
        public string? RequiredClaim { get; set; } // Yetki kontrolü için

        [StringLength(255)]
        public string? ImageUrl { get; set; } // Görsel açıklama için

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
