using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using Org.BouncyCastle.Bcpg;

namespace EmailSender
{
    public static class Extension
    {
        private static string _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static MimeMessage UserRequestMailMessage(this MimeMessage msg, MailConfiguration configuration)
        {
            var path = _path + @"\Templates\UserRequest.html";
            var template = File.ReadAllText(path);

            msg.Subject = "[Bia.Books] Ответ на запрос";
            template = template
                .Replace("{book.headerUrl}", configuration.HeaderUrl)
                .Replace("{user.name}", configuration.UserName)
                .Replace("{book.url}", configuration.BookUrl)
                .Replace("{book.name}", configuration.BookTitle)
                .Replace("{book.footerUrl}", configuration.FooterUrl);

            msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = template
            };
            return msg;
        }

        public static MimeMessage UserRequestConfirmMessage(this MimeMessage msg, MailConfiguration configuration)
        {
            var path = _path + @"\Templates\UserRequestConfirm.html";
            var template = File.ReadAllText(path);

            msg.Subject = "[Bia.Books] Подтверждение выдачи книги";
            template = template
                .Replace("{book.headerUrl}", configuration.HeaderUrl)
                .Replace("{user.name}", configuration.UserName)
                .Replace("{book.url}", configuration.BookUrl)
                .Replace("{book.name}", configuration.BookTitle)
                .Replace("{data.date}", configuration.Date)
                .Replace("{book.btn}", configuration.BtnUrl)
                .Replace("{book.footerUrl}", configuration.FooterUrl);

            msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = template
            };
            return msg;
        }

        public static MimeMessage UserReturnBookMessage(this MimeMessage msg, MailConfiguration configuration)
        {
            var path = _path + @"\Templates\UserReturnBook.html";
            var template = File.ReadAllText(path);

            msg.Subject = "[Bia.Books] Спасибо за возращение книги";
            template = template
                .Replace("{book.headerUrl}", configuration.HeaderUrl)
                .Replace("{user.name}", configuration.UserName)
                .Replace("{book.url}", configuration.BookUrl)
                .Replace("{book.name}", configuration.BookTitle)
                .Replace("{data.date}", configuration.Date)
                .Replace("{book.btn}", configuration.BtnUrl)
                .Replace("{book.footerUrl}", configuration.FooterUrl);
            //
            msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = template
            };
            return msg;
        }

        public static MimeMessage AdminRequestMessage(this MimeMessage msg, MailConfiguration configuration)
        {
            var path = _path + @"\Templates\AdminRequest.html";
            var template = File.ReadAllText(path);

            msg.Subject = "[Bia.Books] Ответ на запрос";
            template = template
                .Replace("{book.headerUrl}", configuration.HeaderUrl)
                .Replace("{user.name}", configuration.UserName)
                .Replace("{book.url}", configuration.BookUrl)
                .Replace("{book.name}", configuration.BookTitle)
                .Replace("{book.btn}", configuration.BtnUrl)
                .Replace("{book.footerUrl}", configuration.FooterUrl);

            msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = template
            };
            return msg;
        }

        public static MimeMessage AdminMessageRenewBook(this MimeMessage msg, MailConfiguration configuration)
        {
            var path = _path + @"\Templates\AdminMessRenewBook.html";
            var template = File.ReadAllText(path);

            msg.Subject = "[Bia.Books] Продление книги";
            template = template
                .Replace("{book.headerUrl}", configuration.HeaderUrl)
                .Replace("{user.name}", configuration.UserName)
                .Replace("{book.url}", configuration.BookUrl)
                .Replace("{book.request}", configuration.BookRequestUrl)
                .Replace("{book.name}", configuration.BookTitle)
                .Replace("{book.btn}", configuration.BtnUrl)
                .Replace("{book.footerUrl}", configuration.FooterUrl)
                .Replace("{data.date}", configuration.Date);
            //
            msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = template
            };
            return msg;
        }


        public class MailConfiguration
        {
            public string UserName { get; set; }
            public string UserId { get; set; }
            public string BookTitle { get; set; }
            public int BookID { get; set; }
            public string BookUrl { get; set; }
            public string BookRequestUrl { get; set; }
            public string Date { get; set; }
            public string AdminMail { get; set; }
            public string HeaderUrl { get; set; }
            public string FooterUrl { get; set; }
            public string BtnUrl {  get; set; }
        }

    
    }
}
