using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AcselApp.Data;
using AcselApp.Models;

namespace AcselApp.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly AcselDbContext _db;
        private const string SessionKey = "AdminLoggedIn";

        public List<UpdateNewsItem> NewsItems { get; set; } = new();

        [BindProperty]
        public UpdateNewsItem NewItem { get; set; } = new();

        [BindProperty]
        public UpdateNewsItem EditItem { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IndexModel(AcselDbContext db)
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

            NewsItems = await _db.UpdatesNews
                .OrderByDescending(u => u.PublishDate)
                .ToListAsync();

            // Check for status messages from TempData
            SuccessMessage = TempData["SuccessMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            return Page();
        }

        // Create a new news item
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            if (string.IsNullOrWhiteSpace(NewItem.Title) || string.IsNullOrWhiteSpace(NewItem.Content))
            {
                TempData["ErrorMessage"] = "標題和內容為必填欄位。";
                return RedirectToPage();
            }

            var item = new UpdateNewsItem
            {
                Title = NewItem.Title,
                Content = NewItem.Content,
                PublishDate = NewItem.PublishDate == default ? DateTime.Now : NewItem.PublishDate,
                IsActive = NewItem.IsActive
            };

            _db.UpdatesNews.Add(item);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"消息「{item.Title}」建立成功（編號：{item.Id}）。";
            return RedirectToPage();
        }

        // Update an existing news item
        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var existing = await _db.UpdatesNews.FindAsync(EditItem.Id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = $"找不到編號為 {EditItem.Id} 的消息項目。";
                return RedirectToPage();
            }

            existing.Title = EditItem.Title;
            existing.Content = EditItem.Content;
            existing.PublishDate = EditItem.PublishDate;
            existing.IsActive = EditItem.IsActive;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"消息「{existing.Title}」（編號：{existing.Id}）已成功更新。";
            return RedirectToPage();
        }

        // Delete a news item
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var item = await _db.UpdatesNews.FindAsync(id);
            if (item == null)
            {
                TempData["ErrorMessage"] = $"找不到編號為 {id} 的消息項目。";
                return RedirectToPage();
            }

            var title = item.Title;
            _db.UpdatesNews.Remove(item);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"消息「{title}」（編號：{id}）已成功刪除。";
            return RedirectToPage();
        }

        // Toggle active status
        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var toggleItem = await _db.UpdatesNews.FindAsync(id);
            if (toggleItem == null)
            {
                TempData["ErrorMessage"] = $"找不到編號為 {id} 的消息項目。";
                return RedirectToPage();
            }

            toggleItem.IsActive = !toggleItem.IsActive;
            await _db.SaveChangesAsync();

            var status = toggleItem.IsActive ? "已啟用" : "已停用";
            TempData["SuccessMessage"] = $"消息「{toggleItem.Title}」（編號：{id}）{status}。";
            return RedirectToPage();
        }

        // Logout
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Admin/Login");
        }
    }
}
