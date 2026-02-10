using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models.ViewModels
{
    public class GorevliFilterViewModel
    {
        public string? SearchString { get; set; }
        
        [Display(Name = "Görevliler")]
        public List<int>? StaffIds { get; set; } // Multi-select for autocomplete staff search
        
        [Display(Name = "Görevli Durumu")]
        public List<int>? GorevliDurumIds { get; set; } // Multi-select
        
        [Display(Name = "Sözleşme Tipi")]
        public int? SozlesmeTipId { get; set; }
        
        [Display(Name = "Kurum / Dernek")]
        public int? KurumId { get; set; }
        
        [Display(Name = "Şehir")]
        public string? Sehir { get; set; }
        
        [Display(Name = "Cinsiyet")]
        public string? Cinsiyet { get; set; }
        
        [Display(Name = "Aktiflik Durumu")]
        public bool? IsActive { get; set; }
        
        [Display(Name = "Emekli Mi?")]
        public bool? IsRetired { get; set; } // "Emekli" might be a status or separate flag. Assuming status for now but keeping flag if needed.
        
        [DataType(DataType.Date)]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? TarihBaslangic { get; set; }
        
        [DataType(DataType.Date)]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime? TarihBitis { get; set; }

        // New Filters
        [Display(Name = "Ünvan")]
        public List<int>? UnvanIds { get; set; }

        [Display(Name = "Eğitim Durumu")]
        public List<int>? EgitimDurumuIds { get; set; }

        [Display(Name = "Hafızlık Durumu")]
        public List<int>? HafizlikDurumuIds { get; set; }

        [Display(Name = "Kadro Türü")]
        public List<int>? KadroTuruIds { get; set; }

        [Display(Name = "Askerlik Durumu")]
        public List<int>? AskerlikDurumuIds { get; set; }

        [Display(Name = "Kan Grubu")]
        public List<int>? KanGrubuIds { get; set; }

        // Identity Filters
        public string? BabaAdi { get; set; }
        public string? AnneAdi { get; set; }
        public string? DogumYeri { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DogumTarihiBaslangic { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DogumTarihiBitis { get; set; }

        // Contact
        public string? CepTelefonu { get; set; }

        // Pagination & Sorting
        public string? SortOrder { get; set; }
        public int? PageNumber { get; set; }
    }
}
