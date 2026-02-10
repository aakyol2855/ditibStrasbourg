using System.ComponentModel.DataAnnotations;

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

        // Navigation property for assignments
        public ICollection<Gorevlendirme> Gorevlendirmeler { get; set; } = new List<Gorevlendirme>();

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(Name = "Görevde mi?")]
        public bool isActive => Gorevlendirmeler.Any(g => 
                      DateTime.Now.Date >= g.Tarih.Date && 
                      (!g.BitisTarihi.HasValue || DateTime.Now.Date <= g.BitisTarihi.Value.Date));
        
        [Display(Name = "Durum")]
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