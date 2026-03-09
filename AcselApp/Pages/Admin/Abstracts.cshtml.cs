using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AcselApp.Data;
using AcselApp.Models;

namespace AcselApp.Pages.Admin
{
    public class AbstractsModel : PageModel
    {
        private readonly AcselDbContext _db;
        private const string SessionKey = "AdminLoggedIn";

        public List<AbstractSubmission> Abstracts { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public AbstractsModel(AcselDbContext db)
        {
            _db = db;
        }

        private bool IsAuthenticated() =>
            HttpContext.Session.GetString(SessionKey) == "true";

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            Abstracts = await _db.AbstractSubmissions
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            SuccessMessage = TempData["SuccessMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var item = await _db.AbstractSubmissions.FindAsync(id);
            if (item != null)
            {
                _db.AbstractSubmissions.Remove(item);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"已刪除投稿記錄：{item.Title}（編號 {item.Id}）";
            }
            else
            {
                TempData["ErrorMessage"] = "找不到該投稿記錄。";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var item = await _db.AbstractSubmissions.FindAsync(id);
            if (item != null)
            {
                item.Status = item.Status == "Accepted" ? "Pending" : "Accepted";
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"「{item.Title}」狀態已更新為：{item.Status}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetDownloadCsvAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var abstracts = await _db.AbstractSubmissions
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,PresentationType,Title,Authors,Affiliations,CorrespondingAuthor,CorrespondingEmail,Keywords,AbstractText,SubmittedAt,Status");

            foreach (var a in abstracts)
            {
                sb.AppendLine(string.Join(",",
                    Escape(a.Id.ToString()),
                    Escape(a.PresentationType),
                    Escape(a.Title),
                    Escape(a.Authors),
                    Escape(a.Affiliations),
                    Escape(a.CorrespondingAuthor),
                    Escape(a.CorrespondingEmail),
                    Escape(a.Keywords),
                    Escape(a.AbstractText),
                    Escape(a.SubmittedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                    Escape(a.Status)
                ));
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"abstracts_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
