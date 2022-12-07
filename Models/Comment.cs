using System.ComponentModel.DataAnnotations;
using NuJournalPro.Enums;

namespace NuJournalPro.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string? NuJournalUserId { get; set; }
        public string? ModeratorUserId { get; set; }

        [Required]
        [Display(Name = "Comment")]
        [StringLength(8192, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string Body { get; set; } = string.Empty;

        [Display(Name = "Created")]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Edited { get; set; }
        public DateTime? Moderated { get; set; }
        public DateTime? Deleted { get; set; }

        [Display(Name = "Moderated")]
        [StringLength(8192, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? ModeratedBody { get; set; }

        public ModerationType ModerationType { get; set; } = ModerationType.PendingReview;

        // Database Navigation Properties
        public virtual Post? Post { get; set; }
        public virtual NuJournalUser? NuJournalUser { get; set; }
        public virtual NuJournalUser? ModeratorUser { get; set; }
    }
}