using Bia.Internal.BookLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bia.Internal.BookLibrary.Models
{
    public class RequestBooksViewModel
    {
        public List<RequestBook> RequestBooks { get; set; }

        public int TotalCount { get; set; }
        public List<AppUser> AppUsers { get; set; }

        public RequestBooksViewModel()
        {
            this.RequestBooks = new List<RequestBook>();
            this.TotalCount = 0;
            this.AppUsers = new List<AppUser>();
        }

        public IActionResult ExportToExcel(List<RequestBook> requestBooks)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Requests");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Название";
                worksheet.Cells[1, 3].Value = "Запросил";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Дата запроса";

                int row = 2;
                foreach (var entry in requestBooks)
                {
                    worksheet.Cells[row, 1].Value = entry.Book.BiaId;
                    worksheet.Cells[row, 2].Value = entry.Book.Title;
                    worksheet.Cells[row, 3].Value = entry.AppUser.FullName;
                    worksheet.Cells[row, 4].Value = entry.AppUser.Email;
                    worksheet.Cells[row, 5].Value = entry.Date.ToString("dd.MM.yyyy");
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 5].AutoFitColumns();
                worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;

                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = "requests.xlsx"
                };
            }
        }
    }

    public class CancelingRequest
    {
        public int BookId { get; set; }
        public Guid UserGuid { get; set; }

        public CancelingRequest()
        {
            this.BookId = 0;
            this.UserGuid = Guid.NewGuid();
        }
    }
}
