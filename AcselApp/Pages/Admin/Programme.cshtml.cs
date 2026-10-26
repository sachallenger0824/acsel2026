using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AcselApp.Data;
using AcselApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcselApp.Pages.Admin
{
    public sealed class ProgrammeModel : PageModel
    {
        private const string SessionKey = "AdminLoggedIn";
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly AcselDbContext _db;

        [BindProperty]
        public ProgrammeEditForm Programme { get; set; } = new();

        public string? SuccessMessage { get; private set; }

        public ProgrammeModel(AcselDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            var record = await _db.ProgrammeContents
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == 1);

            Programme = ToForm(DeserializeContent(record?.ContentJson));
            SuccessMessage = TempData["SuccessMessage"] as string;
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Admin/Login");

            if (Programme.Days.Count > 10 || Programme.Days.Sum(day => day.Items.Count) > 100 ||
                Programme.Tracks.Count > 20 || Programme.Notes.Count > 20)
            {
                ModelState.AddModelError(string.Empty, "議程日、活動、主題或備註數量超過上限，請調整後再儲存。");
            }

            if (Programme.Days.Count == 0 || Programme.Days.Any(day => day.Items.Count == 0))
            {
                ModelState.AddModelError(string.Empty, "至少要有一個議程日，且每一天至少要有一個活動。");
            }

            if (!ModelState.IsValid)
                return Page();

            var content = ToContent(Programme);
            var record = await _db.ProgrammeContents.SingleOrDefaultAsync(item => item.Id == 1);
            if (record is null)
            {
                record = new ProgrammeContentRecord { Id = 1 };
                _db.ProgrammeContents.Add(record);
            }

            record.ContentJson = JsonSerializer.Serialize(content, JsonOptions);
            record.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "議程已儲存，公開頁面會立即更新。";
            return RedirectToPage();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Admin/Login");
        }

        private bool IsAuthenticated() => HttpContext.Session.GetString(SessionKey) == "true";

        private static ProgrammeContent DeserializeContent(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return ProgrammeDefaults.CreateInitialContent();

            try
            {
                return JsonSerializer.Deserialize<ProgrammeContent>(json, JsonOptions)
                    ?? ProgrammeDefaults.CreateInitialContent();
            }
            catch (JsonException)
            {
                return ProgrammeDefaults.CreateInitialContent();
            }
        }

        private static ProgrammeEditForm ToForm(ProgrammeContent content) => new()
        {
            ConferenceName = content.ConferenceName,
            Theme = content.Theme,
            Venue = content.Venue,
            Days = content.Days.Select(day => new ProgrammeDayEditForm
            {
                Label = day.Label,
                Date = day.Date,
                Items = day.Items.Select(item => new ProgrammeEventEditForm
                {
                    Time = item.Time,
                    Program = item.Program,
                    DetailsText = string.Join(Environment.NewLine, item.Details)
                }).ToList()
            }).ToList(),
            Tracks = content.Tracks.Select(track => new ProgrammeTrackEditForm
            {
                Name = track.Name,
                Theme = track.Theme
            }).ToList(),
            Notes = content.Notes.ToList()
        };

        private static ProgrammeContent ToContent(ProgrammeEditForm form) => new()
        {
            ConferenceName = form.ConferenceName.Trim(),
            Theme = form.Theme.Trim(),
            Venue = form.Venue.Trim(),
            Days = form.Days.Select(day => new ProgrammeDay
            {
                Label = day.Label.Trim(),
                Date = day.Date.Trim(),
                Items = day.Items.Select(item => new ProgrammeEvent
                {
                    Time = item.Time.Trim(),
                    Program = item.Program.Trim(),
                    Details = item.DetailsText
                        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList()
                }).ToList()
            }).ToList(),
            Tracks = form.Tracks.Select(track => new ProgrammeTrack
            {
                Name = track.Name.Trim(),
                Theme = track.Theme.Trim()
            }).ToList(),
            Notes = form.Notes
                .Where(note => !string.IsNullOrWhiteSpace(note))
                .Select(note => note.Trim())
                .ToList()
        };
    }

    public sealed class ProgrammeEditForm
    {
        [Required, StringLength(200)]
        public string ConferenceName { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string Theme { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Venue { get; set; } = string.Empty;

        public List<ProgrammeDayEditForm> Days { get; set; } = new();
        public List<ProgrammeTrackEditForm> Tracks { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }

    public sealed class ProgrammeDayEditForm
    {
        [Required, StringLength(100)]
        public string Label { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Date { get; set; } = string.Empty;

        public List<ProgrammeEventEditForm> Items { get; set; } = new();
    }

    public sealed class ProgrammeEventEditForm
    {
        [Required, StringLength(40)]
        public string Time { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string Program { get; set; } = string.Empty;

        [StringLength(5000)]
        public string DetailsText { get; set; } = string.Empty;
    }

    public sealed class ProgrammeTrackEditForm
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string Theme { get; set; } = string.Empty;
    }
}
