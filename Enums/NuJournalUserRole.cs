using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum NuJournalUserRole
    {
        [Description("Admnistrator")]
        Admnistrator,
        [Description("Contributor")]
        Contributor,
        [Description("Moderator")]
        Moderator,
        [Description("Reader")]
        Reader
    }
}