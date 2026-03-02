using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AcselApp.Data;
using AcselApp.Models;

namespace AcselApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AcselDbContext _db;
        private readonly ILogger<IndexModel> _logger;

        public List<UpdateNewsItem> Updates { get; set; } = new();

        public IndexModel(AcselDbContext db, ILogger<IndexModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Updates = await _db.UpdatesNews
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.PublishDate)
                .ToListAsync();
        }
    }
}
