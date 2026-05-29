using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DitibStasbourg.Models.Security
{
    public class UserRoleTemplate
    {
        [Key]
        [Required]
        public string UserId { get; set; } = string.Empty; // IdentityUser Id

        public int RoleTemplateId { get; set; }

        [ForeignKey("RoleTemplateId")]
        public RoleTemplate? RoleTemplate { get; set; }
    }
}
