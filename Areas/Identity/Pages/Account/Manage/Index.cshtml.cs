// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuJournalPro.Enums;
using NuJournalPro.Models;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly SignInManager<NuJournalUser> _signInManager;

        public IndexModel(
            UserManager<NuJournalUser> userManager,
            SignInManager<NuJournalUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        public string UserRole { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// 
        [BindProperty]
        public InputModel Input { get; set; }

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
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(NuJournalUser user)
        {
            Username = await _userManager.GetUserNameAsync(user);
            UserRole = String.Join(", ", await _userManager.GetRolesAsync(user));
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var firstName = user.FirstName;
            var middleName = user.MiddleName;
            var lastName = user.LastName;
            var displayName = user.DisplayName;

            if (middleName != null && middleName.Length == 1)
            {
                middleName += ".";
            }           

            Input = new InputModel
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DisplayName = displayName,
                PhoneNumber = phoneNumber
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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRole = String.Join(", ", await _userManager.GetRolesAsync(user));
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.FirstName != user.FirstName || Input.MiddleName != user.MiddleName || Input.LastName != user.LastName || Input.DisplayName != user.DisplayName)
            {
                if (Input.FirstName != user.FirstName) user.FirstName = Input.FirstName;
                if (Input.MiddleName != user.MiddleName) user.MiddleName = Input.MiddleName;
                if (Input.LastName != user.LastName) user.LastName = Input.LastName;
                if (Input.DisplayName != user.DisplayName)
                {
                    foreach (var appUser in _userManager.Users.ToList())
                    {
                        if (Input.DisplayName.ToUpper() == appUser.DisplayName.ToUpper())
                        {
                            ModelState.AddModelError("Input.DisplayName", $"A user with the {Input.DisplayName} public display name already exists.");
                            return Page();
                        }
                        if (Regex.Replace(Input.DisplayName.ToUpper(), @"[^0-9a-zA-Z]+", "") == Regex.Replace(appUser.DisplayName.ToUpper(), @"[^0-9a-zA-Z]+", ""))
                        {
                            ModelState.AddModelError("Input.DisplayName", $"A user with a similar public display name to {Input.DisplayName} already exists.");
                            return Page();
                        }
                    }

                    if (userRole.Contains("Owner") == true) user.DisplayName = Input.DisplayName;
                    
                    else
                    {
                        foreach (var notAllowed in Enum.GetValues(typeof(ForbidenDisplayName)).Cast<ForbidenDisplayName>().ToList())
                        {
                            if (Input.DisplayName.ToUpper().Contains(notAllowed.ToString().ToUpper()))
                            {
                                ModelState.AddModelError("Input.DisplayName", $"The public display name {Input.DisplayName} is not allowed.");
                                return Page();
                            }
                        }
                        user.DisplayName = Input.DisplayName;
                    }
                }
                await _userManager.UpdateAsync(user);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
