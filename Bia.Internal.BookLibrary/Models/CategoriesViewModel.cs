using Bia.Internal.BookLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

namespace Bia.Internal.BookLibrary.Models
{
    public class CategoriesViewModel
    {
        public Dictionary<Category, int> Categories { get; set; }
        public int TotalCount { get; set; }
        public CategoriesViewModel() 
        { 
            this.Categories = new Dictionary<Category, int>();
            this.TotalCount = 0;
        }

        public IActionResult ExportToExcel(Dictionary<Category, int> category)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Category");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Категория";
                worksheet.Cells[1, 3].Value = "Книг в системе";

                int row = 2;
                foreach (var entry in category)
                {
                    worksheet.Cells[row, 1].Value = entry.Key.CategoryId;
                    worksheet.Cells[row, 2].Value = entry.Key.CategoryName;
                    worksheet.Cells[row, 3].Value = entry.Value;
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 3].AutoFitColumns();
                worksheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;

                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = "category.xlsx"
                };
            }
        }
    }
}
