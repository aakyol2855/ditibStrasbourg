using System.Collections.Generic;

namespace DitibStasbourg.Models.ViewModels
{
    public class TrashBinViewModel
    {
        public List<Kurum> DeletedAssociations { get; set; } = new();
        public List<Gorevli> DeletedPersonnel { get; set; } = new();
        public List<Gorevlendirme> DeletedAssignments { get; set; } = new();
        public int SoftDeleteRetentionDays { get; set; } = 30;
    }
}
