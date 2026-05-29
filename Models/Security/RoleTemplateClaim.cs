using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models.Security
{
    public class RoleTemplateClaim
    {
        public int Id { get; set; }

        public int RoleTemplateId { get; set; }

        [ForeignKey("RoleTemplateId")]
        public RoleTemplate? RoleTemplate { get; set; }

        [Required]
        [StringLength(100)]
        public string ClaimValue { get; set; } = string.Empty; // e.g. "Gorevli-Create"
    }
}
