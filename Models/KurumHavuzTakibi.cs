using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class KurumHavuzTakibi : ISoftDeletable
    {
        public int Id { get; set; }

        [ForeignKey("Kurum")]
        public int KurumId { get; set; }
        public Kurum Kurum { get; set; } = null!;

        public int Yil { get; set; }

        public PersonnelGender PersonnelGender { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VariableAmount { get; set; }

        public DateTime? PaymentDate { get; set; }
        public bool IsSettled { get; set; }
        public string? InternalNotes { get; set; }

        // Soft‑delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
