using System.Collections.Generic;
using System.Net.Mail;

namespace Lorgus.UI.Services.Abstractions
{
    public interface IEmailSender
    {
        void Send(MailAddress to, string body, string subject, Dictionary<string, string> replacements, List<Attachment> attachments, bool isBodyHtml = true, MailPriority priority = MailPriority.Normal);
    }
}
