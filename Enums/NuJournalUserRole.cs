using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum NuJournalUserRole
    {
        [Description("Administrator")]
        Administrator,
        [Description("Contributor")]
        Contributor,
        [Description("Moderator")]
        Moderator,
        [Description("Reader")]
        Reader
    }
}