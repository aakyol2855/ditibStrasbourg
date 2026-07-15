using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class KurumButcePeriod
    {
        public int Id { get; set; }
        [ForeignKey("KurumButce")]
        public int KurumButceId { get; set; }
        public KurumButce KurumButce { get; set; } = null!;

        [Range(1,4)]
        public int PeriodNumber { get; set; }

        public DateTime TargetDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ScheduledAmount { get; set; }

        public bool IsPaid { get; set; } = false;
        public DateTime? PaymentDate { get; set; }
        public string? TransactionTutanakNo { get; set; }
        public string? AttachmentPath { get; set; }
        public string? PaidFromSource { get; set; }
    }
}
