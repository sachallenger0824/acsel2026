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
                TempData["ErrorMessage"] = "Title and Content are required.";
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

            TempData["SuccessMessage"] = $"News item \"{item.Title}\" created successfully (ID: {item.Id}).";
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
                TempData["ErrorMessage"] = $"News item with ID {EditItem.Id} not found.";
                return RedirectToPage();
            }

            existing.Title = EditItem.Title;
            existing.Content = EditItem.Content;
            existing.PublishDate = EditItem.PublishDate;
            existing.IsActive = EditItem.IsActive;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"News item \"{existing.Title}\" (ID: {existing.Id}) updated successfully.";
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
                TempData["ErrorMessage"] = $"News item with ID {id} not found.";
                return RedirectToPage();
            }

            var title = item.Title;
            _db.UpdatesNews.Remove(item);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"News item \"{title}\" (ID: {id}) deleted successfully.";
            return RedirectToPage();
        }

        // Toggle active status
        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var item = await _db.UpdatesNews.FindAsync(id);
            if (item == null)
            {
                TempData["ErrorMessage"] = $"News item with ID {id} not found.";
                return RedirectToPage();
            }

            item.IsActive = !item.IsActive;
            await _db.SaveChangesAsync();

            var status = item.IsActive ? "activated" : "deactivated";
            TempData["SuccessMessage"] = $"News item \"{item.Title}\" (ID: {id}) has been {status}.";
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
