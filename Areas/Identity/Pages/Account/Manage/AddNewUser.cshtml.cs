#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Models.ViewModels;
using NuJournalPro.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class AddNewUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly SignInManager<NuJournalUser> _signInManager;
        private readonly ILogger<AddNewUserModel> _logger;
        private readonly IImageService _imageService;
        private readonly DefaultUserSettings _defaultUserSettings;
        private readonly DefaultGraphics _defaultGraphics;
        private readonly IUserEmailStore<NuJournalUser> _newUserEmailStore;
        private readonly IUserStore<NuJournalUser> _newUserStore;

        public AddNewUserModel(UserManager<NuJournalUser> userManager,
                               SignInManager<NuJournalUser> signInManager,
                               ILogger<AddNewUserModel> logger,
                               IImageService imageService,
                               IOptions<DefaultUserSettings> defaultUserSettings,
                               IOptions<DefaultGraphics> defaultGraphics,
                               IUserStore<NuJournalUser> newUserStore)
        {
            _userManager = userManager;
            _logger = logger;
            _imageService = imageService;
            _defaultUserSettings = defaultUserSettings.Value;
            _signInManager = signInManager;
            _defaultGraphics = defaultGraphics.Value;
            _newUserStore = newUserStore;
            _newUserEmailStore = GetEmailStore();
        }

        public byte[] AccessDeniedImageData { get; set; }
        public string AccessDeniedMimeType { get; set; }

        [Display(Name = "Select User Role")]
        public NuJournalUserRole NewUserRole { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel NewUserInput { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            public string FirstName { get; set; }

            [Display(Name = "Middle Name")]
            [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 1)]
            public string MiddleName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Public Display Name")]
            [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            public string DisplayName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email / Username")]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public byte[] ImageData { get; set; }
            public string MimeType { get; set; }
            public IFormFile ImageFile { get; set; }
        }

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

            if (activeUserInfo.UserRolesString.Equals("Owner") != true && activeUserInfo.UserRolesString.Equals("Administrator") != true)
            {
                AccessDeniedImageData = await _imageService.EncodeImageAsync(_defaultGraphics.SecureAccess);
                AccessDeniedMimeType = _imageService.MimeType(_defaultGraphics.SecureAccess);
            }
            else
            {
                NewUserInput = new InputModel()
                {
                    ImageData = await _imageService.EncodeImageAsync(_defaultUserSettings.Avatar),
                    MimeType = _imageService.MimeType(_defaultUserSettings.Avatar)
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

                var activeUserInfo = await LoadActiveUserAsync(activeUser);

                var newUser = await CreateNewUser(activeUserInfo, NewUserInput.ImageFile);

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

        private async Task<ActiveUserInfo> LoadActiveUserAsync(NuJournalUser activeUser)
        {
            return new ActiveUserInfo()
            {
                UserName = await _userManager.GetUserNameAsync(activeUser),
                UserRoles = await _userManager.GetRolesAsync(activeUser) as List<string>
            };
        }

        private async Task<NuJournalUser> CreateNewUser(ActiveUserInfo activeUserInfo, IFormFile newUserImageFile)
        {
            try
            {
                NuJournalUser newUser = Activator.CreateInstance<NuJournalUser>();
                newUser.UserName = NewUserInput.Email;
                newUser.Email = NewUserInput.Email;
                newUser.FirstName = NewUserInput.FirstName;
                newUser.MiddleName = NewUserInput.MiddleName;
                newUser.LastName = NewUserInput.LastName;
                newUser.DisplayName = NewUserInput.DisplayName;
                newUser.PhoneNumber = NewUserInput.PhoneNumber;
                newUser.CreatedByUser = activeUserInfo.UserName;
                newUser.CreatedByRoles = activeUserInfo.UserRoles;
                if (newUserImageFile == null)
                {
                    newUser.ImageData = await _imageService.EncodeImageAsync(_defaultUserSettings.Avatar);
                    newUser.MimeType = _imageService.MimeType(_defaultUserSettings.Avatar);
                }
                else
                {
                    newUser.ImageData = await _imageService.EncodeImageAsync(newUserImageFile);
                    newUser.MimeType = _imageService.MimeType(newUserImageFile);
                }
                newUser.EmailConfirmed = true;
                return newUser;
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(NuJournalUser)}'. " +
                    $"Ensure that '{nameof(NuJournalUser)}' is not an abstract class and has a parameterless constructor.");
            }
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