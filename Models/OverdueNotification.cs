using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    /// <summary>
    /// Gecikmiş ödeme/dönem ve belge süresi gibi kritik olaylar için
    /// sistem bildirimi modeli. Dashboard ve e-posta entegrasyonu için kullanılır.
    /// </summary>
    public class OverdueNotification
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Bildirim Türü")]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty;
        // "OdemeGecikme", "VizeSuresi", "PasaportSuresi", "OturumIzni", "BelgeSuresi", "SeçimTakvimi"

        [Required]
        [Display(Name = "Başlık")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Detay Mesajı")]
        [StringLength(1000)]
        public string? Message { get; set; }

        [Display(Name = "İlgili Kurum")]
        public int? RelatedKurumId { get; set; }
        [ForeignKey("RelatedKurumId")]
        public Kurum? RelatedKurum { get; set; }

        [Display(Name = "İlgili Görevli")]
        public int? RelatedGorevliId { get; set; }
        [ForeignKey("RelatedGorevliId")]
        public Gorevli? RelatedGorevli { get; set; }

        [Display(Name = "İlgili Bütçe Dönemi")]
        public int? RelatedBudgetPeriodId { get; set; }
        [ForeignKey("RelatedBudgetPeriodId")]
        public KurumButcePeriod? RelatedBudgetPeriod { get; set; }

        public int? KurumButcePeriodId { get; set; }
        [ForeignKey("KurumButcePeriodId")]
        public KurumButcePeriod? KurumButcePeriod { get; set; }

        [Display(Name = "Çözüldü mü?")]
        public bool IsResolved { get; set; } = false;

        [Display(Name = "Çözülme Tarihi")]
        public DateTime? ResolvedAt { get; set; }

        [Display(Name = "Çözülme Notu")]
        [StringLength(500)]
        public string? ResolutionNotes { get; set; }

        [Display(Name = "Aciliyet Seviyesi")]
        [StringLength(20)]
        public string Severity { get; set; } = "Warning"; // "Info", "Warning", "Critical"

        [Display(Name = "Okundu mu?")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "Okunma Tarihi")]
        public DateTime? ReadAt { get; set; }

        [Display(Name = "Hedef Alıcı E-posta")]
        [StringLength(200)]
        public string? TargetEmail { get; set; }

        [Display(Name = "E-posta Gönderildi mi?")]
        public bool IsEmailSent { get; set; } = false;

        [Display(Name = "E-posta Gönderim Tarihi")]
        public DateTime? EmailSentAt { get; set; }

        [Display(Name = "Son Kullanma / Vade Tarihi")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "İlgili Dernek Notu")]
        public int? RelatedDernekNotId { get; set; }
        [ForeignKey("RelatedDernekNotId")]
        public DernekNot? RelatedDernekNot { get; set; }

        [Display(Name = "İlgili Görevli Belgesi")]
        public int? RelatedGorevliBelgeId { get; set; }
        [ForeignKey("RelatedGorevliBelgeId")]
        public GorevliBelge? RelatedGorevliBelge { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
