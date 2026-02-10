using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class Gorevli
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Display(Name = "E-posta")]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Ad Soyad")]
        public string AdSoyad => $"{Ad} {Soyad}";

        [Display(Name = "Cinsiyet")]
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
        public string? TCKimlikNo { get; set; }

        [Display(Name = "Baba Adı")]
        public string? BabaAdi { get; set; }

        [Display(Name = "Anne Adı")]
        public string? AnneAdi { get; set; }

        [Display(Name = "Doğum Yeri")]
        public string? DogumYeri { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        // Contact
        [Display(Name = "Cep Telefonu")]
        public string? CepTelefonu { get; set; }

        [Display(Name = "Ev Telefonu")]
        public string? EvTelefonu { get; set; }

        [Display(Name = "Adres")]
        [StringLength(500)]
        public string? Adres { get; set; }

        // Education
        [Display(Name = "Eğitim Durumu")]
        public int? EgitimDurumuId { get; set; }
        [ForeignKey("EgitimDurumuId")]
        public Ref_EgitimDurumu? EgitimDurumu { get; set; }

        [Display(Name = "Mezuniyet Okul")]
        public string? MezuniyetOkul { get; set; }

        [Display(Name = "Mezuniyet Bölüm")]
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
        public string? Derece { get; set; }

        [Display(Name = "Kademe")]
        public string? Kademe { get; set; }

        [Display(Name = "İlk Göreve Başlama Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? IlkGoreveBaslamaTarihi { get; set; }

        [Display(Name = "Emeklilik Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? EmeklilikTarihi { get; set; }

        [Display(Name = "Diyanet Giriş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DiyanetGirisTarihi { get; set; }

        // Other
        [Display(Name = "Fotoğraf Yolu")]
        public string? FotografYolu { get; set; }

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
/*todo : 
Görevliler sekmesinde hangi görevliler aktif , hangileri gelmiş girmiş belli değil. Ayrıca durum konusunda bir kafa karışıklığı var.
durum kısmı görevlinin aktif durumu değil , yönetici için daha sonrası için bir bilgilendirme kısmı. yani bu kişiyi bir daha görevlendirelim mi sorusu için
referans olması açısından orada duruyor. Bu bağlamda kişinin kırmızı turuncu ve yeşil olarak renklendirilen durum statüsü , sadece görevlendirmesi bitmiş(bitiş tarihi geçmiş)
ya da özel durumlar için elle değiştirilecek olamlı. yeni görevli eklendiğinde casual olarak nötr gelmeli(gri renkli). İsActive kısmı ise eğer görevli hala başlangıç ve bitiş 
tarihleri arasında ise active olmalı. Değilse bool değer otomatik olarak false olmalı ve bunu da view'da göstermeliyiz. Ayrıca Görevliler sekmesinde ad-soyad , son görev yeri
başlangıç , bitiş tarihi yanında ; isactive alanını gösteren Görevde mi? alanı bulunmalı.

*/