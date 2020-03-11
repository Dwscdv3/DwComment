using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DwComment
{
    public class MailNotification
    {
        public SmtpClient SmtpClient { get; set; }
        public MailAddress From { get; set; }
        public MailAddress To { get; set; }

        public MailNotification(SmtpClient smtpClient, MailAddress from)
        {
            SmtpClient = smtpClient;
            From = from;
        }

        public async Task SendAsync(string subject, string body)
        {
            if (To != null)
            {
                await SendAsync(subject, body, To);
            }
            else
            {
                throw new Exception("To cannot be null.");
            }
        }
        public async Task SendAsync(string subject, string body, MailAddress to)
        {
            using (var msg = new MailMessage(From, to))
            {
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;
                try
                {
                    await SmtpClient.SendMailAsync(msg);
                }
                catch (SmtpException ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }
    }
}
