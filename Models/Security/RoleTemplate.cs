using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models.Security
{
    public class RoleTemplate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Şablon Adı")]
        public string Name { get; set; } = string.Empty;

        public ICollection<RoleTemplateClaim> Claims { get; set; } = new List<RoleTemplateClaim>();
    }
}
