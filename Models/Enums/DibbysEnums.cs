using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models.Enums
{
    public enum PasaportTuru
    {
        [Display(Name = "Yeşil (Hizmet)")] Yesil = 0,
        [Display(Name = "Gri (Hususi)")] Gri = 1,
        [Display(Name = "Bordo (Umuma Mahsus)")] Bordo = 2
    }

    public enum MedeniDurum
    {
        [Display(Name = "Bekâr")] Bekar = 0,
        [Display(Name = "Evli")] Evli = 1,
        [Display(Name = "Boşanmış")] Bosanmis = 2,
        [Display(Name = "Dul")] Dul = 3
    }

    public enum IzinTuru
    {
        [Display(Name = "Yıllık İzin")] YillikIzin = 0,
        [Display(Name = "Hastalık İzni")] HastalikIzni = 1,
        [Display(Name = "Doğum İzni")] DogumIzni = 2,
        [Display(Name = "Babalık İzni")] BabalikIzni = 3,
        [Display(Name = "Mazeret İzni")] MazeretIzni = 4
    }

    public enum OnayDurumu
    {
        [Display(Name = "Onay Bekliyor")] Beklemede = 0,
        [Display(Name = "Onaylandı")] Onaylandi = 1,
        [Display(Name = "Reddedildi")] Reddedildi = 2,
        [Display(Name = "İptal Edildi")] IptalEdildi = 3
    }

    public enum AllocationType
    {
        [Display(Name = "İmam Maaşı Desteği")] ImamMaasiDestegi = 0,
        [Display(Name = "Dernek Yardımı")] DernekYardimi = 1,
        [Display(Name = "Proje Fonu")] ProjeFonu = 2,
        [Display(Name = "Geçici Görev Ödeneği")] GeciciGorevOdenegi = 3
    }

    public enum KursTuru
    {
        [Display(Name = "Çocuk (4-6 Yaş)")] Cocuk_4_6_Yas = 0,
        [Display(Name = "Yetişkin Eğitimi")] YetiskinEgitimi = 1,
        [Display(Name = "Erkekler Akademi")] ErkeklerAkademi = 2,
        [Display(Name = "Kadınlar Kültür Seminerleri")] KadinlarKulturSeminerleri = 3
    }
}
