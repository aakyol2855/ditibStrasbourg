using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Attributes;
using DitibStasbourg.Models.Enums;

namespace DitibStasbourg.Models
{
    public class Gorevli : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad")]
        [ExportColumn("Ad", Order = 1)]
        public string Ad { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Soyad")]
        [ExportColumn("Soyad", Order = 2)]
        public string Soyad { get; set; } = string.Empty;

        [Display(Name = "E-posta")]
        [EmailAddress]
        [ExportColumn("E-posta", Order = 3)]
        public string? Email { get; set; }

        [Display(Name = "Ad Soyad")]
        public string AdSoyad => $"{Ad} {Soyad}";

        [Display(Name = "Cinsiyet")]
        [ExportColumn("Cinsiyet", Order = 4)]
        public string? Cinsiyet { get; set; } // 'E' or 'K'

        [Display(Name = "Görevli Durumu")]
        public int? GorevliDurumId { get; set; }

        [ForeignKey("GorevliDurumId")]
        public Ref_GorevliDurum? GorevliDurumBilgisi { get; set; }

        [Display(Name = "Sözleşme Tipi")]
        public int? SozlesmeTipId { get; set; }

        [ForeignKey("SozlesmeTipId")]
        public Ref_SozlesmeTip? SozlesmeTip { get; set; }

        // Identity
        [Display(Name = "TC Kimlik No")]
        [StringLength(11)]
        [ExportColumn("TC Kimlik No", Order = 5, IncludeInQuickExport = false)]
        public string? TCKimlikNo { get; set; }

        [Display(Name = "Fransız Kimlik / Oturum Kartı No (NSS / Titre de Séjour)")]
        [StringLength(50)]
        [ExportColumn("Fransız Kimlik No", Order = 6, IncludeInQuickExport = false)]
        public string? FrenchNationalId { get; set; }

        [Display(Name = "Sicil No")]
        [StringLength(20)]
        [ExportColumn("Sicil No", Order = 20, IncludeInQuickExport = false)]
        public string? SicilNo { get; set; }

        [Display(Name = "Baba Adı")]
        [ExportColumn("Baba Adı", Order = 10, IncludeInQuickExport = false)]
        public string? BabaAdi { get; set; }

        [Display(Name = "Anne Adı")]
        [ExportColumn("Anne Adı", Order = 11, IncludeInQuickExport = false)]
        public string? AnneAdi { get; set; }

        [Display(Name = "Doğum Yeri")]
        [ExportColumn("Doğum Yeri", Order = 12, IncludeInQuickExport = false)]
        public string? DogumYeri { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        [ExportColumn("Doğum Tarihi", Order = 13, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
        public DateTime? DogumTarihi { get; set; }

        [Display(Name = "Memleketi")]
        [StringLength(100)]
        [ExportColumn("Memleketi", Order = 21, IncludeInQuickExport = false)]
        public string? Memleketi { get; set; }

        [Display(Name = "Medeni Durum")]
        [ExportColumn("Medeni Durum", Order = 22, IncludeInQuickExport = false)]
        public MedeniDurum? EsDurumu { get; set; }

        // Contact
        [Display(Name = "Cep Telefonu")]
        [ExportColumn("Cep Telefonu", Order = 6)]
        public string? CepTelefonu { get; set; }

        [Display(Name = "Ev Telefonu")]
        [ExportColumn("Ev Telefonu", Order = 7, IncludeInQuickExport = false)]
        public string? EvTelefonu { get; set; }

        [Display(Name = "Adres")]
        [StringLength(500)]
        [ExportColumn("Adres", Order = 8, IncludeInQuickExport = false, FixedWidth = 40)]
        public string? Adres { get; set; }

        // Education
        [Display(Name = "Eğitim Durumu")]
        public int? EgitimDurumuId { get; set; }
        [ForeignKey("EgitimDurumuId")]
        public Ref_EgitimDurumu? EgitimDurumu { get; set; }

        [Display(Name = "Mezuniyet Okul")]
        [ExportColumn("Mezuniyet Okul", Order = 14, IncludeInQuickExport = false)]
        public string? MezuniyetOkul { get; set; }

        [Display(Name = "Mezuniyet Bölüm")]
        [ExportColumn("Mezuniyet Bölüm", Order = 15, IncludeInQuickExport = false)]
        public string? MezuniyetBolum { get; set; }

        [Display(Name = "Mezun Olunan Üniversite")]
        [StringLength(200)]
        [ExportColumn("Üniversite", Order = 33, IncludeInQuickExport = false)]
        public string? Universite { get; set; }

        [Display(Name = "Not Ortalaması (AGNO)")]
        [Range(0, 4)]
        public decimal? Agno { get; set; }

        [Display(Name = "Bilinen Diller")]
        [StringLength(300)]
        [ExportColumn("Diller", Order = 34, IncludeInQuickExport = false)]
        public string? Diller { get; set; }

        [Display(Name = "Hafızlık Durumu")]
        public int? HafizlikDurumuId { get; set; }
        [ForeignKey("HafizlikDurumuId")]
        public Ref_HafizlikDurumu? HafizlikDurumu { get; set; }

        // Employment
        [Display(Name = "Ünvan")]
        public int? UnvanId { get; set; }
        [ForeignKey("UnvanId")]
        public Ref_Unvan? Unvan { get; set; }

        [Display(Name = "Kadro Türü")]
        public int? KadroTuruId { get; set; }
        [ForeignKey("KadroTuruId")]
        public Ref_KadroTuru? KadroTuru { get; set; }

        [Display(Name = "Askerlik Durumu")]
        public int? AskerlikDurumuId { get; set; }
        [ForeignKey("AskerlikDurumuId")]
        public Ref_AskerlikDurumu? AskerlikDurumu { get; set; }

        [Display(Name = "Kan Grubu")]
        public int? KanGrubuId { get; set; }
        [ForeignKey("KanGrubuId")]
        public Ref_KanGrubu? KanGrubu { get; set; }

        // Career
        [Display(Name = "Derece")]
        [ExportColumn("Derece", Order = 16, IncludeInQuickExport = false)]
        public string? Derece { get; set; }

        [Display(Name = "Kademe")]
        [ExportColumn("Kademe", Order = 17, IncludeInQuickExport = false)]
        public string? Kademe { get; set; }

        [Display(Name = "İlk Göreve Başlama Tarihi")]
        [DataType(DataType.Date)]
        [ExportColumn("İlk Göreve Başlama", Order = 9, Format = "dd.MM.yyyy")]
        public DateTime? IlkGoreveBaslamaTarihi { get; set; }

        [Display(Name = "Emeklilik Tarihi")]
        [DataType(DataType.Date)]
        [ExportColumn("Emeklilik Tarihi", Order = 18, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
        public DateTime? EmeklilikTarihi { get; set; }

        [Display(Name = "Diyanet Giriş Tarihi")]
        [DataType(DataType.Date)]
        [ExportColumn("Diyanet Giriş Tarihi", Order = 19, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
        public DateTime? DiyanetGirisTarihi { get; set; }

        [Display(Name = "Fransa Giriş / Sözleşme Tarihi")]
        [DataType(DataType.Date)]
        [ExportColumn("Fransa Giriş Tarihi", Order = 20, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
        public DateTime? FransaGirisTarihi { get; set; }

        // DİBBYS Pasaport & Vize
        [Display(Name = "Pasaport Türü")]
        [ExportColumn("Pasaport Türü", Order = 23, IncludeInQuickExport = false)]
        public PasaportTuru? PasaportTuru { get; set; }

        [Display(Name = "Pasaport No")]
        [StringLength(20)]
        [ExportColumn("Pasaport No", Order = 24, IncludeInQuickExport = false)]
        public string? PasaportNo { get; set; }

        [Display(Name = "Görev Uzatma Bitiş Tarihi")]
        [DataType(DataType.Date)]
        [ExportColumn("Görev Uzatma Bitiş", Order = 25, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
        public DateTime? GorevUzatmaBitisTarihi { get; set; }

        [Display(Name = "Vize Bitiş Tarihi")]
        [DataType(DataType.Date)]
        [ExportColumn("Vize Bitiş Tarihi", Order = 26, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
        public DateTime? VisaExpirationDate { get; set; }

        [Display(Name = "Pasaport Geçerlilik Tarihi")]
        [DataType(DataType.Date)]
        [ExportColumn("Pasaport Geçerlilik Tarihi", Order = 27, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
        public DateTime? PassportExpirationDate { get; set; }

        [Display(Name = "Fransa Oturum Kartı Bitiş Tarihi")]
        [DataType(DataType.Date)]
        [ExportColumn("Fransa Oturum Kartı Bitiş", Order = 28, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
        public DateTime? ResidencePermitExpirationDate { get; set; }

        [Display(Name = "Eğitim Kurs Belgeleri")]
        [StringLength(2000)]
        public string? EgitimKursBelgeleri { get; set; }

        // Linked Identity User (for GorevliUser portal login)
        [Display(Name = "Bağlı Kullanıcı ID")]
        public string? LinkedUserId { get; set; }

        // Other
        [Display(Name = "Fotoğraf Yolu")]
        public string? FotografYolu { get; set; }

        [Display(Name = "Başvuru Türü")]
        public int? BasvuruTuruId { get; set; }
        [ForeignKey("BasvuruTuruId")]
        public LookupValue? BasvuruTuru { get; set; }

        // Navigation property for assignments
        public ICollection<Gorevlendirme> Gorevlendirmeler { get; set; } = new List<Gorevlendirme>();

        public ICollection<GorevGecmisi> GorevGecmisleri { get; set; } = new List<GorevGecmisi>();
        public ICollection<GorevliNot> GorevliNotlari { get; set; } = new List<GorevliNot>();

        // DİBBYS Navigation Collections
        public ICollection<GorevliIzin> Izinler { get; set; } = new List<GorevliIzin>();
        public ICollection<GorevliFaaliyetRaporu> FaaliyetRaporlari { get; set; } = new List<GorevliFaaliyetRaporu>();

        // Belge Arşivi (Oturum Kartı, Laiklik Belgesi, Dil Belgesi vb.)
        public ICollection<GorevliBelge> Belgeler { get; set; } = new List<GorevliBelge>();

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(Name = "Görevde mi?")]
        public bool isActive => Gorevlendirmeler.Any(g => 
                      DateTime.Now.Date >= g.Tarih.Date && 
                      (!g.BitisTarihi.HasValue || DateTime.Now.Date <= g.BitisTarihi.Value.Date));

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool RequiresImmigrationAttention => 
            (VisaExpirationDate.HasValue && VisaExpirationDate.Value <= DateTime.Now.AddMonths(3)) ||
            (PassportExpirationDate.HasValue && PassportExpirationDate.Value <= DateTime.Now.AddMonths(3)) ||
            (ResidencePermitExpirationDate.HasValue && ResidencePermitExpirationDate.Value <= DateTime.Now.AddMonths(3));
        
        [Display(Name = "Merkez Personeli mi?")]
        public bool IsMerkezPersoneli { get; set; } = false;

        [Display(Name = "Merkez Görev Alanı")]
        [StringLength(100)]
        public string? MerkezGorevAlani { get; set; }

        [Display(Name = "Eski Durum (Deprecated)")]
        public GorevliDurum Durum { get; set; } = GorevliDurum.Notr;

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        [NotMapped]
        [ExportColumn("Aktif Görev Yeri", Order = 29, IncludeInQuickExport = false)]
        public string? ExportAktifGorevYeri { get; set; }

        [NotMapped]
        [ExportColumn("Önceki Görev Yerleri", Order = 30, IncludeInQuickExport = false)]
        public string? ExportOncekiGorevYerleri { get; set; }

        [NotMapped]
        [ExportColumn("Mevcut İzin Bilgileri", Order = 31, IncludeInQuickExport = false)]
        public string? ExportMevcutIzinBilgileri { get; set; }

        [NotMapped]
        [ExportColumn("Sistem Notları", Order = 32, IncludeInQuickExport = false)]
        public string? ExportSistemNotlari { get; set; }
    }

    public enum GorevliDurum
    {
        [Display(Name = "Nötr (Gri)")]
        Notr = 0,

        [Display(Name = "Memnun Kalındı (Yeşil)")]
        Yesil = 1,
        
        [Display(Name = "Orta Karar (Turuncu)")]
        Turuncu = 2,
        
        [Display(Name = "Memnun Kalınmadı (Kırmızı)")]
        Kirmizi = 3
    }
}