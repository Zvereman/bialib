using Bia.Internal.BookLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bia.Internal.BookLibrary.Models
{
    public class AuthorsViewModel
    {
        public Dictionary<Author, int> Authors { get; set; }
        public int TotalCount { get; set; }

        public AuthorsViewModel()
        {
            this.Authors = new Dictionary<Author, int>();
            this.TotalCount = 0;
        }

        public IActionResult ExportToExcel(Dictionary<Author, int> authors)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Authors");

                worksheet.Cells[1, 1].Value = "ФИО Автора";
                worksheet.Cells[1, 2].Value = "Книг в системе";

                int row = 2;
                foreach (var entry in authors)
                {
                    worksheet.Cells[row, 1].Value = entry.Key.FullName();
                    worksheet.Cells[row, 2].Value = entry.Value;
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 2].AutoFitColumns();
                worksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;

                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = "authors.xlsx"
                };
            }
        }
    }

    public class AddAuthors
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SurnameName { get; set; }

        public AddAuthors()
        {
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.SurnameName = string.Empty;
        }
    }

    public class EditAuthors
    {
        public Guid AuthorGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SurnameName { get; set; }

        public EditAuthors()
        {
            this.AuthorGuid = Guid.NewGuid();
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.SurnameName = string.Empty;
        }
    }
}
