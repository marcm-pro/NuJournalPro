#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class AdminPanelModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<ProfilePictureModel> _logger;
        private readonly IImageService _imageService;
        private readonly DefaultGraphics _defaultGraphics;

        public AdminPanelModel(UserManager<NuJournalUser> userManager,
                               ILogger<ProfilePictureModel> logger,
                               IImageService imageService,
                               IOptions<DefaultGraphics> defaultGraphics)
        {
            _userManager = userManager;
            _logger = logger;
            _imageService = imageService;
            _defaultGraphics = defaultGraphics.Value;
        }

        public byte[] AccessDeniedImageData { get; set; }
        public string AccessDeniedMimeType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!_userManager.GetRolesAsync(appUser).ToString().Contains(NuJournalUserRole.Owner.ToString()))
            {
                AccessDeniedImageData = await _imageService.EncodeImageAsync(_defaultGraphics.SecureAccess);
                AccessDeniedMimeType = _imageService.MimeType(_defaultGraphics.SecureAccess);
            }

            return Page();
        }
    }
}
