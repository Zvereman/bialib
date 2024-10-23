using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bia.Internal.BookLibrary.Data;
using Bia.Internal.BookLibrary.Data.Enum;
using Bia.Internal.BookLibrary.Models;
using EmailSender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NLog;

namespace Bia.Internal.BookLibrary.Controllers
{
    [Authorize(Policy = Data.S.GroupCommon)]
    public class VisitorController : Controller
    {
        private Logger log;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _config;

        public VisitorController(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _config = configuration;
            _hostingEnvironment = hostingEnvironment;
            log = LogManager.GetCurrentClassLogger();
        }

        [Authorize(Policy = Data.S.GroupCommon)]
        public IActionResult Books([FromServices] BookDbContext ctx, string sortBy = "title", string sortOrder = "asc",
            int pageNumber = 1, int pageSize = 100, string selectedCategories = "Все", string searchQuery = "")
        {
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.SelectedCategories = selectedCategories;
            ViewBag.SearchQuery = searchQuery;

            List<Category> categories = ctx.Categories.ToList();
            List<PaperBook> books = new List<PaperBook>();
            IQueryable<PaperBook> booksQuery;
            //
            if (string.IsNullOrEmpty(searchQuery))
                booksQuery = ctx.PaperBooks.AsQueryable();
            else
                booksQuery = ctx.PaperBooks.Where(q => q.Title.Contains(searchQuery)).AsQueryable();
            //
            if (selectedCategories != "Все")
                booksQuery = booksQuery.Where(q => q.Cathegories.Any(c => c.Category.CategoryName == selectedCategories));
            //
            int totalPages = (int)Math.Ceiling((double)booksQuery.Count() / pageSize);
            ViewBag.TotalPages = totalPages;
            //
            switch (sortBy)
            {
                case "title":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(q => q.Title) : booksQuery.OrderByDescending(q => q.Title);
                    break;
                case "authors":
                    booksQuery = sortOrder == "asc" ?
                        booksQuery.OrderBy(q => q.Authors.Any() ? q.Authors.OrderBy(a => a.Author.FullName()).FirstOrDefault().Author.FullName() : "") :
                        booksQuery.OrderByDescending(q => q.Authors.Any() ? q.Authors.OrderByDescending(a => a.Author.FullName()).FirstOrDefault().Author.FullName() : "");
                    break;
                case "categories":
                    booksQuery = sortOrder == "asc" ?
                        booksQuery.OrderBy(q => q.Cathegories.Any() ? q.Cathegories.OrderBy(c => c.Category.CategoryName)
                            .FirstOrDefault().Category.CategoryName : "") :
                        booksQuery.OrderByDescending(q => q.Cathegories.Any() ? q.Cathegories
                            .OrderByDescending(c => c.Category.CategoryName).FirstOrDefault().Category.CategoryName : "");
                    break;
                case "rating":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(q => q.AverageRating) : booksQuery.OrderByDescending(q => q.AverageRating);
                    break;
                case "language":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(q => q.Language) : booksQuery.OrderByDescending(q => q.Language);
                    break;
                case "actions":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(q => q.TakenByUser != null || q.ReqestBooks.Any()) : booksQuery.OrderByDescending(q => q.TakenByUser != null || q.ReqestBooks.Any());
                    break;
                default:
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(q => q.Title) : booksQuery.OrderByDescending(q => q.Title);
                    break;
            }
            //
            books = booksQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return View("Paper", new PaperBookViewModel { PaperBooks = books, Categories = categories, TotalCount = booksQuery.Count()});
        }

        [Authorize(Policy = Data.S.GroupCommon)]
        [HttpGet]
        public IActionResult SortBooks([FromServices] BookDbContext ctx, string column, string sortOrder)
        {
            var sortedBooks = ctx.PaperBooks.OrderByDescending(b => b.Title).ToList();

            return Json(sortedBooks);
        }

        [Authorize(Policy = Data.S.GroupCommon)]
        [HttpGet]
        public IActionResult Book([FromServices] BookDbContext ctx, int id)
        {
            string currentUser = User.Identity.Name.Replace("DELLIN\\", "");
            if (string.IsNullOrEmpty(currentUser))
                return View("Error");
            string userRole = ctx.AppUsers.FirstOrDefault(u => u.LoginName == currentUser).Discriminator;
            //
            int requestAndReadBooksCount = ctx.RequestBooks
                .Where(r => r.AppUser != null && r.AppUser.LoginName == currentUser)
                .Count() + ctx.RentHistories.Where(b => b.User != null &&
                    b.User.LoginName == currentUser && b.ReturnedDate == null).Distinct().Count();
            List<RentHistory> readBooksByUser = ctx.RentHistories.Where(r => r.User != null && r.User.LoginName == currentUser
                && r.ReturnedDate == null).ToList();
            List<RequestBook> requestBooksByUser = ctx.RequestBooks.Where(r => r.AppUser != null
                && r.AppUser.LoginName == currentUser).ToList();

            DateTime? returnDate = ctx.RentHistories.FirstOrDefault(r => r.TakenBook.BiaId == id && r.ReturnedDate == null)?.ExtendedDueDate;
            //
            PaperBook book = ctx.PaperBooks.FirstOrDefault(q => q.BiaId == id);
            if (book == null)
                return View("Error");

            return View("BookDetails", new BookDetailsViewModel
            {
                Book = book,
                ReturnDate = returnDate,
                RequestAndReadBooksCount = requestAndReadBooksCount,
                ReadBooksByUser = readBooksByUser,
                RequestBooksByUser = requestBooksByUser,
                UserRole = userRole
            });
        }

