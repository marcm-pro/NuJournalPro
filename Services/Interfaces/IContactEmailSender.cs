using Microsoft.AspNetCore.Identity.UI.Services;

namespace NuJournalPro.Services.Interfaces
{
    public interface IContactEmailSender : IEmailSender
    {
        Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage);
    }
}
