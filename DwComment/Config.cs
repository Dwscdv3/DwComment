using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DwComment
{
    public class Config
    {
        public ConfigMail Mail { get; set; }
        public ConfigLimitation Limitation { get; set; }

        public MailNotification ForwardNotification { get; set; }
        public MailNotification SiteAdminNotification { get; set; }

        public void Init()
        {
            if (Mail != null)
            {
                ForwardNotification = new MailNotification(
                    new SmtpClient
                    {
                        Host = Mail.Server.Host,
                        Port = Mail.Server.Port,
                        Credentials = new NetworkCredential(
                            Mail.Credential.Username,
                            Mail.Credential.Password),
                        EnableSsl = true
                    },
                    new MailAddress(Mail.Display.Address, Mail.Display.Name)
                );

                SiteAdminNotification = new MailNotification(
                    new SmtpClient
                    {
                        Host = Mail.Server.Host,
                        Port = Mail.Server.Port,
                        Credentials = new NetworkCredential(
                            Mail.Credential.Username,
                            Mail.Credential.Password),
                        EnableSsl = true
                    },
                    new MailAddress(Mail.Display.Address, Mail.Display.Name)
                )
                {
                    To = new MailAddress(Mail.SiteAdmin.Address)
                };
            }
        }

        public class ConfigMail
        {
            public MailServer Server { get; set; }
            public MailCredential Credential { get; set; }
            public MailAddress Display { get; set; }
            public MailAddress SiteAdmin { get; set; }
            public string LinkTemplate { get; set; }

            public class MailServer
            {
                public string Host { get; set; }
                public int Port { get; set; }
            }
            public class MailCredential
            {
                public string Username { get; set; }
                public string Password { get; set; }
            }
            public class MailAddress
            {
                public string Address { get; set; }
                public string Name { get; set; }
            }
        }
        public class ConfigLimitation
        {
            public int NicknameLength { get; set; } = 80;
            public int LinkLength { get; set; } = 2000;
            public int ContentLength { get; set; } = 10000;
        }
    }
}
