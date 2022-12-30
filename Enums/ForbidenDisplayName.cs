using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum ForbidenDisplayName
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
        Banned,
        [Description("Manager")]
        Manager,
        [Description("Support")]
        Support,
        [Description("Staff")]
        Staff,
        [Description("NuJournal")]
        NuJournal,
        [Description("Admin")]
        Admin
    }
}