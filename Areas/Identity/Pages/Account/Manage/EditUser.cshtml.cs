#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Models.Identity;
using NuJournalPro.Services.Identity;
using System.ComponentModel.DataAnnotations;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class EditUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly SignInManager<NuJournalUser> _signInManager;
        private readonly ILogger<EditUserModel> _logger;
        private readonly DefaultUserSettings _defaultUserSettings;
        private readonly DefaultGraphics _defaultGraphics;
        private readonly UserService _userService;

        public EditUserModel(UserManager<NuJournalUser> userManager,
                                     SignInManager<NuJournalUser> signInManager,
                                     ILogger<EditUserModel> logger,
                                     IOptions<DefaultUserSettings> defaultUserSettings,
                                     IOptions<DefaultGraphics> defaultGraphics,
                                     UserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _defaultUserSettings = defaultUserSettings.Value;
            _defaultGraphics = defaultGraphics.Value;
            _userService = userService;
        }

        public CompressedImage AccessDeniedImage { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public string visibilityUserForm { get; set; } = "d-none";

        public List<NuJournalUser> AppUserList { get; set; } = new List<NuJournalUser>();

        [Display(Name = "Select a user to edit")]
        public string SelectedUser { get; set; }

        [BindProperty]
        public InputModel ExistingUserInput { get; set; }

        public class InputModel : UserInputModel { }

        public async Task<IActionResult> OnGetAsync()
        {
            var activeUser = await _userManager.GetUserAsync(User);
            if (activeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var activeUserInfo = await LoadActiveUserAsync(activeUser);

            ExistingUserInput = new InputModel()
            {
                Avatar = await _userService.GetDefaultUserAvatar()
            };

            if (activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()) != true && activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()) != true)
            {
                accessDeniedImageData = await _imageService.EncodeImageAsync(_defaultGraphics.SecureAccess);
                accessDeniedMimeType = _imageService.MimeType(_defaultGraphics.SecureAccess);
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


                    ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithRoles");
                }
                else
                {
                    StatusMessage = "Access Denied: You do not have access to this resource.";
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string selectedUser)
        {
            var activeUser = await _userManager.GetUserAsync(User);
            if (activeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var activeUserInfo = await LoadActiveUserAsync(activeUser);
            var selectedUserInfo = await _userManager.FindByEmailAsync(selectedUser);

            ViewData["SelectUserList"] = new SelectList(LoadSelectUserList(activeUser), "Email", "UserNameWithRoles");

            Input.FirstName = selectedUserInfo.FirstName;
            Input.MiddleName = selectedUserInfo.MiddleName;
            Input.LastName = selectedUserInfo.LastName;
            Input.DisplayName = selectedUserInfo.DisplayName;
            Input.Email = selectedUserInfo.Email;
            Input.PhoneNumber = selectedUserInfo.PhoneNumber;
            Input.ImageData = selectedUserInfo.ImageData;
            Input.MimeType = selectedUserInfo.MimeType;

            visibilityUserForm = string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostModifyUserAsync(InputModel input)
        {
            var activeUser = await _userManager.GetUserAsync(User);
            if (activeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var activeUserInfo = await LoadActiveUserAsync(activeUser);

            ViewData["SelectUserList"] = new SelectList(LoadSelectUserList(activeUser), "Email", "UserNameWithRoles");

            var selectedUserInfo = await _userManager.FindByEmailAsync(input.Email);

            if (ModelState.IsValid)
            {
                if (Input.FirstName != selectedUserInfo.FirstName || Input.MiddleName != selectedUserInfo.MiddleName || Input.LastName != selectedUserInfo.LastName || Input.DisplayName != selectedUserInfo.DisplayName)
                {
                    if (Input.FirstName != selectedUserInfo.FirstName) selectedUserInfo.FirstName = Input.FirstName;
                    if (Input.MiddleName != selectedUserInfo.MiddleName) selectedUserInfo.MiddleName = Input.MiddleName;
                    if (Input.LastName != selectedUserInfo.LastName) selectedUserInfo.LastName = Input.LastName;

                    await _userManager.UpdateAsync(selectedUserInfo);
                }
            }

            return Page();
        }

        private List<NuJournalUser> LoadSelectUserList(NuJournalUser activeUserInfo)
        {
            if (activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()))
            {
                return _userManager.Users.Cast<NuJournalUser>()
                                         .Where(u => !u.UserName.Equals(activeUserInfo.UserName))
                                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                                         .OrderBy(r => r.UserRoles)
                                         .ToList();
            }
            else if (!activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()) && activeUserInfo.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()))
            {
                return _userManager.Users.Cast<NuJournalUser>()
                                         .Where(u => !u.UserName.Equals(activeUserInfo.UserName))
                                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Administrator.ToString()))
                                         .OrderBy(r => r.UserRoles)
                                         .ToList();
            }
            else return null;
        }
    }
}
