using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum ForbidenDisplayName
    {
        [Description("Admin")]
        Admin,
        [Description("Administrator")]
        Administrator,
        [Description("Moderator")]
        Moderator,
        [Description("Contributor")]
        Contributor,
        [Description("Manager")]
        Manager,
        [Description("Support")]
        Support,
        [Description("Staff")]
        Staff,
        [Description("NuJournal")]
        NuJournal,
        [Description("NuJournalPro")]
        NuJournalPro
    }
}