        //!!!
        [Authorize(Policy = Data.S.GroupCommon)]
        public Task<JsonResult> RentBook([FromServices] BookDbContext ctx, [FromServices] EmailService email, [FromBody] int model)
        {
            OperationResult result = new OperationResult();
            try
            {
                string userLogin = User.Identity.Name.Replace("DELLIN\\", "");
                int bookId = model;
                DateTime date = DateTime.Now;
                if (string.IsNullOrEmpty(userLogin))
                    return Task.FromResult(Json(new OperationResult().SetFail()));
                if (bookId < 0)
                    return Task.FromResult(Json(new OperationResult().SetFail()));
                //
                Book book = ctx.Books.FirstOrDefault(q => q.BiaId == bookId);
                AppUser user = ctx.AppUsers.FirstOrDefault(q => q.LoginName == userLogin);
                //
                if (book == null)
                    return Task.FromResult(Json(new OperationResult().SetFail()));
                if (user == null)
                    return Task.FromResult(Json(new OperationResult().SetFail()));

                if (ctx.PaperBooks.FirstOrDefault(q => q.BiaId == bookId).TakenByUser != null)
                    return Task.FromResult(Json(new OperationResult().SetFail()));

                if (ctx.RequestBooks.Any(q => q.AppUserUid == user.Uid && q.BookId == book.BiaId))
                    return Task.FromResult(Json(new OperationResult().SetSuccess()));

                RequestBook request = new RequestBook()
                {
                    AppUserUid = user.Uid,
                    BookId = book.BiaId,
                    Date = DateTime.Now
                };
                //
                ctx.RequestBooks.Add(request);
                // отправить сообщение на почту
                // отправить сообщение админу
                var admins = ctx.AdminNotifies.Where(q => q.NotifyId == AdminNotifyTypes.General).Select(q => q.Admin);

                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    var config = new Extension.MailConfiguration()
                    {
                        HeaderUrl = $"{_config["NotifyImgUrl"]}/header_request.png",
                        BookUrl = $"{Request.Scheme}://{Request.Host}/Visitor/Book/{request.BookId}",
                        UserName = user.FirstName,
                        BookTitle = request.Book.Title,
                        FooterUrl = $"{_config["NotifyImgUrl"]}/footer_request.png",
                    };  
                    var message = new MimeMessage().UserRequestMailMessage(config);
                    message.From.Add(new MailboxAddress(S.MailBot));
                    message.To.Add(new MailboxAddress(user.Email));
                    email.Send(message);

                    config = new Extension.MailConfiguration()
                    {
                        HeaderUrl = $"{_config["NotifyImgUrl"]}/header_adminRequest.png",
                        BookUrl = $"{Request.Scheme}://{Request.Host}/Admin/Requests?sortBy=id&sortOrder=asc&pageNumber=1&pageSize=50",
                        UserName = $"{user.LastName} {user.FirstName}",
                        BookTitle = request.Book.Title,
                        FooterUrl = $"{_config["NotifyImgUrl"]}/footer_adminRequest.png",
                        BtnUrl = $"{_config["NotifyImgUrl"]}/btn_adminRequest.png",
                    };

                    if (admins.Any())
                    {
                        message = new MimeMessage().AdminRequestMessage(config);
                        message.From.Add(new MailboxAddress(S.MailBot));
                        foreach (var admin in admins)
                        {
                            message.To.Add(new MailboxAddress(admin.Email));
                        }
                        email.Send(message);
                    }
                }
                else
                {
                    if (admins.Any())
                    {
                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(S.MailBot));
                        foreach (var admin in admins)
                        {
                            message.To.Add(new MailboxAddress(admin.Email));
                        }
                        message.Subject = "[Bia.Books] Attention!";
                        message.Body = new TextPart()
                        {
                            Text = $"{user.LoginName} запросил книгу, но он не имеет почты, поэтому оповещение для него не возможно!"
                        };
                        email.Send(message);
                    }
                }
                //
                ctx.SaveChanges();
                log.Info($"{User.Identity.Name}: requested book {book.BiaId}");
                //
                return Task.FromResult(Json(new OperationResult().SetSuccess()));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Json(new OperationResult().SetFail(ex)));
            }
        }

        [Authorize(Policy = Data.S.GroupCommon)]
        public JsonResult AddReview([FromServices] BookDbContext ctx, [FromBody] AddReview review)
        {
            OperationResult result = new OperationResult();
            try
            {
                if (review == null)
                    throw new Exception("Что-то пошло не так");
                //
                AppUser user = ctx.AppUsers.FirstOrDefault(u => u.LoginName == review.Login);
                if (user == null)
                    throw new Exception("Что-то пошло не так");
                //
                Review newRev = new Review()
                {
                    Text = review.Review,
                    Rating = review.Rating == string.Empty ? 0 : int.Parse(review.Rating),
                    RatedByUid = user.Uid,
                    BookId = review.BookId
                };
                ctx.Reviews.Add(newRev);
                ctx.SaveChanges();

                List<Review> reviews = ctx.Reviews.Where(r => r.BookId == review.BookId).ToList();
                double averageRating = reviews.Average(r => r.Rating);
                int roundedRating = (int)Math.Round(averageRating);

                Book book = ctx.Books.FirstOrDefault(b => b.BiaId == review.BookId);
                if (book == null)
                    throw new Exception("Что-то пошло не так");

                book.AverageRating = roundedRating;

                ctx.SaveChanges();

                return Json(result.SetSuccess());
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }
    }
}