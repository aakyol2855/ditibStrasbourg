using Microsoft.AspNetCore.Identity;

namespace DitibStasbourg.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class UserEditViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsAdmin { get; set; }
    }
}
