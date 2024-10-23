using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bia.Internal.BookLibrary.Data;
using Bia.Internal.BookLibrary.Data.Enum;
using Bia.Internal.BookLibrary.Models;
using EmailSender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NLog;

namespace Bia.Internal.BookLibrary.Controllers
{
    [Authorize(Policy = Data.S.GroupCommon)]
    public class AccountController : Controller
    {
        private Logger log;
        private readonly IConfiguration _config;

        public AccountController(IConfiguration configuration)
        {
            _config = configuration;
            log = LogManager.GetCurrentClassLogger();
        }

        [Authorize(Policy = Data.S.GroupCommon)]
        public IActionResult MyRequest([FromServices] BookDbContext ctx, string sortBy = "id", string sortOrder = "asc",
            string selectedRequest = "Все", int pageNumber = 1, int pageSize = 50)
        {
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SelectedRequest = selectedRequest;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            //
            MyRequestViewModal myRequestViewModal = new MyRequestViewModal();
            string user = User.Identity.Name.Replace("DELLIN\\", "");
            AppUser userReading = ctx.AppUsers.First(q => q.LoginName == user);
            myRequestViewModal.MyRequestBooks = new Dictionary<string, List<PaperBook>>();
            List<PaperBook> allBooks = new List<PaperBook>();

            switch (selectedRequest)
            {
                case "Запросил":
                    allBooks = ctx.RequestBooks
                        .Where(q => q.AppUser.LoginName == user)
                        .Select(q => (PaperBook)q.Book)
                        .ToList();
                    break;
                case "Читаю":
                    allBooks = ctx.PaperBooks
                        .Where(q => q.TakenByUser == userReading)
                        .ToList();
                    break;
                case "Вернул":
                    allBooks = ctx.RentHistories
                        .Where(q => q.User.LoginName == user && q.ReturnedDate != null)
                        .Select(q => q.TakenBook)
                        //.Distinct()
                        .ToList();
                    break;
                default:
                    var readBooks = ctx.RentHistories
                        .Where(q => q.User.LoginName == user && q.ReturnedDate != null)
                        .Select(q => q.TakenBook)
                        //.Distinct()
                        .ToList();
                    var readingBooks = ctx.PaperBooks
                        .Where(q => q.TakenByUser == userReading)
                        .ToList();
                    var requestBooks = ctx.RequestBooks
                        .Where(q => q.AppUser.LoginName == user)
                        .Select(q => (PaperBook)q.Book)
                        .ToList();

                    allBooks.AddRange(readBooks);
                    allBooks.AddRange(readingBooks);
                    allBooks.AddRange(requestBooks);
                    break;
            }

            var paginatedBooks = ApplyPagination(allBooks, pageNumber, pageSize);
            myRequestViewModal.CountMyRequestsBooks = paginatedBooks.Distinct().Count();

            foreach (var book in paginatedBooks.Distinct())
            {
                string statusKey = GetStatusKey(ctx, book, userReading, user);
                if (!myRequestViewModal.MyRequestBooks.ContainsKey(statusKey))
                {
                    myRequestViewModal.MyRequestBooks[statusKey] = new List<PaperBook>();
                }
                myRequestViewModal.MyRequestBooks[statusKey].Add(book);
            }

            int totalPages = (int)Math.Ceiling((double)allBooks.Count / pageSize);
            ViewBag.TotalPages = totalPages;

            // Сортируем книги внутри каждого статуса
            switch (sortBy)
            {
                case "id":
                    myRequestViewModal.MyRequestBooks = sortOrder == "asc" ?
                        myRequestViewModal.MyRequestBooks.OrderBy(kv => kv.Value.FirstOrDefault()?.BiaId).ToDictionary(kv => kv.Key, kv => kv.Value) :
                        myRequestViewModal.MyRequestBooks.OrderByDescending(kv => kv.Value.FirstOrDefault()?.BiaId).ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
                case "name":
                    myRequestViewModal.MyRequestBooks = sortOrder == "asc" ?
                        myRequestViewModal.MyRequestBooks.OrderBy(kv => kv.Value.FirstOrDefault()?.Title).ToDictionary(kv => kv.Key, kv => kv.Value) :
                        myRequestViewModal.MyRequestBooks.OrderByDescending(kv => kv.Value.FirstOrDefault()?.Title).ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
                case "return":
                    myRequestViewModal.MyRequestBooks = sortOrder == "asc" ?
                        myRequestViewModal.MyRequestBooks.OrderBy(kv => kv.Value.FirstOrDefault()?.TakenDue).ToDictionary(kv => kv.Key, kv => kv.Value) :
                        myRequestViewModal.MyRequestBooks.OrderByDescending(kv => kv.Value.FirstOrDefault()?.TakenDue).ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
                case "status":
                    myRequestViewModal.MyRequestBooks = sortOrder == "asc" ?
                        myRequestViewModal.MyRequestBooks.OrderBy(kv => GetStatus(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value) :
                        myRequestViewModal.MyRequestBooks.OrderByDescending(kv => GetStatus(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
                case "actions":
                    myRequestViewModal.MyRequestBooks = sortOrder == "asc" ?
                        myRequestViewModal.MyRequestBooks.OrderBy(kv => GetAction(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value) :
                        myRequestViewModal.MyRequestBooks.OrderByDescending(kv => GetAction(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
                default:
                    myRequestViewModal.MyRequestBooks = sortOrder == "asc" ?
                       myRequestViewModal.MyRequestBooks.OrderBy(kv => kv.Value.FirstOrDefault()?.BiaId).ToDictionary(kv => kv.Key, kv => kv.Value) :
                       myRequestViewModal.MyRequestBooks.OrderByDescending(kv => kv.Value.FirstOrDefault()?.BiaId).ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
            }

            return View("MyRequest", myRequestViewModal);
        }

        private List<PaperBook> ApplyPagination(List<PaperBook> query, int pageNumber, int pageSize)
        {
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        private string GetStatusKey(BookDbContext ctx, PaperBook book, AppUser userReading, string user)
        {
            if (book.TakenByUser == userReading)
            {
                return "reading";
            }
            else if (ctx.RequestBooks.Any(q => q.AppUser.LoginName == user && q.Book.BiaId == book.BiaId))
            {
                return "request";
            }
            else if (ctx.RentHistories.Any(q => q.User.LoginName == user && q.TakenBook.BiaId == book.BiaId && q.ReturnedDate != null))
            {
                return "read";
            }
            else
            {
                return "";
            }
        }

        private string GetStatus(string key)
        {
            switch (key)
            {
                case "read":
                    return "Вернул";
                case "reading":
                    return "Читаю";
                case "request":
                    return "Запросил";
                default:
                    return "";
            }
        }

        private string GetAction(string key)
        {
            switch (key)
            {
                case "read":
                    return "Оставить отзыв и оценить книгу";
                case "reading":
                    return "Продлить книгу";
                case "request":
                    return "Отменить запрос";
                default:
                    return "";
            }
        }

        [Authorize(Policy = Data.S.GroupCommon)]
        public JsonResult DeleteRequest([FromServices] BookDbContext ctx, [FromBody] int bookId = -1)
        {
            OperationResult result = new OperationResult();
            //
            try
            {
                if(bookId == -1) 
                    return Json(result.SetFail());
                //
                RequestBook requestBook = ctx.RequestBooks.FirstOrDefault(b => b.BookId == bookId);
                if (requestBook != null)
                {
                    ctx.RequestBooks.Remove(requestBook);
                    ctx.SaveChanges();
                    return Json(result.SetSuccess());
                }
                else
                {
                    return Json(result.SetFail());
                }
            }
            catch(Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }

        [Authorize(Policy = Data.S.GroupCommon)]
        public JsonResult RenewBook ([FromServices] BookDbContext ctx, [FromServices] EmailService email, [FromBody] int bookId = -1)
        {
            OperationResult result = new OperationResult();
            try
            {
                RentHistory bookRent = ctx.RentHistories.FirstOrDefault(b => b.TakenBook.BiaId == bookId && b.ReturnedDate == null);
                if (bookRent != null && bookRent.ExtendedTimesCount < 1)
                {
                    string userLogin = User.Identity.Name.Replace("DELLIN\\", "");
                    AppUser user = ctx.AppUsers.FirstOrDefault(u => u.LoginName == userLogin);
                    //
                    bookRent.ExtendedTimesCount++;
                    DateTime duoDate = bookRent.ExtendedDueDate.AddDays(30);
                    bookRent.ExtendedDueDate = duoDate;
                    bookRent.TakenBook.TakenDue = duoDate;
                    //
                    var admins = ctx.AdminNotifies.Where(q => q.NotifyId == AdminNotifyTypes.General).Select(q => q.Admin);
                    //
                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        var config = new Extension.MailConfiguration()
                        {
                            HeaderUrl = $"{_config["NotifyImgUrl"]}/header_adminReNewBook.png",
                            BookUrl = $"{Request.Scheme}://{Request.Host}/Visitor/Book/{bookRent.TakenBook.BiaId}",
                            UserName = $"{user.LastName} {user.FirstName}",
                            BookTitle = bookRent.TakenBook.Title,
                            FooterUrl = $"{_config["NotifyImgUrl"]}/footer_adminReNewBook.png",
                            BtnUrl = $"{_config["NotifyImgUrl"]}/btn_adminReNewBook.png",
                            BookRequestUrl = $"{Request.Scheme}://{Request.Host}/Admin/TakenBooks?sortBy=id&sortOrder=asc&pageNumber=1&pageSize=50",
                            Date = bookRent.ExtendedDueDate.ToString("dd.MM.yyyy")
                        };

                        if (admins.Any())
                        {
                            var message = new MimeMessage().AdminMessageRenewBook(config);
                            message.From.Add(new MailboxAddress(S.MailBot));
                            foreach (var admin in admins)
                            {
                                message.To.Add(new MailboxAddress(admin.Email));
                            }
                            email.Send(message);
                        }
                    }
                    //
                    ctx.SaveChanges();
                    //
                    return Json(result.SetSuccess());
                }
                else
                {
                    throw new Exception("Книгу уже продлялась");
                }
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }

        [Authorize(Policy = Data.S.GroupCommon)]
        public JsonResult AddReview([FromServices] BookDbContext ctx, [FromBody] AddReviewModel model)
        {
            var result = new OperationResult();
            try
            {
                var book = ctx.Books.FirstOrDefault(q => q.BiaId == model.BookId);
                var user = ctx.AppUsers.FirstOrDefault(q => q.LoginName == User.Identity.Name.Replace("DELLIN\\", ""));

                if (book == null)
                {
                    return Json(result.SetFail("не найдена книга"));
                }

                if (user == null)
                {
                    return Json(result.SetFail("не найден пользователь"));
                }

                if (ctx.Reviews.Any(q => q.BookId == book.BiaId && q.RatedByUid == user.Uid))
                {
                    return Json(result.SetFail("вы уже остваляли отзыв"));
                }

                var review = new Review()
                {
                    Text = model.Text,
                    Rating = model.Rating,
                    RatedByUid = user.Uid,
                    BookId = book.BiaId
                };

                ctx.Reviews.Add(review);
                ctx.SaveChanges();
                book.AverageRating = book.SumCurrentRating();
                ctx.SaveChanges();

                return Json(result.SetSuccess("отзыв отправлен"));
            }
            catch (Exception e)
            {
                return Json(result.SetFail(e));
            }
        }
    }
}