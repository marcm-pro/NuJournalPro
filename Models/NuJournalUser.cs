using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NuJournalPro.Models
{
    public class NuJournalUser : IdentityUser
    {
        // Personal Information
        [Required]
        [Display(Name = "First Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{FirstName} {MiddleName} {LastName}";
            }
        }

        // Public Display Name
        [Required]
        [Display(Name = "Display Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? DisplayName { get; set; }

        // User Join - Date and Time
        [Display(Name = "Joined")]
        [DataType(DataType.DateTime)]
        public DateTime Joined { get; set; } = DateTime.UtcNow;

        // User Avatar or Profile Picture
        public byte[]? ImageData { get; set; }
        public string? MimeType { get; set; }
        [NotMapped]
        IFormFile? ImageFile { get; set; }

        // Social Media
        [Display(Name = "GitHub Repository")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]        
        public string? GitHubUrl { get; set; }

        [Display(Name = "Twitter Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? TwitterUrl { get; set; }

        [Display(Name = "LinkedIn Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]        
        public string? LinkedInUrl { get; set; }

        [Display(Name = "YouTube Channel")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? YouTubeUrl { get; set; }

        [Display(Name = "Facebook Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? FacebookUrl { get; set; }

        [Display(Name = "Instagram Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]        
        public string? InstagramUrl { get; set; }

        // Database Navigation Properties
        public virtual ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>();
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }
}
