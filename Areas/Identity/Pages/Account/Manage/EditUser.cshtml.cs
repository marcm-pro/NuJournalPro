#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NuJournalPro.Models;
using NuJournalPro.Models.ViewModels;
using NuJournalPro.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class EditUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly SignInManager<NuJournalUser> _signInManager;
        private readonly ILogger<AddNewUserModel> _logger;
        private readonly IImageService _imageService;
        private readonly DefaultUserSettings _defaultUserSettings;
        private readonly DefaultGraphics _defaultGraphics;

        public EditUserModel(UserManager<NuJournalUser> userManager,
                                     SignInManager<NuJournalUser> signInManager,
                                     ILogger<AddNewUserModel> logger,
                                     IImageService imageService,
                                     IOptions<DefaultUserSettings> defaultUserSettings,
                                     IOptions<DefaultGraphics> defaultGraphics)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _imageService = imageService;
            _defaultUserSettings = defaultUserSettings.Value;
            _defaultGraphics = defaultGraphics.Value;
        }

        public string Username { get; set; }

        public string UserRole { get; set; }

        public byte[] accessDeniedImageData { get; set; }
        public string accessDeniedMimeType { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

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

        private async Task LoadAsync(NuJournalUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var userRole = await _userManager.GetRolesAsync(user);
            Username = userName;
            UserRole = String.Join(", ", userRole); // in case the user has more than one role
            Input = new InputModel
            {
                ImageData = await _imageService.EncodeImageAsync(_defaultUserSettings.Avatar),
                MimeType = _imageService.MimeType(_defaultUserSettings.Avatar),
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            if (UserRole.Equals("Owner") != true && UserRole.Equals("Administrator") != true)
            {
                accessDeniedImageData = await _imageService.EncodeImageAsync(_defaultGraphics.SecureAccess);
                accessDeniedMimeType = _imageService.MimeType(_defaultGraphics.SecureAccess);
            }
            else
            {
                //foreach (var appUser in _userManager.Users.ToList())
                //{
                //    AppUserList.Add(new AppUserList()
                //    {
                //        userEmail = appUser.Email,
                //        userFullName = appUser.FullName,
                //        userRole = String.Join(", ", await _userManager.GetRolesAsync(appUser))
                //    });
                //}
                //ViewData["AppUserList"] = new SelectList(AppUserList, "userEmail");

                //ViewData["USStatesList"] = new SelectList(Enum.GetValues(typeof(USStates)).Cast<USStates>().ToList());
            }

            return Page();
        }
    }
}
