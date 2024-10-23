using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using Bia.Internal.BookLibrary.Data;
using Bia.Internal.BookLibrary.Data.Enum;
using EmailSender;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using static System.Net.Mime.MediaTypeNames;

namespace Bia.Internal.BookLibrary.Notification
{
    class Program
    {

        private static void Execute(string host, string mail, string mailUser, string pass, string notifyImgUrl, string connection, string libUrl)
        {
            var localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Temlate\";
            var template1 = File.ReadAllText(localPath + "template1.html");
            var template2 = File.ReadAllText(localPath + "template2.html");
            var templateReport = File.ReadAllText(localPath + "templateReport.html");

            using (var ctx = new BookDbContextFactory(connection).CreateDbContext(new string[] { connection }))
            {
                var ignoredUsers = ctx.IgnoredUsers.Select(q => q.AppUser.LoginName).ToList();
                var notifaedAdmins = ctx.AdminNotifies.Where(q => q.NotifyId == AdminNotifyTypes.General).Select(q => q.Admin);
                var mentionAdminList = notifaedAdmins.Select(q => q.Email).ToList();
                mentionAdminList.Add("sk@bia-tech.ru");
                var mentionAdmin = string.Join(";", mentionAdminList);
                //if (mentionAdmin == string.Empty)
                //    mentionAdmin = "books-app@bia-tech.ru";
                // поиск юзеров с просроченными или почти просроченными книгами
                var rents = ctx.RentHistories.Where(q => q.ReturnedDate == null).Where(q => q.ExtendedDueDate.Date < DateTime.Now.Date && !ignoredUsers.Contains(q.User.LoginName));

                var client = new EmailService(host, mailUser, pass);

                var loglist = new StringBuilder();
                // для каждого найденого юзер если есть мыло оповестить по почте 
                // если нет отослать админу уведомление о том что не возможно доставить письмо
                foreach (var rent in rents)
                {
                    try
                    {
                        var book = rent.TakenBook;
                        var user = rent.User;
                        if (!string.IsNullOrWhiteSpace(user.Email))
                        {
                            MimeMessage msg = new MimeMessage();
                            msg.From.Add(new MailboxAddress(mail));
                            msg.To.Add(new MailboxAddress(user.Email));
                            msg.Subject = "[Bia.Books] Напоминание!";
                            if (rent.ExtendedDueDate < DateTime.Now)
                            {
                                //просрочил
                                var text = template2
                                    .Replace("{book.headerUrl}", $"{notifyImgUrl}/header_OverduBook.png")
                                    .Replace("{book.footerUrl}", $"{notifyImgUrl}/footer_OverduBook.png")
                                    .Replace("{book.btnPass}", $"{notifyImgUrl}/btn_OverduBookFirst.png")
                                    .Replace("{book.btnNotPass}", $"{notifyImgUrl}/btn_OverduBookTwo.png")
                                    .Replace("{user.name}", user.FirstName)
                                    .Replace("{admin.mail}", mentionAdmin)
                                    .Replace("{book.url}", $"{libUrl}/Visitor/Book/{book.BiaId}")
                                    .Replace("{book.name}", book.Title)
                                    .Replace("{data.date}", book.TakenDue?.ToString("dd.MM.yyyy"));

                                msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                                {
                                    Text = text
                                };
                                client.Send(msg);
                                loglist.AppendLine($"Пользователь {user.FullName} задерживает возврат книги {book.Title} ({rent.ExtendedDueDate}){Environment.NewLine} <br/><br/>");
                            }
                            else
                            {
                                // еще нет

                                //var text = template1
                                //    .Replace("{user.name}", user.FullName)
                                //    .Replace("{data.date}", rent.ExtendedDueDate.ToString("dd/MM/yyyy"))
                                //    .Replace("{admin.mail}", notifaedAdmin?.Email)
                                //    .Replace("{book.id}", book.BiaId.ToString())
                                //    .Replace("{data.name}", book.Title);

                                //msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                                //{
                                //    Text = string.Format(template1, user.FullName, rent.ExtendedDueDate, book.Title)
                                //};
                                //client.Send(msg);
                                //loglist.Append($"пользователь {user.LoginName} задолжал книгу {book.Title} ({rent.ExtendedDueDate})");
                            }
                        }
                        else
                        {
                            // оповестить админа по почте
                            loglist.AppendLine($"пользователь {user.LoginName} не имеет почты{Environment.NewLine}");
                        }
                    }
                    catch (Exception e)
                    {
                        loglist.AppendLine($"при отправке пользователю произошла непредвиденная ошибка: {e.Message}{Environment.NewLine}");
                    }
                }

                // отправить лог подписанным админам
                var log = loglist.ToString();
                var currentTime = DateTime.Now;

                if (loglist.Length > 0)
                {
                    MimeMessage msg = new MimeMessage();
                    msg.From.Add(new MailboxAddress(mail));
                    foreach (var notifaedAdmin in notifaedAdmins)
                    {
                        msg.To.Add(new MailboxAddress(notifaedAdmin.Email));
                    }
                    msg.Subject = $"[Bia.Books automatic notification] Отчет по задолжникам на {currentTime}";
                    var text = templateReport
                                    .Replace("{book.headerUrl}", $"{notifyImgUrl}/header_userMustBook.png")
                                    .Replace("{book.footerUrl}", $"{notifyImgUrl}/footer_userMustBook.png")
                                    .Replace("{book.btnBook}", $"{notifyImgUrl}/btn_userMustBook.png")
                                    .Replace("{book.text}", log)
                                    .Replace("{book.btn}", $"{libUrl}/Admin/TakenBooks?sortBy=id&sortOrder=asc&pageNumber=1&pageSize=50");

                    msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = text
                    };
                    client.Send(msg);
                }
            }
        }
        static void Main(string[] args)
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                return;
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                var config = builder.Build();

                NLog.LogManager.GetCurrentClassLogger().Info("initilazed");
                NLog.LogManager.GetCurrentClassLogger().Trace($"directory {Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                var connection = config.GetValue<string>("ConnectionStrings:dbConnection");

                var host = config.GetSection("MailServer").Get<string>();
                var mail = config.GetSection("MailEmail").Get<string>();
                var mailUser = config.GetSection("MailUser").Get<string>();
                var mailPass = config.GetSection("MailPassword").Get<string>();
                var notifyImgUrl = config.GetSection("NotifyImgUrl").Get<string>();
                var libUrl = config.GetSection("LibraryUrl").Get<string>();
                // obsolete
                //var list = config.GetSection("Ignore").Get<List<string>>();
                //if(list==null) list= new List<string>();
                NLog.LogManager.GetCurrentClassLogger().Trace($"{host} {mail} {connection}");
                NLog.LogManager.GetCurrentClassLogger().Trace($" {connection}");
                NLog.LogManager.GetCurrentClassLogger().Trace("settings seted");

                Execute(host, mail, mailUser, mailPass, notifyImgUrl, connection, libUrl);

                NLog.LogManager.GetCurrentClassLogger().Info("succesful completed");
            }
            catch (Exception e)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(e);
            }
        }
    }
}
