
namespace NuJournalPro.Services.Interfaces
{
    public interface ISlugService
    {
        string UrlFriendly(string title);
        bool IsAvailable(string slug);
    }
}
