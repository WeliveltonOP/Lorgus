using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Lorgus.UI.Services.Abstractions;
using Lorgus.UI.Models;

namespace HILC.UI.Services
{
    public class EmailSender : IEmailSender
    {
        private EmailSettings _emailSettings { get; }

        public EmailSender(IOptions<LorgusConfig> config)
        {
            _emailSettings = config.Value.EmailSettings;
        }

        public void Send(MailAddress to, string body, string subject, Dictionary<string, string> replacements, List<Attachment> attachments, bool isBodyHtml = true, MailPriority priority = MailPriority.Normal)
        {
            using var client = new SmtpClient()
            {
                Host = _emailSettings.Host,
                Port = _emailSettings.Port,
                DeliveryMethod = (SmtpDeliveryMethod)Enum.Parse(typeof(SmtpDeliveryMethod), _emailSettings.DeliveryMethod),
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
            };

            var mail = new MailMessage
            {
                Subject = subject,
                Body = ReplaceBodyContentKeys(body, replacements),
                IsBodyHtml = isBodyHtml,
                Priority = priority
            };

            foreach (var attachment in attachments)
            {
                mail.Attachments.Add(attachment);
            }

            mail.From = new MailAddress(_emailSettings.From, _emailSettings.DisplayName);

            mail.To.Add(to);

            client.Timeout = Convert.ToInt32(TimeSpan.FromMinutes(5).TotalMilliseconds);

            client.Send(mail);
        }

        private static string ReplaceBodyContentKeys(string bodyContent, Dictionary<string, string> replacements)
        {
            if (replacements is null)
                return bodyContent;

            foreach (var key in replacements.Keys)
            {
                bodyContent = bodyContent.Replace(key, replacements[key]);
            }

            return bodyContent;
        }
    }
}
