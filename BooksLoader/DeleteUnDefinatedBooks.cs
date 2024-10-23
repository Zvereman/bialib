using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bia.Internal.BookLibrary.Data;
using ClosedXML;
using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;

namespace BooksLoader
{
    public static class DeleteUnDefinatedBooks
    {
        public static void Do(string path)
        {
            // взять все имена книг 
            var excel = new XLWorkbook(path);
            var titles = new List<string>();
            foreach (var sheet in excel.Worksheets)
            {
                var rows = sheet.RowsUsed();
                var nameColumn = sheet.Column(1).ColumnNumber();
                titles.AddRange(rows.Select(q => q.Cell(nameColumn).GetString()).ToList());
            }
            // для каждой записи
            using (var ctx = new BookDbContextFactory().CreateDbContext(new string[] { }))
            {
                var books = ctx.PaperBooks.Where(q=>titles.Contains(q.Title));
                var exceptBooks = new List<PaperBook>();

                foreach (var title in titles)
                {
                    var doubles = books.Where(q => q.Title == title).ToList();
                    if (doubles.Count()<=1)
                    {
                        if(doubles.Count()>0)
                        exceptBooks.Add(doubles.First());
                    }
                    else
                    {
                        var temp = doubles.Where(q => q.RentHistory.Any());
                        if (temp.Any())
                        {
                            exceptBooks.AddRange(temp);
                        }
                        else
                        {
                            temp = doubles.Where(q => q.ReqestBooks.Any());
                            if (temp.Any())
                            {
                                exceptBooks.AddRange(temp);
                            }
                            else
                            {
                                exceptBooks.Add(doubles.First());
                            }
                        }

                    }
                }
                var deleted_books = books.Except(exceptBooks);
                var deleted_authors = new List<BookAuthor>();
                var deleted_categories = new List<BookCategory>();
                var deleted_revievs = new List<Review>();
                var deleted_requests = new List<RequestBook>();
                foreach (var del in deleted_books)
                {
                    var tepa = ctx.BookAuthors.Where(q=>q.Book.BiaId==del.BiaId);
                    var tepc = ctx.BookCategories.Where(q=>q.Book.BiaId==del.BiaId);
                    var tepr = ctx.Reviews.Where(q => q.BookId == del.BiaId);
                    var teprq = ctx.RequestBooks.Where(q => q.BookId == del.BiaId);
                    if (tepa.Any()) deleted_authors.AddRange(tepa);
                    if (tepc.Any()) deleted_categories.AddRange(tepc);
                    if (tepr.Any()) deleted_revievs.AddRange(tepr);
                    if (teprq.Any()) deleted_requests.AddRange(teprq);
                }
                if(deleted_authors.Any()) ctx.BookAuthors.RemoveRange(deleted_authors);
                if (deleted_categories.Any()) ctx.BookCategories.RemoveRange(deleted_categories);
                if (deleted_revievs.Any()) ctx.Reviews.RemoveRange(deleted_revievs);
                if (deleted_requests.Any()) ctx.RequestBooks.RemoveRange(deleted_requests);
                if (deleted_books.Any()) ctx.Books.RemoveRange(deleted_books);
                ctx.SaveChanges();
            }

        }
    }
}
