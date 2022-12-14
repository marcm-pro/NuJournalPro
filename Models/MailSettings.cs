﻿namespace NuJournalPro.Models
{
    public class MailSettings
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? DisplayName { get; set; }
        public string? ReplyToName { get; set; }
        public string? ReplyToEmail { get; set; }
    }
}