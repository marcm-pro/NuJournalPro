#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuJournalPro.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class SocialMediaModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<ProfilePictureModel> _logger;

        public SocialMediaModel(UserManager<NuJournalUser> userManager,
                                ILogger<ProfilePictureModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            // Social Media
            [Display(Name = "GitHub Repository")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string GitHubUrl { get; set; }

            [Display(Name = "Twitter Profile")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string TwitterUrl { get; set; }

            [Display(Name = "LinkedIn Profile")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string LinkedInUrl { get; set; }

            [Display(Name = "YouTube Channel")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string YouTubeUrl { get; set; }

            [Display(Name = "Facebook Profile")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string FacebookUrl { get; set; }

            [Display(Name = "Instagram Profile")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string InstagramUrl { get; set; }
        }

        private async Task LoadAsync(NuJournalUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            Input = new InputModel
            {
                GitHubUrl = user.GitHubUrl,
                TwitterUrl = user.TwitterUrl,
                LinkedInUrl = user.LinkedInUrl,
                YouTubeUrl = user.YouTubeUrl,
                FacebookUrl = user.FacebookUrl,
                InstagramUrl = user.InstagramUrl
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

            if (Input.GitHubUrl != user.GitHubUrl || Input.TwitterUrl != user.TwitterUrl || Input.LinkedInUrl != user.LinkedInUrl || Input.YouTubeUrl != user.YouTubeUrl || Input.FacebookUrl != user.FacebookUrl || Input.InstagramUrl != user.InstagramUrl)
            {
                user.GitHubUrl = Input.GitHubUrl;
                user.TwitterUrl = Input.TwitterUrl;
                user.LinkedInUrl = Input.LinkedInUrl;
                user.YouTubeUrl = Input.YouTubeUrl;
                user.FacebookUrl = Input.FacebookUrl;
                user.InstagramUrl = Input.InstagramUrl;
                await _userManager.UpdateAsync(user);
                StatusMessage = "Your social media links have been updated";
            }

            return RedirectToPage();
        }
    }
}
