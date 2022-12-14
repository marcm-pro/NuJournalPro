using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;
using MailKit.Net.Smtp;
using NuJournalPro.Models.ViewModels;

namespace NuJournalPro.Services
{
    public class ContactEmailSender : IContactEmailSender
    {
        private readonly MailSettings _mailSettings;
        private readonly ContactUsSettings _contactUsSettings;

        public ContactEmailSender(IOptions<MailSettings> mailSettings, IOptions<ContactUsSettings> contactUsSettings)
        {
            _mailSettings = mailSettings.Value;
            _contactUsSettings = contactUsSettings.Value;
        }

        public async Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage)
        {
            var emailTo = _contactUsSettings.SendToEmail ?? Environment.GetEnvironmentVariable("SendToEmail")!;
            await SendCustomEmailAsync(emailTo, subject, htmlMessage, emailFrom, name);
        }

        public async Task SendCustomEmailAsync(string emailTo, string subject, string htmlMessage, string? emailFrom = null, string? name = null)
        {
            var emailAccountHost = _mailSettings.Host ?? Environment.GetEnvironmentVariable("Host");
            var emailServerPort = _mailSettings.Port != 0 ? _mailSettings.Port : int.Parse(Environment.GetEnvironmentVariable("Port")!);
            var emailSenderAccount = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email");
            var emailAccountPassword = _mailSettings.Password ?? Environment.GetEnvironmentVariable("Password");

            var emailMessage = new MimeMessage()
            {
                Subject = subject,
                Body = new BodyBuilder()
                {
                    HtmlBody = htmlMessage
                }.ToMessageBody(),
            };

            if (emailFrom != null)
            {
                if (name != null)
                {
                    emailMessage.From.Add(new MailboxAddress(name, emailFrom));
                }
                else
                {
                    emailMessage.From.Add(MailboxAddress.Parse(emailFrom));
                }
                emailMessage.Sender = MailboxAddress.Parse(emailFrom);
                emailMessage.ReplyTo.Add(MailboxAddress.Parse(emailFrom));
            }
            else
            {
                emailMessage.Sender = MailboxAddress.Parse(emailSenderAccount);
            }

            foreach (var emailAddress in emailTo.Split(";"))
            {
                emailMessage.To.Add(MailboxAddress.Parse(emailAddress));
            }

            using SmtpClient smtpClient = new();
            try
            {
                await smtpClient.ConnectAsync(emailAccountHost, emailServerPort, SecureSocketOptions.Auto);
                await smtpClient.AuthenticateAsync(emailSenderAccount, emailAccountPassword);
                await smtpClient.SendAsync(emailMessage);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendCustomEmailAsync(email, subject, htmlMessage);
        }
    }
}
