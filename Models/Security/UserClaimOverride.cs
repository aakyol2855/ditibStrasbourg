using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models.Security
{
    public class UserClaimOverride
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // IdentityUser Id

        [Required]
        [StringLength(100)]
        public string ClaimValue { get; set; } = string.Empty;

        // false = ADD (User gets this claim even if template doesn't have it)
        // true = REMOVE (User loses this claim even if template has it)
        public bool IsDenied { get; set; } 
    }
}
