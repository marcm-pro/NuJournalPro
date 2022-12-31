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
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Middle Name or Initial")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 1)]
        public string? MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(MiddleName))
                {
                    return $"{FirstName} {LastName}";
                }
                else if (MiddleName.Length == 1)
                {
                    return $"{FirstName} {MiddleName}. {LastName}";
                }
                else
                {
                    return $"{FirstName} {MiddleName[0]}. {LastName}";
                }
            }
        }

        // Public Display Name
        [Required]
        [Display(Name = "Public Display Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string DisplayName { get; set; } = string.Empty;

        // User Join - Date and Time
        [Display(Name = "Joined")]
        [DataType(DataType.DateTime)]
        public DateTime Joined { get; set; } = DateTime.UtcNow;

        [Display(Name = "Modified")]
        [DataType(DataType.DateTime)]
        public DateTime? Modified { get; set; } = null;

        // The username that created this user
        public string CreatedByUser { get; set; } = "User Registration";

        // The user role that created this user
        public List<string> CreatedByRoles { get; set; } = new List<string>() { "Application" };
        public string CreatedByRolesString
        {
            get
            {
                return string.Join(", ", CreatedByRoles);
            }
        }

        // The username that modified this user last time.

        public string? ModifiedByUser { get; set; } = null;
        // The user role that modified this user last time.
        public List<string>? ModifiedByRoles { get; set; } = null;
        public string? ModifiedByRolesString
        {
            get
            {
                if (ModifiedByRoles != null)
                {
                    return string.Join(", ", ModifiedByRoles);
                }
                else
                {
                    return null;
                }
            }
        }

        // User Avatar or Profile Picture
        public byte[]? ImageData { get; set; }
        public string? MimeType { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        // User Roles (Backup)
        [Display(Name = "User Role")]
        public List<string> UserRoles { get; set; } = new List<string>();
        public string UserRolesString
        {
            get
            {
                return string.Join(", ", UserRoles);
            }
        }

        // UserName with UserRoles combined
        public string UserNameWithRoles
        {
            get
            {
                return $"{UserName} ({UserRolesString})";
            }
        }

        // Social Media
        [Display(Name = "GitHub Repository")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? GitHubUrl { get; set; }

        [Display(Name = "Twitter Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? TwitterUrl { get; set; }

        [Display(Name = "LinkedIn Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? LinkedInUrl { get; set; }

        [Display(Name = "YouTube Channel")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? YouTubeUrl { get; set; }

        [Display(Name = "Facebook Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? FacebookUrl { get; set; }

        [Display(Name = "Instagram Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? InstagramUrl { get; set; }

        // Database Navigation Properties
        public virtual ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>();
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }
}
