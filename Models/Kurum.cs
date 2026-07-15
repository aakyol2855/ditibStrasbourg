using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Attributes;

namespace DitibStasbourg.Models
{
    public enum KurumTip
    {
        Cami,
        Dernek
    }

    public class Kurum : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "İsim")]
        [ExportColumn("Dernek / Kurum Adı", Order = 1)]
        public string Isim { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        [ExportColumn("Adres", Order = 8, FixedWidth = 40, IncludeInQuickExport = false)]
        public string? Adres { get; set; }

        [Required]
        [Display(Name = "Tip")]
        [ExportColumn("Kurum Tipi", Order = 9, IncludeInQuickExport = false)]
        public KurumTip Tip { get; set; }

        [Display(Name = "Üst Kurum")]
        public int? UstKurumId { get; set; }

        [ForeignKey("UstKurumId")]
        public Ref_KurumTuru? UstKurum { get; set; }

        [Display(Name = "Şehir")]
        [ExportColumn("Şehir", Order = 3)]
        public string? Sehir { get; set; }

        [Display(Name = "Aktif mi?")]
        [ExportColumn("Aktif mi?", Order = 10)]
        public bool AktifMi { get; set; } = true;

        [Display(Name = "Başkonsolosluk Bölgesi")]
        [ExportColumn("Başkonsolosluk Bölgesi", Order = 2)]
        public string? BaskonsoloslukBolgesi { get; set; }

        [Display(Name = "CRM Üyelik Form Durumu")]
        [ExportColumn("CRM Form Durumu", Order = 7, IncludeInQuickExport = false)]
        public string? CrmUyelikFormDurumu { get; set; }

        [Display(Name = "Kuruluş Kanunu")]
        [ExportColumn("Kuruluş Kanunu", Order = 6, IncludeInQuickExport = false)]
        public string? KurulusKanunu { get; set; }

        [Display(Name = "Bölge")]
        [ExportColumn("Bölge", Order = 4)]
        public string? Bolge { get; set; }

        [Display(Name = "Dernek Başkanı Adı")]
        [ExportColumn("Başkan Adı", Order = 5)]
        public string? DernekBaskaniAd { get; set; }

        [Display(Name = "Dernek Başkanı İletişim")]
        [ExportColumn("Başkan İletişim", Order = 11)]
        public string? DernekBaskaniIletisim { get; set; }

        [Display(Name = "Din Görevlisi Adı")]
        [ExportColumn("Din Görevlisi", Order = 12)]
        public string? DinGorevlisiAd { get; set; }

        [Display(Name = "Din Görevlisi İletişim")]
        [ExportColumn("Din Görevlisi İletişim", Order = 13, IncludeInQuickExport = false)]
        public string? DinGorevlisiIletisim { get; set; }

        [Display(Name = "İletişim Numarası")]
        [ExportColumn("İletişim Numarası", Order = 14)]
        public string? IletisimNumarasi { get; set; }

        [Display(Name = "Maili")]
        [ExportColumn("Maili", Order = 15)]
        public string? Maili { get; set; }

        [Display(Name = "Başkan Mail")]
        [ExportColumn("Başkan Mail", Order = 16)]
        public string? BaskanMail { get; set; }

        [Display(Name = "Enlem (Latitude)")]
        public double? Latitude { get; set; }

        [Display(Name = "Boylam (Longitude)")]
        public double? Longitude { get; set; }

        [Display(Name = "Cemaat Sayısı")]
        [ExportColumn("Cemaat Sayısı", Order = 17, IncludeInQuickExport = false)]
        public int? CemaatCount { get; set; }

        [Display(Name = "Resmi Fransızca Adı")]
        [ExportColumn("Resmi Fransızca Adı", Order = 18, IncludeInQuickExport = false)]
        public string? FrenchRegistrationName { get; set; }

        // Navigation property for assignments
        public ICollection<Gorevlendirme> Gorevlendirmeler { get; set; } = new List<Gorevlendirme>();
        
        // Navigation for Members
        public ICollection<DernekUye> DernekUyeleri { get; set; } = new List<DernekUye>();

        // Navigation for Financial Periods
        public ICollection<KurumFinansalDonem> FinansalDonemler { get; set; } = new List<KurumFinansalDonem>();

        // Navigation for Management Board Members
        public ICollection<KurumYonetimKuruluUyesi> YonetimKuruluUyeleri { get; set; } = new List<KurumYonetimKuruluUyesi>();

        // Financial metadata
        [Required]
        [Display(Name = "IBAN No")]
        public string IbanNo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "SIRET No")]
        public string SiretNo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "RNA No")]
        public string RnaNo { get; set; } = string.Empty;

        [Display(Name = "Ekonomi Notu")]
        public string? EkonomiNotu { get; set; }

        // ── Tesis Bilgileri ──────────────────────────────────────────
        [Display(Name = "Cami var mı?")]
        public bool HasCami { get; set; } = false;

        [Display(Name = "Lojman var mı?")]
        public bool HasLojman { get; set; } = false;

        [Display(Name = "Lojman Kapasitesi")]
        public int? LojmanKapasite { get; set; }

        [Display(Name = "Müştemilat var mı?")]
        public bool HasMustemilat { get; set; } = false;

        [Display(Name = "Müştemilat Kapasitesi")]
        public int? MustemilatKapasite { get; set; }

        // Navigation to financial entities
        public virtual ICollection<KurumButce> Butceler { get; set; } = new List<KurumButce>();
        public virtual ICollection<KurumHavuzTakibi> HavuzTakibi { get; set; } = new List<KurumHavuzTakibi>();

        // Document Management System (DMS) - Resmi Evrak Arşivi
        public virtual ICollection<KurumDocument> Documents { get; set; } = new List<KurumDocument>();

        // Dernek Notları (tarihli, soft-deletable)
        public virtual ICollection<DernekNot> DernekNotlari { get; set; } = new List<DernekNot>();

        // Dernek Görselleri (cami, lojman, müştemilat fotoğrafları)
        public virtual ICollection<DernekGorsel> DernekGorselleri { get; set; } = new List<DernekGorsel>();

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
