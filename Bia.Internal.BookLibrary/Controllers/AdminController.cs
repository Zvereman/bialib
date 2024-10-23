using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Bia.Internal.BookLibrary.Data;
using Bia.Internal.BookLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Wangkanai.Detection;
using EmailSender;
using Newtonsoft.Json;
using NLog;
using Bia.Internal.BookLibrary.Data.Enum;
using MimeKit;
using AppUser = Bia.Internal.BookLibrary.Data.AppUser;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.AspNetCore.Http;

namespace Bia.Internal.BookLibrary.Controllers
{
    [Authorize(Policy = Data.S.GroupAdmin)]
    public class AdminController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _config;
        private readonly IBrowser _browser;
        private Logger log;
        public AdminController(IConfiguration configuration, IHostingEnvironment hostingEnvironment, IBrowserResolver browserResolver)
        {
            log = NLog.LogManager.GetCurrentClassLogger();
            _config = configuration;
            _hostingEnvironment = hostingEnvironment;
            _browser = browserResolver.Browser;
        }

        #region Users
        public IActionResult Users([FromServices] BookDbContext ctx, string sortBy = "title", string sortOrder = "asc",
            int pageNumber = 1, int pageSize = 50)
        {
            //!!!
            //Костыль для загрузки списка игнорируемых пользователей
            //Подумать как поправить если будут меняться еще какието объекты в моделях БД
            var ignored = ctx.IgnoredUsers.ToList();
            //
            UsersViewModel usersViewModel = new UsersViewModel();
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            usersViewModel.TotalCount = ctx.AppUsers.Count();
            int totalPages = (int)Math.Ceiling((double)usersViewModel.TotalCount / pageSize);
            ViewBag.TotalPages = totalPages;
            //
            List<AppUser> users = ctx.AppUsers.ToList();
            var adminNotifyUids = ctx.AdminNotifies.Select(an => an.Uid).ToHashSet();
            //
            switch (sortBy)
            {
                case "title":
                    users = sortOrder == "asc" ? users.OrderBy(q => q.FullName).ToList() :
                        users.OrderByDescending(q => q.FullName).ToList();
                    break;
                case "email":
                    users = sortOrder == "asc" ? users.OrderBy(q => q.Email).ToList() :
                        users.OrderByDescending(q => q.Email).ToList();
                    break;
                case "isadmin":
                    users = sortOrder == "asc" ? users.OrderBy(q => q.AccessGroup).ToList() :
                       users.OrderByDescending(q => q.AccessGroup).ToList();
                    break;
                case "isnotification":
                    users = sortOrder == "asc" ?
                        users.OrderBy(q => !adminNotifyUids.Contains(q.Uid)).ToList() :
                        users.OrderBy(q => adminNotifyUids.Contains(q.Uid)).ToList();
                    break;
                case "isignored":
                    users = sortOrder == "asc" ?
                        users.OrderBy(q => q.Ignored != null).ToList() :
                        users.OrderByDescending(q => q.Ignored != null).ToList();
                    break;
                case "actions":
                    users = sortOrder == "asc" ? users.OrderBy(q => q.FullName).ToList() :
                        users.OrderByDescending(q => q.FullName).ToList();
                    break;
                default:
                    users = sortOrder == "asc" ? users.OrderBy(q => q.FullName).ToList() :
                        users.OrderByDescending(q => q.FullName).ToList();
                    break;
            }
            //
            users = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            usersViewModel.Users = users;
            return View("Users", usersViewModel);
        }

