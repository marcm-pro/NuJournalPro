using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NuJournalPro.Models.ViewModels
{
    public class ContactUs
    {
        [Required]
        [Description("Name")]
        [StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        public string? Name { get; set; }
        
        [Required]
        [Description("Email Address")]
        [EmailAddress]
        [StringLength(64, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string? Email { get; set; }
        
        [Description("Phone Number")]
        [Phone]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
        public string? Phone { get; set; }

        [Required]
        [Description("Subject")]
        [StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        public string? Subject { get; set; }

        [Required]
        [Description("Message")]
        [StringLength(4096, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 16)]
        public string? Message { get; set; }
    }
}
