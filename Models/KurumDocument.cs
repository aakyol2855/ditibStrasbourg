using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    /// <summary>
    /// Dernek/Cami'ye ait resmi evrakların dijital arşiv kaydı.
    /// Fransa hukuku gereği: Tüzük (Statut), Valilik Tescil (Prefevrak), SIRET/RNA yazışmaları vb.
    /// </summary>
    public class KurumDocument : ISoftDeletable
    {
        public int Id { get; set; }

        [ForeignKey("Kurum")]
        public int KurumId { get; set; }
        public Kurum Kurum { get; set; } = null!;

        [Required]
        [Display(Name = "Belge Adı")]
        [StringLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Belge Kategorisi")]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // "Tüzük", "Valilik Tescil", "SIRET/RNA", "Sözleşme", "Diğer"

        [Display(Name = "Açıklama / Notlar")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Dosya Yolu")]
        public string FilePath { get; set; } = string.Empty;

        [Display(Name = "Dosya Boyutu (KB)")]
        public long? FileSizeKb { get; set; }

        [Display(Name = "Geçerlilik Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        [Display(Name = "Yükleyen Kullanıcı")]
        [StringLength(100)]
        public string? UploadedBy { get; set; }

        [Display(Name = "Yükleme Tarihi")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
