using Bia.Internal.BookLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bia.Internal.BookLibrary.Models
{
    public class ManageViewModel
    {
        public List<PaperBook> PaperBooks { get; set; }
        public int TotalCount { get; set; }
        public List<AppUser> AppUsers { get; set; }

        public ManageViewModel() 
        { 
            this.PaperBooks = new List<PaperBook>();
            this.TotalCount = 0;
            this.AppUsers = new List<AppUser>();
        }

        public IActionResult ExportToExcel(List<PaperBook> paperBooks)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Manage");

                worksheet.Cells[1, 1].Value = "Название";
                worksheet.Cells[1, 2].Value = "Авторы";
                worksheet.Cells[1, 3].Value = "Категории";
                worksheet.Cells[1, 4].Value = "Год";
                worksheet.Cells[1, 5].Value = "Страницы";
                worksheet.Cells[1, 6].Value = "Рейтинг";
                worksheet.Cells[1, 7].Value = "Отзывы";
                worksheet.Cells[1, 8].Value = "Язык";

                int row = 2;
                foreach (var entry in paperBooks)
                {
                    worksheet.Cells[row, 1].Value = entry.Title;
                    worksheet.Cells[row, 2].Value = string.Join(", ", entry.Authors.Where(a => a.BookId == entry.BiaId).Select(a => a.Author.FullName()));
                    worksheet.Cells[row, 3].Value = string.Join(", ", entry.Cathegories.Where(a => a.BookId == entry.BiaId).Select(a => a.Category.CategoryName));
                    worksheet.Cells[row, 4].Value = entry.Year;
                    worksheet.Cells[row, 5].Value = entry.Pages;
                    worksheet.Cells[row, 6].Value = entry.AverageRating;
                    worksheet.Cells[row, 7].Value = entry.Reviews.Where(r => r.BookId == entry.BiaId).Count();
                    worksheet.Cells[row, 8].Value = (int)entry.Language == 1 ? "Русский" : "Английский";
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 8].AutoFitColumns();
                worksheet.Cells[1, 1, 1, 8].Style.Font.Bold = true;

                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = "takenBooks.xlsx"
                };
            }
        }
    }
}
