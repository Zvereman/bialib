using Bia.Internal.BookLibrary.Data;
using Bia.Internal.BookLibrary.Data.Enum;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bia.Internal.BookLibrary.Models
{

    public class UsersViewModel
    {
        public List<AppUser> Users { get; set; }
        public int TotalCount { get; set; }

        public UsersViewModel()
        {
            this.Users = new List<AppUser>();
            this.TotalCount = 0;
        }

        public IActionResult ExportToExcel(List<AppUser> users)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Users");

                worksheet.Cells[1, 1].Value = "ФИО";
                worksheet.Cells[1, 2].Value = "Электроная почта";
                worksheet.Cells[1, 3].Value = "Админ";
                worksheet.Cells[1, 4].Value = "Уведомления";
                worksheet.Cells[1, 5].Value = "Игнорировать";

                int row = 2;
                foreach (var entry in users)
                {
                    var admin = entry as Admin;

                    worksheet.Cells[row, 1].Value = entry.FullName;
                    worksheet.Cells[row, 2].Value = entry.Email;
                    worksheet.Cells[row, 3].Value = entry.AccessGroup == AccessGroupId.Common ? "Пользователь" : "Администратор";
                    worksheet.Cells[row, 4].Value = admin != null ? admin.AdminNotifies.Any(q => q.NotifyId == AdminNotifyTypes.General) ? 
                        "Уведомлять администратора" : "Не уведомлять администратора" : "";
                    worksheet.Cells[row, 5].Value = entry.Ignored != null ? "Игнорировать" : "Не игнорировать";
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 5].AutoFitColumns();
                worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;

                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = "users.xlsx"
                };
            }
        }
    }
}