        public JsonResult AddOrDelToAdminList([FromServices] BookDbContext ctx, [FromBody] UserForAddToAdminList addUser)
        {
            OperationResult result = new OperationResult();
            //
            try
            {
                if (addUser.UserUid == null)
                    throw new Exception("Ожидалось значение (Guid)userUid");
                //
                AppUser user = ctx.AppUsers.FirstOrDefault(u => u.Uid == addUser.UserUid);
                if (user == null)
                    throw new Exception("Не удалось найти пользователя");
                //
                if (addUser.IsAdmin == true)
                {
                    user.AccessGroup = AccessGroupId.Common;
                    user.Discriminator = "AppUser";
                }
                else
                {
                    user.AccessGroup = AccessGroupId.Admin;
                    user.Discriminator = "Admin";
                }
                //
                ctx.SaveChanges();
                //

                return Json(new { status = result.SetSuccess().Status, fio = user.FullName }); ;
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }
        public JsonResult AddOrDelToIgnoreList([FromServices] BookDbContext ctx, [FromBody] Guid userUid)
        {
            OperationResult result = new OperationResult();
            try
            {
                AppUser user = ctx.AppUsers.FirstOrDefault(q => q.Uid == userUid);
                string actionIgnored = string.Empty;
                if (user == null)
                    throw new Exception("Не удалось найти пользователя");
                //
                IgnoredUser ignoredUser = ctx.IgnoredUsers.FirstOrDefault(i => i.AppUserUid == userUid);
                if (ignoredUser == null)
                {
                    ctx.IgnoredUsers.Add(new IgnoredUser() { AppUserUid = user.Uid });
                    actionIgnored = "добавлен в игнор лист";
                }
                else
                {
                    ctx.IgnoredUsers.Remove(ignoredUser);
                    actionIgnored = "удален из игнор листа";
                }
                //
                ctx.SaveChanges();
                //log.Info($"{User.Identity.Name}: add user {user.FullName} to ignore list");
                return Json(new { status = result.SetSuccess().Status, fio = user.FullName, action = actionIgnored });
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }

        public JsonResult AddOrDelNotify([FromServices] BookDbContext ctx, [FromBody] Guid userUid)
        {
            OperationResult result = new OperationResult();
            try
            {
                //AppUser user = ctx.UserNotifies
                string actionNotify = string.Empty;
                Admin admin = ctx.Admins.FirstOrDefault(q => q.Uid == userUid);
                if (admin == null)
                    throw new Exception("Не удалось найти администратора");
                //
                AdminNotify notify = ctx.AdminNotifies.FirstOrDefault(q => q.Uid == admin.Uid);
                //
                if (admin.AdminNotifies.Any(q => q.NotifyId == AdminNotifyTypes.General) == false)
                {
                    ctx.AdminNotifies.Add(new AdminNotify()
                    {
                        Uid = admin.Uid,
                        NotifyId = AdminNotifyTypes.General
                    });
                    actionNotify = "включены";
                    //ctx.IgnoredUsers.Remove(new IgnoredUser { AppUserUid = admin.Uid});
                    //log.Info($"{User.Identity.Name}: add admin {admin.FullName} to notify list");
                }
                else if (notify != null)
                {
                    ctx.AdminNotifies.Remove(notify);
                    actionNotify = "выключены";
                }
                //
                ctx.SaveChanges();
                //
                return Json(new { status = result.SetSuccess().Status, fio = admin.FullName, action = actionNotify });
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }

        public IActionResult ExportUsersToExcel([FromServices] BookDbContext ctx)
        {
            UsersViewModel users = new UsersViewModel();
            users.Users = ctx.AppUsers.ToList();
            //
            return users.ExportToExcel(users.Users);
        }

        #region Action Users

        /*
        public JsonResult DeleteUser([FromServices] BookDbContext ctx, [FromBody] Guid userUid)
        {
            OperationResult result = new OperationResult();
            try
            {
                AppUser user = ctx.AppUsers.First(q => q.Uid == userUid);

                if (user == null)
                    throw new Exception("Не удалось найти пользователя");
                //
                if (user.ReqestBooks.Any())
                {
                    ctx.RequestBooks.RemoveRange(user.ReqestBooks);
                }

                if (user.RentHistory.Any())
                {
                    ctx.RentHistories.RemoveRange(user.RentHistory);
                }

                if (user.Reviews.Any())
                {
                    ctx.Reviews.RemoveRange(user.Reviews);
                }

                if (user is Admin)
                {
                    Admin admin = user as Admin;
                    if (admin.AdminNotifies.Any())
                    {
                        ctx.AdminNotifies.RemoveRange(admin.AdminNotifies);
                    }
                }

                ctx.AppUsers.Remove(user);
                ctx.SaveChanges();
                //log.Info($"{User.Identity.Name}: remove user {user.FullName}");

                return Json( new { status = result.SetSuccess().Status, fio = user.FullName });
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }

        //public IActionResult NewOrEditUser([FromServices] BookDbContext ctx, Guid userUid)
        //{
        //    return View("NewOrEditUser");
        //}

        public JsonResult AddUser([FromServices] BookDbContext ctx, [FromBody] dynamic model)
        {
            OperationResult result = new OperationResult();
            try
            {
                var login = (string)model.login;
                var first = (string)model.first;
                var last = (string)model.last;
                var full = (string)model.full;
                var email = (string)model.email;
                bool isAdmin = (bool)model.admin;

                if (string.IsNullOrWhiteSpace(login) || ctx.AppUsers.Any(q => q.LoginName == login))
                {
                    return Json(result.SetFail("не получилось добавить пользователя."));
                }

                if (isAdmin)
                {
                    var user = new Admin();
                    user.LoginName = login;
                    user.FirstName = first;
                    user.LastName = last;
                    user.FullName = full;
                    user.Email = email;
                    //user.AccessGroup = AccessGroupId.Common | AccessGroupId.Admin;
                    user.AccessGroup = AccessGroupId.Admin;
                    user.IsActive = true;
                    ctx.Admins.Add(user);
                    ctx.SaveChanges();
                }
                else
                {
                    var user = new AppUser();
                    user.LoginName = login;
                    user.FirstName = first;
                    user.LastName = last;
                    user.FullName = full;
                    user.Email = email;
                    user.AccessGroup = AccessGroupId.Common;
                    user.IsActive = true;
                    ctx.AppUsers.Add(user);
                    ctx.SaveChanges();
                }
                log.Info($"{User.Identity.Name}: add user {login}");

                return Json(result.SetSuccess($"Пользователь {login} добавлен"));
            }
            catch (Exception e)
            {
                return Json(result.SetFail("Не получилось добавить пользователя."));
            }
        }

        public JsonResult EditUser([FromServices] BookDbContext ctx, [FromBody] dynamic model)
        {
            OperationResult result = new OperationResult();
            try
            {
                var login = (string)model.login;
                var first = (string)model.first;
                var last = (string)model.last;
                var full = (string)model.full;
                var email = (string)model.email;
                bool isAdmin = (bool)model.admin;

                AppUser user = ctx.AppUsers.First(q => q.LoginName == login);
                if (!string.IsNullOrWhiteSpace(first))
                    user.FirstName = first;
                if (!string.IsNullOrWhiteSpace(last))
                    user.LastName = last;
                if (!string.IsNullOrWhiteSpace(full))
                    user.FullName = full;
                if (!string.IsNullOrWhiteSpace(email))
                    user.Email = email;
                if (isAdmin)
                {
                    //user.AccessGroup = AccessGroupId.Common | AccessGroupId.Admin;
                    user.AccessGroup = AccessGroupId.Admin;
                    user.Discriminator = "Admin";
                }
                else
                {
                    user.AccessGroup = AccessGroupId.Common;
                    user.Discriminator = "AppUser";
                }

                ctx.SaveChanges();
                log.Info($"{User.Identity.Name}: edit user {user.FullName}");

                return Json(result.SetSuccess($"Пользователь {user.FullName} изменен"));
            }
            catch (Exception e)
            {
                return Json(result.SetFail("Не получилось изменить пользователя."));
            }
        }
        */

        #endregion Action Users

        #endregion Users

        #region Reviews

        public IActionResult Reviews([FromServices] BookDbContext ctx, string sortBy = "title", string sortOrder = "asc",
            int pageNumber = 1, int pageSize = 50)
        {
            ReviewsViewModel reviewsViewModel = new ReviewsViewModel();
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            reviewsViewModel.TotalCount = ctx.Reviews.Count();
            int totalPages = (int)Math.Ceiling((double)reviewsViewModel.TotalCount / pageSize);
            ViewBag.TotalPages = totalPages;
            //
            List<Review> reviews = ctx.Reviews.ToList();
            //
            switch (sortBy)
            {
                case "title":
                    reviews = sortOrder == "asc" ? reviews.OrderBy(q => q.Book.Title).ToList() :
                        reviews.OrderByDescending(q => q.Book.Title).ToList();
                    break;
                case "authors":
                    reviews = sortOrder == "asc" ?
                        reviews.OrderBy(q => q.Book.Authors.Any() ? q.Book.Authors.OrderBy(a => a.Author.FullName()).FirstOrDefault().Author.FullName() : "").ToList() :
                        reviews.OrderByDescending(q => q.Book.Authors.Any() ? q.Book.Authors.OrderByDescending(a => a.Author.FullName()).FirstOrDefault().Author.FullName() : "").ToList();
                    break;
                case "review":
                    reviews = sortOrder == "asc" ? reviews.OrderBy(q => q.Text).ToList() :
                       reviews.OrderByDescending(q => q.Text).ToList();
                    break;
                case "rating":
                    reviews = sortOrder == "asc" ?
                        reviews.OrderBy(q => q.Rating).ToList() :
                        reviews.OrderByDescending(q => q.Rating).ToList();
                    break;
                default:
                    reviews = sortOrder == "asc" ? reviews.OrderBy(q => q.Book.Title).ToList() :
                       reviews.OrderByDescending(q => q.Book.Title).ToList();
                    break;
            }
            //
            reviews = reviews.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            reviewsViewModel.Reviews = reviews;
            //
            return View("Reviews", reviewsViewModel);
        }

        public JsonResult DeleteReview([FromServices] BookDbContext ctx, [FromBody] Guid uid)
        {
            OperationResult result = new OperationResult();
            try
            {
                Review review = ctx.Reviews.FirstOrDefault(q => q.Uid == uid);
                if (review == null)
                    return Json(new OperationResult().SetFail());
                //
                Book book = review.Book;
                ctx.Reviews.Remove(review);
                ctx.SaveChanges();
                book.AverageRating = review.Book.SumCurrentRating();
                ctx.SaveChanges();
                log.Info($"{User.Identity.Name}: delete review {book.BiaId}");
                return Json(result.SetSuccess("отзыв удален"));
            }
            catch (Exception e)
            {
                log.Error(e);
                return Json(new OperationResult().SetFail(e));
            }
        }

        public IActionResult ExportReviewsToExcel([FromServices] BookDbContext ctx)
        {
            ReviewsViewModel reviews = new ReviewsViewModel();
            reviews.Reviews = ctx.Reviews.ToList();
            //
            return reviews.ExportToExcel(reviews.Reviews);
        }

        #endregion Reviews

        #region Manage

        public IActionResult Manage([FromServices] BookDbContext ctx, string sortBy = "title", string sortOrder = "asc",
            int pageNumber = 1, int pageSize = 50)
        {
            ManageViewModel manageViewModel = new ManageViewModel();
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            manageViewModel.TotalCount = ctx.PaperBooks.Count();
            int totalPages = (int)Math.Ceiling((double)manageViewModel.TotalCount / pageSize);
            ViewBag.TotalPages = totalPages;
            //
            List<PaperBook> books = ctx.PaperBooks.ToList();
            //
            switch (sortBy)
            {
                case "title":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.Title).ToList() : books.OrderByDescending(q => q.Title).ToList();
                    break;
                case "authors":
                    books = sortOrder == "asc" ?
                        books.OrderBy(q => q.Authors.Any() ? q.Authors.OrderBy(a => a.Author.FullName()).FirstOrDefault().Author.FullName() : "").ToList() :
                        books.OrderByDescending(q => q.Authors.Any() ? q.Authors.OrderByDescending(a => a.Author.FullName()).FirstOrDefault().Author.FullName() : "").ToList();
                    break;
                case "categories":
                    books = sortOrder == "asc" ?
                        books.OrderBy(q => q.Cathegories.Any() ? q.Cathegories.OrderBy(c => c.Category.CategoryName)
                            .FirstOrDefault().Category.CategoryName : "").ToList() :
                        books.OrderByDescending(q => q.Cathegories.Any() ? q.Cathegories
                            .OrderByDescending(c => c.Category.CategoryName).FirstOrDefault().Category.CategoryName : "").ToList();
                    break;
                case "year":
                    books = sortOrder == "asc" ?
                        books.OrderBy(q => q.Year).ToList() :
                        books.OrderByDescending(q => q.Year).ToList();
                    break;
                case "pages":
                    books = sortOrder == "asc" ?
                        books.OrderBy(q => q.Pages).ToList() :
                        books.OrderByDescending(q => q.Pages).ToList();
                    break;
                case "rating":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.AverageRating).ToList() :
                        books.OrderByDescending(q => q.AverageRating).ToList();
                    break;
                case "reviev":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.Reviews.FirstOrDefault() == null ? 0 : q.Reviews.FirstOrDefault().Rating).ToList() :
                         books.OrderByDescending(q => q.Reviews.FirstOrDefault() == null ? 0 : q.Reviews.FirstOrDefault().Rating).ToList();
                    break;
                case "language":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.Language).ToList() :
                        books.OrderByDescending(q => q.Language).ToList();
                    break;
                case "actions":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.Title).ToList() :
                        books.OrderByDescending(q => q.Title).ToList();
                    break;
                default:
                    books = sortOrder == "asc" ? books.OrderBy(q => q.Title).ToList() : books.OrderByDescending(q => q.Title).ToList();
                    break;
            }
            //
            books = books.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            manageViewModel.PaperBooks = books;
            manageViewModel.AppUsers = ctx.AppUsers.ToList();
            //
            return View("Manage", manageViewModel);
        }

        public JsonResult DeleteBook([FromServices] BookDbContext ctx, [FromBody] int bookId = -1)
        {
            OperationResult result = new OperationResult();
            //
            try
            {
                if (bookId == -1)
                    return Json(result.SetFail());
                //
                PaperBook book = ctx.PaperBooks.FirstOrDefault(b => b.BiaId == bookId);
                if (book == null)
                    throw new Exception("Неудалось найти книгу для удаления");
                //
                if (book.TakenByUser != null)
                    throw new Exception($"Невозможно удалить книгу. Книга взята {book.TakenByUser.FullName}");

                var rentbook = ctx.RentHistories.Where(q => q.TakenBook == book);
                if (rentbook.Any())
                    ctx.RentHistories.RemoveRange(rentbook);
                //
                ctx.PaperBooks.Remove(book);

                ctx.SaveChanges();
                //
                return Json(result.SetSuccess());
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }

        public IActionResult NewOrEditBook([FromServices] BookDbContext ctx, int bookId)
        {
            string currentUser = User.Identity.Name.Replace("DELLIN\\", "");
            NewOrEditBookViewModel newOrEdit = new NewOrEditBookViewModel();
            if (bookId != 0)
            {
                Book book = ctx.Books.FirstOrDefault(b => b.BiaId == bookId);
                if (book == null)
                    throw new Exception();
                //
                newOrEdit.Id = book.BiaId;
                newOrEdit.Title = book.Title;
                newOrEdit.Subtitle = book.Subtitle;
                newOrEdit.Annotation = book.Annotation;
                newOrEdit.CoverImage = book.CoverImage;
                newOrEdit.Year = book.Year;
                newOrEdit.Pages = book.Pages;
                newOrEdit.Edition = book.Edition;
                newOrEdit.Language = book.Language;
                newOrEdit.Authors = book.Authors.Where(a => a.BookId == bookId).ToList();
                newOrEdit.Categories = book.Cathegories.Where(c => c.BookId == bookId).ToList();
            }
            //
            newOrEdit.RequestAndReadBooksCount = ctx.RequestBooks
                .Where(r => r.AppUser != null && r.AppUser.LoginName == currentUser)
                .Count() + ctx.RentHistories.Where(b => b.User != null &&
                    b.User.LoginName == currentUser && b.ReturnedDate == null).Count();
            newOrEdit.ReadBooksByUser = ctx.RentHistories.Where(r => r.User != null && r.User.LoginName == currentUser
                && r.ReturnedDate == null).ToList();
            newOrEdit.RequestBooksByUser = ctx.RequestBooks.Where(r => r.AppUser != null && r.AppUser.LoginName == currentUser).ToList();
            //
            return View("NewOrEditBook", newOrEdit);
        }

        [HttpPost]
        public JsonResult GetBookAuthorsById([FromServices] BookDbContext ctx, [FromBody] int bookId = -1)
        {
            OperationResult result = new OperationResult();
            //
            try
            {
                if (bookId == -1)
                    throw new Exception("Ожидалось значение (int)bookId");
                //
                var allAuthors = ctx.Authors.Select(a => new { fullName = a.FullName(), uid = a.AuthorUid }).ToArray();
                var bookAuthors = ctx.BookAuthors.Where(c => c.BookId == bookId).Select(a => new { fullName = a.Author.FullName(), uid = a.Author.AuthorUid }).ToArray();
                //
                result.SetSuccess("Authors fetched successfully", allAuthors, bookAuthors);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }

        [HttpPost]
        public JsonResult GetBookCategoryById([FromServices] BookDbContext ctx, [FromBody] int bookId = -1)
        {
            OperationResult result = new OperationResult();
            //
            try
            {
                if (bookId == -1)
                    throw new Exception("Ожидалось значение (int)bookId");
                //
                var allСategory = ctx.Categories.Select(a => new { categoryName = a.CategoryName, categoryId = a.CategoryId }).ToArray();
                var bookCategory = ctx.BookCategories.Where(c => c.BookId == bookId).Select(a => new { categoryName = a.Category.CategoryName, categoryId = a.CategoryId }).ToArray();
                //
                result.SetSuccess("Category fetched successfully", allСategory, bookCategory);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex.Message));
            }
        }

        [HttpPost]
        public JsonResult AddBook([FromServices] BookDbContext ctx, [FromForm] AddBookForm book)
        {
            var result = new OperationResult();
            try
            {
                AppUser user = ctx.AppUsers.FirstOrDefault(q => q.LoginName == User.Identity.Name.Replace("DELLIN\\", ""));
                if (user == null)
                    throw new Exception("Ожидалось значение (AppUser)user");
                if (book == null)
                    throw new Exception("Ожидалось значение (AddBookForm)book");

                PaperBook newBook;
                if (book.BiaId == 0)
                {
                    newBook = new PaperBook
                    {
                        Title = book.Title,
                        Subtitle = book.Subtitle,
                        Annotation = book.Annotation,
                        Pages = book.Pages,
                        Year = book.Year,
                        Language = book.Language == "Русский" ? LanguageId.Rus : LanguageId.Eng,
                        CoverImage = null
                    };

                    ctx.PaperBooks.Add(newBook);
                    // Добавляем запись в историю загрузок
                    ctx.UploadHistories.Add(new UploadHistory
                    {
                        BookId = newBook.BiaId,
                        AppUserUid = user.Uid,
                        UploadDate = DateTime.Now
                    });
                    ctx.SaveChanges();
                }
                else
                {
                    newBook = ctx.PaperBooks.FirstOrDefault(b => b.BiaId == book.BiaId);
                    if (newBook == null)
                        throw new Exception("Не удалось найти книгу для редактирования");

                    newBook.Title = book.Title;
                    newBook.Subtitle = book.Subtitle;
                    newBook.Annotation = book.Annotation;
                    newBook.Pages = book.Pages;
                    newBook.Year = book.Year;
                    newBook.Language = book.Language == "Русский" ? LanguageId.Rus : LanguageId.Eng;
                }

                // Обработка обложки
                //if (book.CoverImage != null)
                //{
                string coverPath = SaveCoverImage(ctx, newBook.BiaId, book.CoverImage);
                newBook.CoverImage = coverPath;
                //}

                ctx.SaveChanges();

                // Добавляем авторов и категории
                UpdateAuthorsAndCategories(ctx, newBook.BiaId, book.Authors, book.Categories);

                //ctx.SaveChanges();
                return Json(result.SetSuccess(null,newBook.BiaId, null));
            }
            catch (Exception ex)
            {
                return Json(result.SetFail(ex));
            }
        }

        private string SaveCoverImage(BookDbContext ctx, int bookId, IFormFile file)
        {
            string uriPath = string.Empty;
            try
            {
                if (file != null)
                {
                    var fileExtension = Path.GetExtension(ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"'));
                    if (string.IsNullOrEmpty(fileExtension))
                    {
                        throw new Exception("Некорректный файл изображения");
                    }

                    string coverName = $"{bookId}{fileExtension}";
                    uriPath = $"/images/covers/{coverName}";
                    string fullPath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "covers", coverName);

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(400, 400)
                        }));

                        var encoder = new JpegEncoder
                        {
                            Quality = 75
                        };

                        image.Save(fullPath, encoder);
                    }
                }
                else
                {
                    if (bookId == 0)
                    {
                        uriPath = string.Empty;
                    }
                    else
                    {
                        PaperBook paperBook = ctx.PaperBooks.FirstOrDefault(b => b.BiaId == bookId);
                        if (paperBook == null)
                            throw new Exception("Не удалось найти книгу");
                        //
                        uriPath = paperBook.CoverImage;
                    }
                }

                return uriPath;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void UpdateAuthorsAndCategories(BookDbContext ctx, int bookId, List<Guid> authors, List<int> categories)
        {
            if (authors != null)
            {
                List<BookAuthor> deleted =
                    ctx.BookAuthors.Where(q => q.BookId == bookId).ToList();
                if (deleted.Any())
                {
                    ctx.BookAuthors.RemoveRange(deleted);
                }

                foreach (Guid author in authors)
                {
                    BookAuthor bookAuthor = new BookAuthor()
                    {
                        BookId = bookId,
                        AuthorUid = author
                    };
                    ctx.BookAuthors.Add(bookAuthor);
                }
            }
            else
            {
                IQueryable<BookAuthor> deleted = ctx.BookAuthors.Where(q => q.BookId == bookId);
                if (deleted.Any())
                    ctx.BookAuthors.RemoveRange(deleted);
            }

            if (categories != null)
            {
                IQueryable<BookCategory> deleted =
                    ctx.BookCategories
                        .Where(q => q.BookId == bookId);
                if (deleted.Any())
                {
                    ctx.BookCategories.RemoveRange(deleted);
                }

                foreach (int category in categories)
                {
                    BookCategory bookCategory = new BookCategory()
                    {
                        BookId = bookId,
                        CategoryId = category
                    };
                    ctx.BookCategories.Add(bookCategory);
                }
            }
            else
            {
                var deleted = ctx.BookCategories.Where(q => q.BookId == bookId);
                if (deleted.Any())
                    ctx.BookCategories.RemoveRange(deleted);
            }
            ctx.SaveChanges();
        }

        public IActionResult ExportManageToExcel([FromServices] BookDbContext ctx)
        {
            ManageViewModel mange = new ManageViewModel();
            mange.PaperBooks = ctx.PaperBooks.ToList();
            //
            return mange.ExportToExcel(mange.PaperBooks);
        }

        #endregion Manage

        #region Requests

        [Authorize(Policy = Data.S.GroupAdmin)]
        public IActionResult Requests([FromServices] BookDbContext ctx, string sortBy = "id", string sortOrder = "asc",
            int pageNumber = 1, int pageSize = 50)
        {
            RequestBooksViewModel requestBooksViewModel = new RequestBooksViewModel();
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            requestBooksViewModel.TotalCount = ctx.RequestBooks.Count();
            int totalPages = (int)Math.Ceiling((double)requestBooksViewModel.TotalCount / pageSize);
            ViewBag.TotalPages = totalPages;
            //
            List<RequestBook> requests = ctx.RequestBooks.ToList();
            //
            switch (sortBy)
            {
                case "id":
                    requests = sortOrder == "asc" ? requests.OrderBy(q => q.Book.BiaId).ToList() :
                        requests.OrderByDescending(q => q.Book.BiaId).ToList();
                    break;
                case "bookName":
                    requests = sortOrder == "asc" ? requests.OrderBy(q => q.Book.Title).ToList() :
                 requests.OrderByDescending(q => q.Book.Title).ToList();
                    break;
                case "request":
                    requests = sortOrder == "asc" ? requests.OrderBy(q => q.AppUser.FullName).ToList() :
                         requests.OrderByDescending(q => q.AppUser.FullName).ToList();
                    break;
                case "requestDate":
                    requests = sortOrder == "asc" ? requests.OrderBy(q => q.Date).ToList() :
                         requests.OrderByDescending(q => q.Date).ToList();
                    break;
                case "action":
                    requests = sortOrder == "asc" ? requests.OrderBy(q => q.Book.BiaId).ToList() :
                       requests.OrderByDescending(q => q.Book.BiaId).ToList();
                    break;
                default:
                    requests = sortOrder == "asc" ? requests.OrderBy(q => q.Book.BiaId).ToList() :
                       requests.OrderByDescending(q => q.Book.BiaId).ToList();
                    break;
            }
            //
            requests = requests.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            requestBooksViewModel.RequestBooks = requests;

            requestBooksViewModel.AppUsers = ctx.AppUsers.ToList();
            //
            return View("Requests", requestBooksViewModel);
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public JsonResult DeclineRent([FromServices] BookDbContext ctx, [FromBody] CancelingRequest request)
        {
            OperationResult result = new OperationResult();
            //
            try
            {
                PaperBook book = ctx.PaperBooks.FirstOrDefault(q => q.BiaId == request.BookId);
                if (book == null)
                    return Json(new OperationResult().SetFail("Книга не найдена"));
                //
                AppUser user = ctx.AppUsers.FirstOrDefault(q => q.Uid == request.UserGuid);
                if (user == null)
                    return Json(new OperationResult().SetFail("Пользователь не найдена"));
                //
                RequestBook req = ctx.RequestBooks.FirstOrDefault(q => q.AppUser == user && q.Book == book);
                if (req == null)
                    return Json(new OperationResult().SetFail("Запрос не найдена"));
                //
                ctx.RequestBooks.Remove(req);
                ctx.SaveChanges();
                //
                log.Info($"{User.Identity.Name}: decline rent of user {request.UserGuid} to book {request.BookId}");
                return Json(new OperationResult().SetSuccess());

            }
            catch (Exception ex)
            {
                return Json(new OperationResult().SetFail(ex));
            }
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public JsonResult RentBook([FromServices] BookDbContext ctx, [FromServices] EmailService email, [FromBody] RentBookModel model)
        {
            OperationResult result = new OperationResult();
            try
            {
                int id = model.Id;
                Guid userGuid = model.User;
                DateTime rentDate = model.Date;

                if (rentDate < DateTime.Now)
                {
                    return Json(new { status = result.SetFail(), details = "дата указанна неверно" });
                }


                PaperBook book = ctx.PaperBooks.First(b => b.BiaId == id);
                if (book != null)
                {
                    if (book.TakenByUser == null)
                    {
                        AppUser user = ctx.AppUsers.FirstOrDefault(q => q.Uid == userGuid);

                        if (ctx.PaperBooks.Count(q => q.TakenByUser == user) >= S.MaxBook)
                        {
                            return Json(new { status = result.SetFail(), code = "maxBookCount", details = $"Пользователь {user.FullName} уже имеет 5 книг" });
                        }
                        List<RequestBook> requesOtherUsers = ctx.RequestBooks.Where(q => q.BookId == book.BiaId && q.AppUserUid != user.Uid).ToList();

                        if(requesOtherUsers.Count > 0)
                        {
                            foreach(var otherRequest in requesOtherUsers)
                            {
                                ctx.RequestBooks.Remove(otherRequest);
                            }
                        }
                        ctx.SaveChanges();

                        RequestBook request = ctx.RequestBooks.FirstOrDefault(q => q.BookId == book.BiaId && q.AppUserUid == user.Uid);
                        if (request != null)
                        {
                            ctx.RequestBooks.Remove(request);
                        }

                        ctx.RentHistories.Add(new RentHistory()
                        {
                            Uid = Guid.NewGuid(),
                            ExtendedDueDate = rentDate,
                            TakenDate = DateTime.Now,
                            TakenBook = book,
                            User = user
                        });
                        book.TakenByUserUid = user.Uid;
                        book.TakenDue = rentDate;
                        ctx.SaveChanges();

                        //!!!
                        // отправить сообщение на почту
                        // отправить сообщение админу
                        IQueryable<Admin> admins = ctx.AdminNotifies.Where(q => q.NotifyId == AdminNotifyTypes.General).Select(q => q.Admin);
                        if (!string.IsNullOrWhiteSpace(user.Email))
                        {

                            var config = new Extension.MailConfiguration()
                            {
                                HeaderUrl = $"{_config["NotifyImgUrl"]}/header_requestConfirm.png",
                                BookUrl = $"{Request.Scheme}://{Request.Host}/Visitor/Book/{book.BiaId}",
                                UserName = user.FirstName,
                                BookTitle = book.Title,
                                Date = rentDate.ToString("dd.MM.yyyy"),
                                BtnUrl = $"{_config["NotifyImgUrl"]}/btn_requestConfirm.png",
                                FooterUrl = $"{_config["NotifyImgUrl"]}/footer_requestConfirm.png",
                            };
                            var message = new MimeMessage().UserRequestConfirmMessage(config);
                            message.From.Add(new MailboxAddress(S.MailBot));
                            message.To.Add(new MailboxAddress(user.Email));
                            email.Send(message);

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
                                    Text = $"{user.LoginName} не имеет почты, поэтому оповещение для него не возможно!"
                                };
                                email.Send(message);
                            }
                        }
                        log.Info($"{User.Identity.Name}: rent book {book.BiaId} to {book.TakenByUser.FullName}");
                        return Json(result.SetSuccess());
                    }
                    else
                    {
                        return Json(result.SetFail());
                    }
                }
                else
                {
                    return Json(result.SetFail());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Json(new OperationResult().SetFail(ex.Message));
            }
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public IActionResult ExportRequestsToExcel([FromServices] BookDbContext ctx)
        {
            RequestBooksViewModel request = new RequestBooksViewModel();
            request.RequestBooks = ctx.RequestBooks.ToList();
            //
            return request.ExportToExcel(request.RequestBooks);
        }

        #endregion Requests

        #region TakenBooks

        [Authorize(Policy = Data.S.GroupAdmin)]
        public IActionResult TakenBooks([FromServices] BookDbContext ctx, string sortBy = "id", string sortOrder = "asc",
            int pageNumber = 1, int pageSize = 50)
        {
            TakenBooksViewModel takenBooksViewModel = new TakenBooksViewModel();
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            takenBooksViewModel.TotalCount = ctx.RentHistories.Where(r => r.ReturnedDate == null).Count();
            int totalPages = (int)Math.Ceiling((double)takenBooksViewModel.TotalCount / pageSize);
            ViewBag.TotalPages = totalPages;
            //
            List<RentHistory> books = ctx.RentHistories.Where(q => q.ReturnedDate == null).ToList();
            //
            switch (sortBy)
            {
                case "id":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.TakenBook.BiaId).ToList() :
                        books.OrderByDescending(q => q.TakenBook.BiaId).ToList();
                    break;
                case "bookName":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.TakenBook.Title).ToList() :
                 books.OrderByDescending(q => q.TakenBook.Title).ToList();
                    break;
                case "request":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.User.FullName).ToList() :
                         books.OrderByDescending(q => q.User.FullName).ToList();
                    break;
                case "issued":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.TakenDate).ToList() :
                         books.OrderByDescending(q => q.TakenDate).ToList();
                    break;
                case "returnBook":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.ExtendedDueDate).ToList() :
                         books.OrderByDescending(q => q.ExtendedDueDate).ToList();
                    break;
                case "action":
                    books = sortOrder == "asc" ? books.OrderBy(q => q.TakenBook.BiaId).ToList() :
                        books.OrderByDescending(q => q.TakenBook.BiaId).ToList();
                    break;
                default:
                    books = sortOrder == "asc" ? books.OrderBy(q => q.TakenBook.BiaId).ToList() :
                        books.OrderByDescending(q => q.TakenBook.BiaId).ToList();
                    break;
            }
            //
            books = books.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            takenBooksViewModel.RentHistories = books;
            //
            return View("TakenBook", takenBooksViewModel);
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public JsonResult PassBook([FromServices] BookDbContext ctx, [FromServices] EmailService email, [FromBody] Guid id)
        {
            OperationResult result = new OperationResult();
            try
            {
                RentHistory rent = ctx.RentHistories.First(q => q.Uid == id);
                PaperBook book = rent.TakenBook;
                AppUser user = rent.User;
                book.TakenByUserUid = null;
                book.TakenDue = null;
                rent.ReturnedDate = DateTime.Now;
                //!!!
                // отправить сообщение на почту
                // отправить сообщение админу
                var admins = ctx.AdminNotifies.Where(q => q.NotifyId == AdminNotifyTypes.General).Select(q => q.Admin);

                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    var config = new Extension.MailConfiguration()
                    {
                        HeaderUrl = $"{_config["NotifyImgUrl"]}/header_returnBook.png",
                        BookUrl = $"{Request.Scheme}://{Request.Host}/Visitor/Book/{book.BiaId}",
                        UserName = user.FirstName,
                        BookTitle = book.Title,
                        BtnUrl = $"{_config["NotifyImgUrl"]}/btn_returnBook.png",
                        FooterUrl = $"{_config["NotifyImgUrl"]}/footer_returnBook.png",
                        Date = book.TakenDue?.ToString("dd.MM.yyyy")
                    };
                    var message = new MimeMessage().UserReturnBookMessage(config);
                    message.From.Add(new MailboxAddress(S.MailBot));
                    message.To.Add(new MailboxAddress(user.Email));
                    email.Send(message);
                }
                else
                {
                    foreach (var admin in admins)
                    {
                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(S.MailBot));
                        message.To.Add(new MailboxAddress(admin.Email));
                        message.Subject = "[Bia.Books] Attention!";
                        message.Body = new TextPart()
                        {
                            Text = $"{user.LoginName} запросил книгу, но он не имеет почты, поэтому оповещение для него не возможно!"
                        };
                        email.Send(message);
                    }
                }
                ctx.SaveChanges();
                log.Info($"{User.Identity.Name}: pass book {book.BiaId}");
                //
                return Json(result.SetSuccess("книга сдана"));
            }
            catch (Exception e)
            {
                log.Error(e);
                return Json(result.SetFail(e));
            }
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        [HttpPost]
        public JsonResult ProlongBook([FromServices] BookDbContext ctx, [FromBody] ProlongBook model)
        {
            OperationResult result = new OperationResult();
            try
            {
                Guid bookId = model.BookId;
                DateTime curentDate = model.CurentDate;
                DateTime newDate = model.NewDate;

                if (newDate == curentDate || newDate < curentDate)
                {
                    return Json(new { status = result.SetFail(), details = "Дата указанна неверно" });
                }

                RentHistory rent = ctx.RentHistories.FirstOrDefault(q => q.Uid == bookId);

                if (rent != null && rent.ExtendedTimesCount < 3)
                {
                    PaperBook book = rent.TakenBook;
                    book.TakenDue = newDate;
                    rent.ExtendedDueDate = newDate;
                    rent.ExtendedTimesCount++;
                    ctx.SaveChanges();

                    log.Info($"{User.Identity.Name}: prolong book {book.BiaId}");
                    return Json(result.SetSuccess());
                }
                else
                {
                    return Json(new { status = result.SetFail(), details = "Книга уже продлевалась три раза" });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Json(result.SetFail(ex.Message));
            }
        }

        public IActionResult ExportTakenBooksToExcel([FromServices] BookDbContext ctx)
        {
            TakenBooksViewModel takenBooks = new TakenBooksViewModel();
            takenBooks.RentHistories = ctx.RentHistories.Where(q => q.ReturnedDate == null).ToList();
            //
            return takenBooks.ExportToExcel(takenBooks.RentHistories);
        }

        #endregion

        #region Category

        [Authorize(Policy = Data.S.GroupAdmin)]
        public IActionResult Categories([FromServices] BookDbContext ctx, string sortBy = "title", string sortOrder = "asc",
            int pageNumber = 1, int pageSize = 50)
        {
            CategoriesViewModel categoriesViewModel = new CategoriesViewModel();
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            categoriesViewModel.TotalCount = ctx.Categories.Count();
            int totalPages = (int)Math.Ceiling((double)categoriesViewModel.TotalCount / pageSize);
            ViewBag.TotalPages = totalPages;
            //            
            List<Category> categories = ctx.Categories.ToList();
            switch (sortBy)
            {
                case "title":
                    categories = sortOrder == "asc" ? categories.OrderBy(q => q.CategoryName).ToList() :
                        categories.OrderByDescending(q => q.CategoryName).ToList();
                    break;
                case "countBook":
                    categories = sortOrder == "asc" ? categories.OrderBy(q => ctx.BookCategories.Count(bc => bc.CategoryId == q.CategoryId)).ToList() :
                 categories.OrderByDescending(q => ctx.BookCategories.Count(bc => bc.CategoryId == q.CategoryId)).ToList();
                    break;
                case "action":
                    categories = sortOrder == "asc" ? categories.OrderBy(q => q.CategoryName).ToList() :
                         categories.OrderByDescending(q => q.CategoryName).ToList();
                    break;
                default:
                    categories = sortOrder == "asc" ? categories.OrderBy(q => q.CategoryId).ToList() :
                        categories.OrderByDescending(q => q.CategoryId).ToList();
                    break;
            }

            categories = categories.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            foreach (Category category in categories)
            {
                categoriesViewModel.Categories.Add(category, ctx.BookCategories.Where(bc => bc.Category == category) == null ? 0
                    : ctx.BookCategories.Where(bc => bc.Category == category).Count());
            }

            return View("Categories", categoriesViewModel);
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public JsonResult EditCategory([FromServices] BookDbContext ctx, [FromBody] EditCategoryRequest request)
        {
            try
            {
                int idCategory = request.IdCategory;
                string newCategoryName = request.NewCategoryName;

                if (idCategory == 0 || string.IsNullOrWhiteSpace(newCategoryName))
                    return Json(new OperationResult().SetFail());
                //
                Category category = ctx.Categories.First(q => q.CategoryId == idCategory);
                if (ctx.Categories.Where(c => c.CategoryName == newCategoryName).Count() >= 1)
                    return Json(new OperationResult().SetFail("Категория уже существует"));
                //
                category.CategoryName = newCategoryName;
                //
                ctx.SaveChanges();
                var res = new OperationResult().SetSuccess();
                
                return Json(res);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult().SetFail(ex));
            }
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public JsonResult AddCategory([FromServices] BookDbContext ctx, [FromBody] string categoryName)
        {
            OperationResult result = new OperationResult();
            categoryName = categoryName.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(categoryName))
                return Json(result.SetFail());
            //
            if (ctx.Categories.Any(q => q.CategoryName.ToLower() == categoryName))
            {
                return Json(result.SetFail("Уже существует"));
            }
            else
            {
                try
                {
                    Category category = new Category() { CategoryName = (char.ToUpper(categoryName[0]) + categoryName.Substring(1).ToLower()) };
                    ctx.Categories.Add(category);
                    ctx.SaveChanges();
                    OperationResult res = new OperationResult().SetSuccess();
                    res.Details = JsonConvert.SerializeObject(new { name = category.CategoryName, value = category.CategoryId, details = "Категория добавлена" });
                    log.Info($"{User.Identity.Name}: {res.Details}");
                    return Json(res);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return Json(new OperationResult().SetFail(ex));
                }
            }
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public JsonResult DeleteCategory([FromServices] BookDbContext ctx, [FromBody] int id)
        {
            OperationResult result = new OperationResult();
            try
            {
                Category category = ctx.Categories.FirstOrDefault(q => q.CategoryId == id);
                if (category == null)
                {
                    return Json(result.SetFail());
                }

                List<BookCategory> bookCategories = ctx.BookCategories.Where(q => q.CategoryId == category.CategoryId).ToList();
                if (bookCategories.Any())
                {
                    ctx.BookCategories.RemoveRange(bookCategories);
                }

                ctx.SaveChanges();
                ctx.Categories.Remove(category);
                ctx.SaveChanges();
                log.Info($"{User.Identity.Name}: delete category {category}");
                return Json(result.SetSuccess());
            }
            catch (Exception e)
            {
                log.Error(e);
                return Json(result.SetFail());
            }

        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public IActionResult ExportCategoryToExcel([FromServices] BookDbContext ctx)
        {
            CategoriesViewModel categoriesViewModel = new CategoriesViewModel();
            List<Category> categories = ctx.Categories.ToList();
            //
            foreach (Category category in categories)
            {
                categoriesViewModel.Categories.Add(category, ctx.BookCategories.Where(bc => bc.Category == category) == null ? 0
                    : ctx.BookCategories.Where(bc => bc.Category == category).Count());
            }
            //
            return categoriesViewModel.ExportToExcel(categoriesViewModel.Categories);
        }

        #endregion Category

        #region Authors

        [Authorize(Policy = Data.S.GroupAdmin)]
        public IActionResult Authors([FromServices] BookDbContext ctx, string sortBy = "title", string sortOrder = "asc",
            int pageNumber = 1, int pageSize = 50)
        {
            AuthorsViewModel authorsViewModel = new AuthorsViewModel();

            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            authorsViewModel.TotalCount = ctx.Authors.Count();
            int totalPages = (int)Math.Ceiling((double)authorsViewModel.TotalCount / pageSize);
            ViewBag.TotalPages = totalPages;

            List<Author> authors = ctx.Authors.ToList();

            switch (sortBy)
            {
                case "title":
                    authors = sortOrder == "asc" ? authors.OrderBy(q => q.FullName()).ToList() :
                        authors.OrderByDescending(q => q.FullName()).ToList();
                    break;
                case "countBook":
                    authors = sortOrder == "asc" ? authors.OrderBy(q => ctx.BookAuthors.Count(bc => bc.AuthorUid == q.AuthorUid)).ToList() :
                 authors.OrderByDescending(q => ctx.BookAuthors.Count(bc => bc.AuthorUid == q.AuthorUid)).ToList();
                    break;
                case "action":
                    authors = sortOrder == "asc" ? authors.OrderBy(q => q.FullName()).ToList() :
                         authors.OrderByDescending(q => q.FullName()).ToList();
                    break;
                default:
                    authors = sortOrder == "asc" ? authors.OrderBy(q => q.FullName()).ToList() :
                        authors.OrderByDescending(q => q.FullName()).ToList();
                    break;
            }

            authors = authors.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            foreach (Author author in authors)
            {
                authorsViewModel.Authors.Add(author, ctx.BookAuthors.Where(ba => ba.Author == author) == null ? 0
                    : ctx.BookAuthors.Where(ba => ba.Author == author).Count());
            }
            return View("Authors", authorsViewModel);
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public IActionResult ExportAuthorsToExcel([FromServices] BookDbContext ctx)
        {
            AuthorsViewModel viewModel = new AuthorsViewModel();
            List<Author> authors = ctx.Authors.ToList();
            Dictionary<Author, int> keyValuePairs = new Dictionary<Author, int>();
            foreach (Author author in authors)
            {
                keyValuePairs.Add(author, ctx.BookAuthors.Where(ba => ba.Author == author) == null ? 0
                    : ctx.BookAuthors.Where(ba => ba.Author == author).Count());
            }
            //
            return viewModel.ExportToExcel(keyValuePairs);
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public JsonResult AddAuthor([FromServices] BookDbContext ctx, [FromBody] AddAuthors author)
        {
            /* Название полей в БД
             * FirstName - Имя
             * MiddleName - Отчесто
             * LastName- Фамилия
            */
            OperationResult result = new OperationResult();
            //
            try
            {
                Author authorDb = ctx.Authors.FirstOrDefault(a => a.FirstName.ToLower().Trim() == author.FirstName.ToLower().Trim()
                                                           && a.MiddleName.ToLower().Trim() == author.SurnameName.ToLower().Trim()
                                                           && a.LastName.ToLower().Trim() == author.LastName.ToLower().Trim());
                if (authorDb != null)
                    throw new Exception("Уже существует");
                //
                authorDb = new Author()
                {
                    AuthorUid = Guid.NewGuid(),
                    FirstName = (char.ToUpper(author.FirstName[0]) + author.FirstName.Substring(1).ToLower()),
                    MiddleName = !string.IsNullOrEmpty(author.SurnameName) ? (char.ToUpper(author.SurnameName[0]) + author.SurnameName.Substring(1).ToLower()) : null,
                    LastName = (char.ToUpper(author.LastName[0]) + author.LastName.Substring(1).ToLower())
                };
                ctx.Authors.Add(authorDb);
                ctx.SaveChanges();
                var res = new OperationResult().SetSuccess();
                res.Details = JsonConvert.SerializeObject(new
                {
                    name = authorDb.FullName(),
                    value = authorDb.AuthorUid,
                    details = "Автор добавлен"
                });
                log.Info($"{User.Identity.Name}: {res.Details}");
                return Json(res);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Json(new OperationResult().SetFail(ex));
            }
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public JsonResult EditAuthor([FromServices] BookDbContext ctx, [FromBody] EditAuthors author)
        {
            try
            {
                Author authorDb = ctx.Authors.FirstOrDefault(q => q.AuthorUid == author.AuthorGuid);

                if (authorDb == null)
                    throw new Exception("Не удалось найти автора");

                if (authorDb.FirstName == author.FirstName && authorDb.LastName == author.LastName && authorDb.MiddleName == author.SurnameName)
                    throw new Exception("Введенные данные совпадают с текущими");

                List<Author> existingAuthors = ctx.Authors.Where(a => a.FirstName == author.FirstName &&
                    a.LastName == author.LastName &&
                    a.MiddleName == author.SurnameName).ToList();

                if (existingAuthors.Count > 0)
                    throw new Exception("Уже существует");
                //

                authorDb.FirstName = author.FirstName;
                authorDb.MiddleName = author.SurnameName;
                authorDb.LastName = author.LastName;

                ctx.SaveChanges();
                OperationResult res = new OperationResult().SetSuccess();
                res.Details = JsonConvert.SerializeObject(new { uid = authorDb.AuthorUid.ToString(), details = "автор отредактирован" });
                log.Info($"{User.Identity.Name}: {res.Details}");
                return Json(res);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult().SetFail(ex));
            }
        }

        [Authorize(Policy = Data.S.GroupAdmin)]
        public JsonResult DeleteAuthor([FromServices] BookDbContext ctx, [FromBody] Guid id)
        {
            OperationResult result = new OperationResult();
            try
            {
                Author author = ctx.Authors.FirstOrDefault(q => q.AuthorUid == id);
                if (author == null)
                    return Json(result.SetFail("Не найден"));
                //
                List<BookAuthor> bookAuthors = ctx.BookAuthors.Where(q => q.AuthorUid == author.AuthorUid).ToList();
                if (bookAuthors.Any())
                    ctx.BookAuthors.RemoveRange(bookAuthors);
                //
                ctx.SaveChanges();
                ctx.Authors.Remove(author);
                ctx.SaveChanges();

                log.Info($"{User.Identity.Name}: delete author {author.FullName()}");
                return Json(result.SetSuccess("Автор удален"));
            }
            catch (Exception e)
            {
                log.Error(e);
                return Json(result.SetFail());
            }

        }

        #endregion Authors

    }
}