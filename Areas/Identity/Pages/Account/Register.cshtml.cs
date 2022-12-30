// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<NuJournalUser> _signInManager;
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly IUserStore<NuJournalUser> _userStore;
        private readonly IUserEmailStore<NuJournalUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IImageService _imageService;
        private readonly DefaultUserSettings _defaultUserSettings;

        public RegisterModel(
            UserManager<NuJournalUser> userManager,
            IUserStore<NuJournalUser> userStore,
            SignInManager<NuJournalUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IImageService imageService,
            IOptions<DefaultUserSettings> defaultUserSettings)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _defaultUserSettings = defaultUserSettings.Value;
            _imageService = imageService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                foreach (var notAllowed in Enum.GetValues(typeof(ForbidenDisplayName)).Cast<ForbidenDisplayName>().ToList())
                {
                    if (Input.DisplayName.ToUpper().Contains(notAllowed.ToString().ToUpper()))
                    {
                        ModelState.AddModelError("Input.DisplayName", $"The public display name {Input.DisplayName} is not allowed.");
                        return Page();
                    }
                }

                foreach (var newUser in _userManager.Users.ToList())
                {
                    if (Input.DisplayName.ToUpper() == newUser.DisplayName.ToUpper())
                    {
                        ModelState.AddModelError("Input.DisplayName", $"A user with the {Input.DisplayName} public display name already exists.");
                        return Page();
                    }
                    if (Regex.Replace(Input.DisplayName.ToUpper(), @"[^0-9a-zA-Z]+", "") == Regex.Replace(newUser.DisplayName.ToUpper(), @"[^0-9a-zA-Z]+", ""))
                    {
                        ModelState.AddModelError("Input.DisplayName", $"A user with a similar public display name to {Input.DisplayName} already exists.");
                        return Page();
                    }
                }

                user.ImageData = await _imageService.EncodeImageAsync(_defaultUserSettings.Avatar);
                user.MimeType = _imageService.MimeType(_defaultUserSettings.Avatar);

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    foreach (var userRole in Enum.GetNames(typeof(NuJournalUserRole)))
                    {
                        if (userRole == _defaultUserSettings.Role)
                        {
                            await _userManager.AddToRoleAsync(user, userRole);
                        }
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private NuJournalUser CreateUser()
        {
            try
            {
                NuJournalUser user = Activator.CreateInstance<NuJournalUser>();
                user.FirstName = Input.FirstName;
                user.MiddleName = Input.MiddleName;
                user.LastName = Input.LastName;
                user.DisplayName = Input.DisplayName;
                return user;
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(NuJournalUser)}'. " +
                    $"Ensure that '{nameof(NuJournalUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<NuJournalUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<NuJournalUser>)_userStore;
        }
    }
}
