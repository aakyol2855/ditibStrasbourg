namespace DitibStasbourg.Models.ViewModels
{
    public class KurumListViewModel
    {
        public int Id { get; set; }
        public string Isim { get; set; }
        public string Adres { get; set; }
        public string Tip { get; set; }
        public string? UstKurumAd { get; set; }
        public int AktifGorevliSayisi { get; set; }
        public int ToplamGorevliSayisi { get; set; } // Distinct staff count history
        public string? Sehir { get; set; }
        public bool AktifMi { get; set; }
    }
}
