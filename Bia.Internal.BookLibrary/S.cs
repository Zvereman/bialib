using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Bia.Internal.BookLibrary
{
    public class S
    {
        public const string NLogConfig = "nlog.config";
        public static string MailHost;
        public static string MailBot;
        public static string MailUser;
        public static string MailPassword;

        public static void Initializate(IConfiguration configuration)
        {
            MailUser = configuration.GetSection("MailUser").Get<string>();
            MailPassword = configuration.GetSection("MailPassword").Get<string>();
            MailBot = configuration.GetSection("MailEmail").Get<string>();
            MailHost = configuration.GetSection("MailServer").Get<string>();
        }

        //prod smtp
        //public static string MailBot = "biahelper@bia-tech.ru";
        //public static string MailHost = "mail.dellin.local";
        //public static string MailUser = "mbx-biahelper";
        //public static string MailPassword = "%f1dE)tSd7J(5h%7";
        //test smtp
        //public static string MailHost = "mail.dellin.ru";
        //public static string MailBot = "books-app@bia-tech.ru";
        //public static string MailUser = "svc-books-app";
        //public static string MailPassword = "dk6Bc6eET5hfyt";

        public const int MaxBook = 5;
    }
}
