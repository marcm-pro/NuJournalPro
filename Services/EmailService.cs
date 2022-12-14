using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using NuJournalPro.Models;

namespace NuJournalPro.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailAccountHost = _mailSettings.Host ?? Environment.GetEnvironmentVariable("Host");
            var emailServerPort = _mailSettings.Port != 0 ? _mailSettings.Port : int.Parse(Environment.GetEnvironmentVariable("Port")!);
            var emailSenderAccount = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email");
            var emailAccountPassword = _mailSettings.Password ?? Environment.GetEnvironmentVariable("Password");
            var emailAccountReplyToName = _mailSettings.ReplyToName ?? Environment.GetEnvironmentVariable("ReplyToName");
            var emailAccountReplyToEmail = _mailSettings.ReplyToEmail ?? Environment.GetEnvironmentVariable("ReplyToEmail");

            var emailMessage = new MimeMessage
            {
                Sender = MailboxAddress.Parse(emailAccountReplyToEmail),
                Subject = subject,
                Body = new BodyBuilder()
                {
                    HtmlBody = htmlMessage
                }.ToMessageBody()
            };

            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.From.Add(new MailboxAddress(emailAccountReplyToName, emailAccountReplyToEmail));
            emailMessage.ReplyTo.Add(MailboxAddress.Parse(emailAccountReplyToEmail));

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
    }
}