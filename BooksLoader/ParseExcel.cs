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
    public static class ParseExcel
    {
        public static void Do(string path)
        {
            // запсиь в бд
            using (var ctx = new BookDbContextFactory().CreateDbContext(new string[] { }))
            {
                // parse exel
                var excel = new XLWorkbook(path);
                foreach (var sheet in excel.Worksheets)
                {
                    var rows = sheet.RowsUsed();
                    var nameColumn = sheet.Column(1).ColumnNumber();
                    var authorColumn = sheet.Column(2).ColumnNumber();
                    //var languageColumn = sheet.Column(3).ColumnNumber();
                    //var yearColumn = sheet.Column(4).ColumnNumber();
                    //var pagesColumn = sheet.Column(5).ColumnNumber();
                    //var desctriptionColumn = sheet.Column(6).ColumnNumber();
                    // возможно добавить и др инфу
                    foreach (var row in rows)
                    {
                        // authors
                        var currentAuthors = new List<Author>();
                        var authorRaw = row.Cell(authorColumn).GetString();
                        if (!string.IsNullOrWhiteSpace(authorRaw))
                            foreach (var author in authorRaw.Split(',').Where(q => !q.Contains("перевод")))
                            {
                                var first = "";
                                var second = "";
                                var midle = "";
                                var splits = author.Split(new char[] { '.', ' ' });

                                if (splits.Length == 1)
                                {
                                    second = splits[0];
                                }
                                else
                                {
                                    if (splits.Length == 2)
                                    {
                                        first = splits[0];
                                        second = splits[1];
                                    }
                                    else if (splits.Length == 3)
                                    {
                                        first = splits[0];
                                        second = splits[2];
                                        midle = splits[1];
                                    }
                                    else
                                    {
                                        second = author;
                                    }
                                }

                                var res = new Author();
                                if (!string.IsNullOrEmpty(first)) res.FirstName = first;
                                if (!string.IsNullOrEmpty(midle)) res.MiddleName = midle;
                                if (!string.IsNullOrEmpty(second)) res.LastName = second;
                                currentAuthors.Add(res);
                            }

                        //book
                        var book = new PaperBook();
                        book.Title = row.Cell(nameColumn).GetString();
                        //book.Language = (row.Cell(languageColumn).GetString().Contains("анг"))? LanguageId.Eng : LanguageId.Rus;
                        //book.Annotation = row.Cell(desctriptionColumn).GetString();
                        //book.Year = int.Parse(row.Cell(yearColumn).GetString());
                        //book.Pages = int.Parse(row.Cell(pagesColumn).GetString());


                        ctx.Authors.AddRange(currentAuthors);
                        ctx.Books.Add(book);
                        ctx.SaveChanges();

                        var relation = new List<BookAuthor>();
                        foreach (var curr in currentAuthors)
                        {
                            relation.Add(new BookAuthor()
                            {
                                AuthorUid = curr.AuthorUid,
                                BookId = book.BiaId
                            });
                        }

                        ctx.BookAuthors.AddRange(relation);
                        ctx.SaveChanges();
                    }
                }
            }
        }
    }
}
