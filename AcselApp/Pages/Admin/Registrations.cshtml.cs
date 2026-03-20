using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AcselApp.Data;
using AcselApp.Models;

namespace AcselApp.Pages.Admin
{
    public class RegistrationsModel : PageModel
    {
        private readonly AcselDbContext _db;
        private const string SessionKey = "AdminLoggedIn";

        public List<Registration> Registrations { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public RegistrationsModel(AcselDbContext db)
        {
            _db = db;
        }

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString(SessionKey) == "true";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            Registrations = await _db.Registrations
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();

            SuccessMessage = TempData["SuccessMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var reg = await _db.Registrations.FindAsync(id);
            if (reg != null)
            {
                _db.Registrations.Remove(reg);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"已刪除報名記錄：{reg.FullName}（編號 {reg.Id}）";
            }
            else
            {
                TempData["ErrorMessage"] = "找不到該報名記錄。";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetDownloadCsvAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var registrations = await _db.Registrations
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,FullName,Email,Phone,Institution,TitlePosition,TicketType,PaymentMethod,RegistrationDate,PaymentStatus,Comments");

            foreach (var r in registrations)
            {
                sb.AppendLine(string.Join(",",
                    Escape(r.Id.ToString()),
                    Escape(r.FullName),
                    Escape(r.Email),
                    Escape(r.Phone),
                    Escape(r.Institution),
                    Escape(r.TitlePosition),
                    Escape(r.TicketType),
                    Escape(r.PaymentMethod),
                    Escape(r.RegistrationDate.ToString("yyyy-MM-dd HH:mm:ss")),
                    Escape(r.PaymentStatus),
                    Escape(r.Comments)
                ));
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"registrations_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        public async Task<IActionResult> OnPostTogglePaymentAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var reg = await _db.Registrations.FindAsync(id);
            if (reg != null)
            {
                reg.PaymentStatus = reg.PaymentStatus == "Paid" ? "Pending" : "Paid";
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"「{reg.FullName}」付款狀態已更新為：{reg.PaymentStatus}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetExportExcelAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var registrations = await _db.Registrations
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("報名資料");

            // Header row
            string[] headers = { "編號", "姓名", "Email", "電話", "機構", "職稱", "費用類型", "付款方式", "付款狀態", "報名日期", "備註" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#4F46E5");
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            }

            // Data rows
            for (int i = 0; i < registrations.Count; i++)
            {
                var r = registrations[i];
                int row = i + 2;
                ws.Cell(row, 1).Value = r.Id;
                ws.Cell(row, 2).Value = r.FullName;
                ws.Cell(row, 3).Value = r.Email;
                ws.Cell(row, 4).Value = r.Phone ?? "";
                ws.Cell(row, 5).Value = r.Institution ?? "";
                ws.Cell(row, 6).Value = r.TitlePosition ?? "";
                ws.Cell(row, 7).Value = r.TicketType;
                ws.Cell(row, 8).Value = r.PaymentMethod ?? "";
                ws.Cell(row, 9).Value = r.PaymentStatus;
                ws.Cell(row, 10).Value = r.RegistrationDate.ToString("yyyy-MM-dd HH:mm");
                ws.Cell(row, 11).Value = r.Comments ?? "";

                // Alternate row shading
                if (i % 2 == 1)
                {
                    ws.Row(row).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#F1F5F9");
                }

                // Color payment status cell
                var statusCell = ws.Cell(row, 9);
                if (r.PaymentStatus == "Paid")
                {
                    statusCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#D1FAE5");
                    statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#065F46");
                }
                else
                {
                    statusCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FEF3C7");
                    statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#92400E");
                }
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            var bytes = stream.ToArray();
            var fileName = $"ACSEL2026_Registrations_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
