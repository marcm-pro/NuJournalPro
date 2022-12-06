using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum ModerationType
    {
        [Description("Pending Review")]
        PendingReview,
        [Description("Administrative")]
        Administrative,
        [Description("Political Propaganda")]
        Political,
        [Description("Offensive Language")]
        Language,
        [Description("Drug References")]
        Drugs,
        [Description("Threatening Speech")]
        Threatening,
        [Description("Sexual Content")]
        Sexual,
        [Description("Hate Speech")]
        HateSpeech,
        [Description("Targeted Shaming")]
        Shaming
    }
}