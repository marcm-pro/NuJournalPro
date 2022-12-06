using System.ComponentModel.DataAnnotations;

namespace NuJournalPro.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string? NuJournalUserId { get; set; }

        [Required]
        [Display(Name = "Tag")]
        [StringLength(64, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Text { get; set; }

        // Database Navigation Properties
        public virtual Post? Post { get; set; }
        public virtual NuJournalUser? NuJournalUser { get; set; }
        
    }
}
