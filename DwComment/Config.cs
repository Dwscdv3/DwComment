using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DwComment
{
    public static class Config
    {
        public static MailNotification ForwardNotification { get; set; }
        public static MailNotification SiteAdminNotification { get; set; }
        public static string LinkTemplate { get; set; }
    }
}
