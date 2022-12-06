using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum PostVisibility
    {
        [Description("Public")]
        Public,
        [Description("Private")]
        Private,
        [Description("Hidden")]
        Hidden
    }
}