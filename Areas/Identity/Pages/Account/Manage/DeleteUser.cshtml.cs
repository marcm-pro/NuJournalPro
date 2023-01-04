#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

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

        [Display(Name = "Select a user to delete")]
        public string SelectedUser { get; set; }

        [Display(Name = "Confirm user deletion by entering the user's email address")]
        [Required]
        public string ConfirmUserName { get; set; }

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
                    AppUserList = _userManager.Users.Cast<NuJournalUser>()
                                                    .Where(u => !u.UserName.Equals(activeUserInfo.UserName))
                                                    .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                                                    .OrderBy(r => r.UserRoles)
                                                    .ToList();

                    if (AppUserList.Count == 0)
                    {
                        StatusMessage = "There are no users to delete.";
                        return Page();
                    }

                    ViewData["SelectUserList"] = new SelectList(AppUserList, "UserName", "UserNameWithRoles");

                }
                else if (activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()) && !activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()))
                {
                    AppUserList = _userManager.Users.Cast<NuJournalUser>()
                                                    .Where(u => !u.UserName.Equals(activeUserInfo.UserName))
                                                    .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                                                    .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Administrator.ToString()))
                                                    .OrderBy(r => r.UserRoles)
                                                    .ToList();

                    if (AppUserList.Count == 0)
                    {
                        StatusMessage = "There are no users to delete.";
                        return Page();
                    }

                    SelectedUser = AppUserList[0].UserNameWithRoles;
                    ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithRoles");
                }
                else
                {
                    StatusMessage = "Access Denied: You do not have access to this resource.";                    
                }

            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string selectedUser, string confirmUserName)
        {
            if (ModelState.IsValid)
            {
                var activeUser = await _userManager.GetUserAsync(User);
                if (activeUser == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var activeUserInfo = await LoadActiveUserAsync(activeUser);

                if (selectedUser.Contains(confirmUserName))
                {
                    var deleteUser = await _userManager.FindByEmailAsync(selectedUser);
                    var userDeleteResult = await _userManager.DeleteAsync(deleteUser);

                    if (userDeleteResult.Succeeded)
                    {
                        _logger.LogInformation($"User {selectedUser} was deleted by {activeUser.UserName}.");
                        StatusMessage = $"User {selectedUser} was deleted by {activeUser.UserName}.";
                    }
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