#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Models.Identity;
using NuJournalPro.Services.Identity.Interfaces;
using NuJournalPro.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class AddNewUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly SignInManager<NuJournalUser> _signInManager;
        private readonly ILogger<AddNewUserModel> _logger;
        private readonly DefaultUserSettings _defaultUserSettings;
        private readonly DefaultGraphics _defaultGraphics;
        private readonly IUserEmailStore<NuJournalUser> _newUserEmailStore;
        private readonly IUserStore<NuJournalUser> _newUserStore;
        private readonly IUserService _userService;

        public AddNewUserModel(UserManager<NuJournalUser> userManager,
                               SignInManager<NuJournalUser> signInManager,
                               ILogger<AddNewUserModel> logger,
                               IOptions<DefaultUserSettings> defaultUserSettings,
                               IOptions<DefaultGraphics> defaultGraphics,
                               IUserStore<NuJournalUser> newUserStore,
                               IUserService userService)
        {
            _userManager = userManager;
            _logger = logger;
            _defaultUserSettings = defaultUserSettings.Value;
            _signInManager = signInManager;
            _defaultGraphics = defaultGraphics.Value;
            _newUserStore = newUserStore;
            _newUserEmailStore = GetEmailStore();
            _userService = userService;
        }

        public CompressedImage AccessDeniedImage { get; set; }

        [Display(Name = "Select User Role")]
        public NuJournalUserRole NewUserRole { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel NewUserInput { get; set; }

        public class InputModel : UserInputModel { }
        
        public async Task<IActionResult> OnGetAsync()
        {
            var activeUser = await _userManager.GetUserAsync(User);
            if (activeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var activeUserInfo = await _userService.GetUserInfoAsync(activeUser);

            if (!_userService.IsOwner(activeUser) && !_userService.IsAdmin(activeUser))
            {

            }

            if (activeUserInfo.UserRolesString.Equals("Owner") != true && activeUserInfo.UserRolesString.Equals("Administrator") != true) AccessDeniedImage = await _userService.GetAccessDeniedImage();
            else
            {
                NewUserInput = new InputModel()
                {
                    Avatar = await _userService.GetDefaultUserAvatar()
                };

                if (activeUserInfo.UserRolesString.Equals("Owner"))
                {
                    ViewData["NewUserRoleList"] = new SelectList(Enum.GetValues(typeof(NuJournalUserRole))
                                                                     .Cast<NuJournalUserRole>()
                                                                     .Where(r => r != NuJournalUserRole.Owner)
                                                                     .ToList(), NuJournalUserRole.Reader);
                    NewUserRole = NuJournalUserRole.Reader;
                }
                else if (activeUserInfo.UserRolesString.Equals("Administrator"))
                {
                    ViewData["NewUserRoleList"] = new SelectList(Enum.GetValues(typeof(NuJournalUserRole))
                                                                     .Cast<NuJournalUserRole>()
                                                                     .Where(r => r != NuJournalUserRole.Owner)
                                                                     .Where(r => r != NuJournalUserRole.Administrator)
                                                                     .ToList(), NuJournalUserRole.Reader);
                    NewUserRole = NuJournalUserRole.Reader;
                }
                else
                {
                    ViewData["NewUserRoleList"] = null;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(NuJournalUserRole newUserRole)
        {
            if (ModelState.IsValid)
            {

                var activeUser = await _userManager.GetUserAsync(User);
                if (activeUser == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var activeUserInfo = await _userService.GetUserInfoAsync(activeUser);

                var newUser = await _userService.CreateNewUserAsync(NewUserInput, activeUserInfo, NewUserInput.ImageFile);

                if (activeUserInfo.UserRolesString.Contains("Owner") != true && activeUserInfo.UserRolesString.Contains("Administrator") == true)
                {
                    foreach (var notAllowed in Enum.GetValues(typeof(ForbidenDisplayName)).Cast<ForbidenDisplayName>().ToList())
                    {
                        if (NewUserInput.DisplayName.ToUpper().Contains(notAllowed.ToString().ToUpper()))
                        {
                            ModelState.AddModelError("Input.DisplayName", $"The public display name {NewUserInput.DisplayName} is not allowed.");
                            return Page();
                        }
                    }
                }

                foreach (var appUser in _userManager.Users.ToList())
                {
                    if (newUser.DisplayName.ToUpper() == appUser.DisplayName.ToUpper())
                    {
                        ModelState.AddModelError("Input.DisplayName", $"A user with the {newUser.DisplayName} public display name already exists.");
                        return Page();
                    }
                    if (Regex.Replace(newUser.DisplayName.ToUpper(), @"[^0-9a-zA-Z]+", "") == Regex.Replace(appUser.DisplayName.ToUpper(), @"[^0-9a-zA-Z]+", ""))
                    {
                        ModelState.AddModelError("Input.DisplayName", $"A user with a similar public display name to {newUser.DisplayName} already exists.");
                        return Page();
                    }
                }

                await _newUserStore.SetUserNameAsync(newUser, NewUserInput.Email, CancellationToken.None);
                await _newUserEmailStore.SetEmailAsync(newUser, NewUserInput.Email, CancellationToken.None);

                newUser.UserRoles.Add(newUserRole.ToString());

                var userCreationResult = await _userManager.CreateAsync(newUser, NewUserInput.Password);

                if (userCreationResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, newUserRole.ToString());

                    _logger.LogInformation($"User {activeUserInfo.UserName} created a new account with the username {newUser.UserName} and assigned this account the {newUserRole.ToString()} role.");

                    StatusMessage = $"User {newUser.UserName} with the role {newUserRole.ToString()} has been successfully created.";
                    return RedirectToPage();
                }

                foreach (var error in userCreationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay the new user registration form.
            return Page();
        }

        private IUserEmailStore<NuJournalUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<NuJournalUser>)_newUserStore;
        }
    }
}