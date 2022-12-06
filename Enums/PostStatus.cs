using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum PostStatus
    {
        [Description("Draft")]
        Draft,
        [Description("Pending Review")]
        PendingReview,
        [Description("Published")]
        Published,
        [Description("Archived")]
        Archived
    }
}