#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class DeleteUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<AddNewUserModel> _logger;
        private readonly IImageService _imageService;
        private readonly DefaultGraphics _defaultGraphics;


        public DeleteUserModel(UserManager<NuJournalUser> userManager,
                                       ILogger<AddNewUserModel> logger,
                                       IImageService imageService,
                                       IOptions<DefaultGraphics> defaultGraphics)
        {
            _userManager = userManager;
            _logger = logger;
            _imageService = imageService;
            _defaultGraphics = defaultGraphics.Value;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public List<NuJournalUser> AppUserList { get; set; } = new List<NuJournalUser>();

        public NuJournalUser SelectedUser { get; set; }

        public string ConfirmEmail { get; set; }

        public byte[] AccessDeniedImageData { get; set; }
        public string AccessDeniedMimeType { get; set; }

        private class ActiveUserInfo
        {
            public ActiveUserInfo()
            {
            }

            public string UserName { get; set; }
            public List<string> UserRoles { get; set; } = new List<string>();
            public string UserRolesString
            {
                get
                {
                    return string.Join(", ", UserRoles);
                }
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var activeUser = await _userManager.GetUserAsync(User);
            
            if (activeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var activeUserInfo = await LoadActiveUserAsync(activeUser);

            if (!activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()) && !activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()))
            { 
                AccessDeniedImageData = await _imageService.EncodeImageAsync(_defaultGraphics.SecureAccess);
                AccessDeniedMimeType = _imageService.MimeType(_defaultGraphics.SecureAccess);
            }
            else
            {
                if (activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()))
                {
                    var appUserList = _userManager.Users.Cast<NuJournalUser>()
                                                        .Where(u => !u.UserName.Equals(activeUserInfo.UserName))
                                                        .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                                                        .OrderBy(r => r.UserRoles)
                                                        .ToList();

                    if (appUserList.Count == 0)
                    {
                        StatusMessage = "There are no users to delete.";
                        return Page();
                    }

                    ViewData["SelectUserList"] = new SelectList(appUserList, "UserName", "UserNameWithRoles");

                }
                else if (activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()))
                {
                    var appUserList = _userManager.Users.Cast<NuJournalUser>()
                                                        .Where(u => !u.UserName.Equals(activeUserInfo.UserName))
                                                        .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                                                        .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Administrator.ToString()))
                                                        .OrderBy(r => r.UserRoles)
                                                        .ToList();

                    if (appUserList.Count == 0)
                    {
                        StatusMessage = "There are no users to delete.";
                        return Page();
                    }

                    ViewData["SelectUserList"] = new SelectList(appUserList, "UserName", "UserNameWithRoles");
                }
                else
                {
                    //ViewData["SelectUserList"] = new SelectList();
                }

            }
            


            return Page();
        }

        private async Task<ActiveUserInfo> LoadActiveUserAsync(NuJournalUser activeUser)
        {
            return new ActiveUserInfo()
            {
                UserName = await _userManager.GetUserNameAsync(activeUser),
                UserRoles = await _userManager.GetRolesAsync(activeUser) as List<string>
            };
        }
    }
}