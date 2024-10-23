using Bia.Internal.BookLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

namespace Bia.Internal.BookLibrary.Models
{
    public class TakenBooksViewModel
    {
        public List<RentHistory> RentHistories { get; set; }
        public int TotalCount { get; set; }

        public TakenBooksViewModel()
        {
            this.RentHistories = new List<RentHistory>();
            this.TotalCount = 0;
        }

        public IActionResult ExportToExcel(List<RentHistory> rentHistories)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TakenBooks");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Название";
                worksheet.Cells[1, 3].Value = "Запросил";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Выдана";
                worksheet.Cells[1, 6].Value = "Вернуть";

                int row = 2;
                foreach (var entry in rentHistories)
                {
                    worksheet.Cells[row, 1].Value = entry.TakenBook.BiaId;
                    worksheet.Cells[row, 2].Value = entry.TakenBook.Title;
                    worksheet.Cells[row, 3].Value = entry.User.FullName;
                    worksheet.Cells[row, 4].Value = entry.User.Email;
                    worksheet.Cells[row, 5].Value = entry.TakenDate.ToString("dd.MM.yyyy");
                    worksheet.Cells[row, 6].Value = entry.ExtendedDueDate.ToString("dd.MM.yyyy");
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 6].AutoFitColumns();
                worksheet.Cells[1, 1, 1, 6].Style.Font.Bold = true;

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
