#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NuJournalPro.Models;


namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class OwnerPanelModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<ProfilePictureModel> _logger;

        public OwnerPanelModel(UserManager<NuJournalUser> userManager,
                               ILogger<ProfilePictureModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }
    }
}