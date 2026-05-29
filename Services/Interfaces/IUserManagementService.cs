using DitibStasbourg.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserViewModel>> GetAllUsersAsync();
        Task<UserEditViewModel?> GetUserForEditAsync(string userId);
        Task<IdentityResult> UpdateUserAsync(UserEditViewModel model);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<IdentityResult> ResetUserPasswordAsync(string userId, string newPassword);
        Dictionary<string, List<string>> GetAllSystemClaims();
    }
}
