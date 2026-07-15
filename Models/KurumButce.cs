using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models
{
    public class KurumButce
    {
        public int Id { get; set; }
        [ForeignKey("Kurum")] 
        public int KurumId { get; set; }
        public Kurum Kurum { get; set; } = null!;
        public int Yil { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBudget { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DitibContribution { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DernekContribution { get; set; }
        public virtual ICollection<KurumButcePeriod> Periods { get; set; } = new List<KurumButcePeriod>();
        public virtual ICollection<BudgetRevision> Revisions { get; set; } = new List<BudgetRevision>();
    }
}
