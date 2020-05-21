using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleIdentity.UI.Entities
{
    public static class MailServer
    {
        public static int Port { get; set; } = 587;
        public static string Host { get; set; } = "smtp-mail.outlook.com";
        public static string Email { get; set; } = "bayram.eren@outlook.com.tr";
        public static string Password { get; set; } = "Bayer1991";

    }
}
