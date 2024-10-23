using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bia.Internal.BookLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Bia.Internal.BookLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using NLog;
using Microsoft.AspNetCore.Hosting;

namespace Bia.Internal.BookLibrary.Controllers
{
    public class HomeController : Controller
    {
        private Logger log;
        private readonly IHostingEnvironment _hostingEnvironment;


        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            log = NLog.LogManager.GetCurrentClassLogger();
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize(Policy = Data.S.GroupCommon)]
        public IActionResult Index([FromServices] BookDbContext ctx)
        {

            string currentUser = User.Identity.Name.Replace("DELLIN\\", "");
            string userRole = ctx.AppUsers.FirstOrDefault(u => u.LoginName == currentUser).Discriminator;
            //
            //!!! Требуется изменение в БД
            int requestAndReadBooksCount = ctx.RequestBooks
                .Where(r => r.AppUser != null && r.AppUser.LoginName == currentUser)
                .Count() + ctx.RentHistories.Where(b => b.User != null &&
                    b.User.LoginName == currentUser && b.ReturnedDate == null).Distinct().Count();
            //int readingBooksCount = ctx.RentHistories
            //    .Where(b => b.User != null && b.User.LoginName == currentUser && b.ReturnedDate == null)
            //    .Count();
            //
            List<RentHistory> readBooksByUser = ctx.RentHistories.Where(r => r.User != null && r.User.LoginName == currentUser
                && r.ReturnedDate == null).ToList();
            //
            List<RequestBook> requestBooksByUser = ctx.RequestBooks.Where(r => r.AppUser != null && r.AppUser.LoginName == currentUser).ToList();
            //
            Dictionary<string, List<PaperBook>> booksByCategory = new Dictionary<string, List<PaperBook>>();
            //
            List<Category> lstCategory = ctx.Categories.ToList();
            List<BookCategory> lstBookCategory = ctx.BookCategories.ToList();
            //List<PaperBook> books = ctx.PaperBooks.ToList();
            //
            foreach (Category category in lstCategory)
            {
                //
                List<int> bookIds = ctx.BookCategories
                                  .Where(bc => bc.CategoryId == category.CategoryId)
                                  .Select(bc => bc.BookId)
                                  .ToList();
                if (bookIds.Count == 0)
                    continue;
                //
                List<PaperBook> categoryBooks = ctx.PaperBooks
                                       .Where(book => bookIds.Contains(book.BiaId))
                                       .OrderBy(b => Guid.NewGuid())
                                       .Take(4)
                                       .ToList();
                //
                if (categoryBooks.Any())
                {
                    booksByCategory.Add(category.CategoryName, categoryBooks);
                }
            }
            //
            return View(new HomeBookViewModel
            {
                BooksByCategory = booksByCategory,
                RequestAndReadBooksCount = requestAndReadBooksCount,
                ReadBooksByUser = readBooksByUser,
                RequestBooksByUser = requestBooksByUser,
                UserRole = userRole
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
