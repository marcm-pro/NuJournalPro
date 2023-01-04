using Microsoft.AspNetCore.Identity;
using NuJournalPro.Models;
using NuJournalPro.Models.Identity;

namespace NuJournalPro.Services.Identity.Interfaces
{
    public interface IUserService
    {
        Task<NuJournalUser> CreateNewUserAsync(UserInputModel userModel, UserInfo? parentUserInfo = null, IFormFile? newUserAvatarFile = null);
        Task<CompressedImage> GetAccessDeniedImage();
        Task<UserInfo> GetUserInfoAsync(NuJournalUser user);
        Task<CompressedImage> GetDefaultUserAvatar();
        bool IsAdmin(NuJournalUser user);
        bool IsOwner(NuJournalUser user);
        string GenerateRandomPassword(PasswordOptions? pwdOptions = null);
    }
}
