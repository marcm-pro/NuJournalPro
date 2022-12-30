using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum NuJournalUserRole
    {
        [Description("Owner")]
        Owner,
        [Description("Administrator")]
        Administrator,
        [Description("Editor")]
        Editor,
        [Description("Author")]
        Author,
        [Description("Contributor")]
        Contributor,
        [Description("Moderator")]
        Moderator,
        [Description("Reader")]
        Reader,
        [Description("Restricted")]
        Restricted,
        [Description("Banned")]
        Banned
    }
}