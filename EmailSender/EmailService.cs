using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using MailKit.Net.Smtp;
using System.Text;
using MimeKit;

namespace EmailSender
{
    public class EmailService
    {
        private NetworkCredential _credential;
        private string _host;
        public EmailService(string host, string user, string password)
        {
            _credential = new NetworkCredential(user,password);
            _host = host;
        }

        public void Send(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                client.Connect(_host, 25, false);
                client.Authenticate(_credential);
                client.Send(message);
            }

        }
    }
}
