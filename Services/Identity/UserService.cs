#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Models.Identity;
using NuJournalPro.Services.Identity.Interfaces;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Services.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly IImageService _imageService;
        private readonly DefaultUserSettings _defaultUserSettings;
        private readonly DefaultGraphics _defaultGraphics;

        public UserService(UserManager<NuJournalUser> userManager,
                           IImageService imageService,
                           IOptions<DefaultUserSettings> defaultUserSettings,
                           IOptions<DefaultGraphics> defaultGraphics)
        {
            _userManager = userManager;
            _imageService = imageService;
            _defaultUserSettings = defaultUserSettings.Value;
            _defaultGraphics = defaultGraphics.Value;
        }

        public async Task<NuJournalUser> CreateNewUserAsync(UserInputModel userModel, UserInfo parentUserInfo = null, IFormFile newUserAvatarFile = null)
        {
            NuJournalUser newUser = Activator.CreateInstance<NuJournalUser>();
            newUser.UserName = userModel.Email;
            newUser.Email = userModel.Email;
            newUser.FirstName = userModel.FirstName;
            newUser.MiddleName = userModel.MiddleName;
            newUser.LastName = userModel.LastName;
            newUser.DisplayName = userModel.DisplayName;
            newUser.PhoneNumber = userModel.PhoneNumber;

            if (parentUserInfo is not null)
            {
                newUser.CreatedByUser = parentUserInfo.UserName;
                newUser.CreatedByRoles = parentUserInfo.UserRoles;
            }
            else newUser.CreatedByUser = "User Service"; // In this case the User Role(s) will be added automatically by the NuJournalUser data model.
            
            if (newUserAvatarFile is not null) newUser.Avatar = await _imageService.EncodeImageAsync(newUserAvatarFile);
            else newUser.Avatar = await GetDefaultUserAvatar();
            
            return newUser;
        }

        public async Task<CompressedImage> GetAccessDeniedImage()
        {
            return await _imageService.EncodeImageAsync(_defaultGraphics.SecureAccess);
        }

        public async Task<CompressedImage> GetDefaultUserAvatar()
        {
            return new CompressedImage()
            {
                CompressedImageData = await _imageService.EncodeImageAsync(_defaultUserSettings.Avatar, true),
                MimeType = _imageService.MimeType(_defaultUserSettings.Avatar)
            };
        }
        
        public async Task<UserInfo> GetUserInfoAsync(NuJournalUser user)
        {
            return new UserInfo()
            {
                UserName = await _userManager.GetUserNameAsync(user),
                UserRoles = await _userManager.GetRolesAsync(user) as List<string>
            };
        }

        public bool IsAdmin(NuJournalUser user)
        {
            if (user.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()) && !user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString())) return true;
            else return false;
        }

        public bool IsOwner(NuJournalUser user)
        {
            if (user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString())) return true;
            else return false;
        }

        public string GenerateRandomPassword(PasswordOptions pwdOptions = null)
        {
            if (pwdOptions == null)
            {
                pwdOptions = new PasswordOptions()
                {
                    RequiredLength = 16,
                    RequiredUniqueChars = 4,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = true,
                    RequireUppercase = true
                };
            }

            string[] characterPool = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",
                "abcdefghijkmnopqrstuvwxyz",
                "0123456789",
                "!@$?_-"
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (pwdOptions.RequireUppercase) chars.Insert(rand.Next(0, chars.Count), characterPool[0][rand.Next(0, characterPool[0].Length)]);
            if (pwdOptions.RequireLowercase) chars.Insert(rand.Next(0, chars.Count), characterPool[1][rand.Next(0, characterPool[1].Length)]);
            if (pwdOptions.RequireDigit) chars.Insert(rand.Next(0, chars.Count), characterPool[2][rand.Next(0, characterPool[2].Length)]);
            if (pwdOptions.RequireNonAlphanumeric) chars.Insert(rand.Next(0, chars.Count), characterPool[3][rand.Next(0, characterPool[3].Length)]);

            for (int i = chars.Count; i < pwdOptions.RequiredLength || chars.Distinct().Count() < pwdOptions.RequiredUniqueChars; i++)
            {
                string rcs = characterPool[rand.Next(0, characterPool.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}
