using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DitibStasbourg.Models.Attributes;

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

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(Name = "Görevde mi?")]
        public bool isActive => Gorevlendirmeler.Any(g => 
                      DateTime.Now.Date >= g.Tarih.Date && 
                      (!g.BitisTarihi.HasValue || DateTime.Now.Date <= g.BitisTarihi.Value.Date));
        
        [Display(Name = "Eski Durum (Deprecated)")]
        public GorevliDurum Durum { get; set; } = GorevliDurum.Notr;

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
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