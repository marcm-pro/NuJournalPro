#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuJournalPro.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using NuJournalPro.Services.Interfaces;
using System.Reflection.Metadata;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class ProfilePictureModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<ProfilePictureModel> _logger;
        private readonly IImageService _imageService;

        public ProfilePictureModel(UserManager<NuJournalUser> userManager,
                                   ILogger<ProfilePictureModel> logger,
                                   IImageService imageService)
        {
            _userManager = userManager;
            _logger = logger;
            _imageService = imageService;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string FullName { get; set; }
            public string DisplayName { get; set; }
            public byte[] ImageData { get; set; }
            public string MimeType { get; set; }
            public IFormFile ImageFile { get; set; }
        }

        private async Task LoadAsync(NuJournalUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FullName = user.FullName,
                DisplayName = user.DisplayName,
                ImageData = user.ImageData,
                MimeType = user.MimeType
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
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.ImageFile != null)
            {
                user.ImageData = await _imageService.EncodeImageAsync(Input.ImageFile);
                user.MimeType = _imageService.MimeType(Input.ImageFile);

                await _userManager.UpdateAsync(user);
                StatusMessage = "Your profile picture has been updated";
            }

            return RedirectToPage();
        }
    }
}