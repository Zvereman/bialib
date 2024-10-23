using Bia.Internal.BookLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Bia.Internal.BookLibrary.Models
{
    public class ReviewViewModel
    {
        public Guid ReviewUid { get; set; }
        public string Book { get; set; }
        public string Review { get; set; }
        public User User { get; set; }
        public int Rating { get; set; }
    }

    public class ReviewsViewModel
    {
        public List<Review> Reviews { get; set; }
        public int TotalCount { get; set; }

        public ReviewsViewModel()
        {
            this.Reviews = new List<Review>();
            this.TotalCount = 0;
        }

        public IActionResult ExportToExcel(List<Review> reviews)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Reviews");

                worksheet.Cells[1, 1].Value = "Название";
                worksheet.Cells[1, 2].Value = "Авторы";
                worksheet.Cells[1, 3].Value = "Отзыв";
                worksheet.Cells[1, 4].Value = "Рейтинг";

                int row = 2;
                foreach (var entry in reviews)
                {
                    worksheet.Cells[row, 1].Value = entry.Book.Title;
                    worksheet.Cells[row, 2].Value = string.Join(", ", entry.Book.Authors.Where(a => a.BookId == entry.Book.BiaId).Select(a => a.Author.FullName()));
                    worksheet.Cells[row, 3].Value = entry.Text;
                    worksheet.Cells[row, 4].Value = entry.Rating;
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 4].AutoFitColumns();
                worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;

                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = "reviews.xlsx"
                };
            }
        }
    }

    public class AddReview
    {
        public int BookId { get; set; }
        public string Review { get; set; }
        public string Rating { get; set; }
        public string Login { get; set; }
    }
}
