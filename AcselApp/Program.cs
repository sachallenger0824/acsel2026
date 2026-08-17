using Microsoft.EntityFrameworkCore;
using AcselApp.Data;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Add session support for admin login
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register SQLite DbContext
builder.Services.AddDbContext<AcselDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=acsel.db"));

var app = builder.Build();

// Ensure database is created and seeded on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AcselDbContext>();
    db.Database.EnsureCreated();

    // Add new columns if they don't exist (for existing databases)
    var conn = db.Database.GetDbConnection();
    conn.Open();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "PRAGMA table_info(Registrations)";
    var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read())
            existingColumns.Add(reader.GetString(1));
    }
    var newColumns = new Dictionary<string, string>
    {
        ["TitlePosition"] = "TEXT",
        ["PaymentMethod"] = "TEXT",
        ["PaymentLink"] = "TEXT",
        ["SightseeingTour"] = "TEXT",
        ["TechnicalTour"] = "TEXT",
        ["Comments"] = "TEXT",
        ["ParticipantType"] = "TEXT NOT NULL DEFAULT 'International'"
    };
    foreach (var (col, type) in newColumns)
    {
        if (!existingColumns.Contains(col))
        {
            using var alter = conn.CreateCommand();
            alter.CommandText = $"ALTER TABLE Registrations ADD COLUMN {col} {type}";
            alter.ExecuteNonQuery();
        }
    }

    var deprecatedColumns = new[] { "ParticipationType", "PaperTitle" };
    foreach (var col in deprecatedColumns)
    {
        if (existingColumns.Contains(col))
        {
            using var drop = conn.CreateCommand();
            drop.CommandText = $"ALTER TABLE Registrations DROP COLUMN {col}";
            drop.ExecuteNonQuery();
        }
    }

    // Ensure AbstractSubmissions table exists (for databases created before this feature)
    using var createAbstracts = conn.CreateCommand();
    createAbstracts.CommandText = @"
        CREATE TABLE IF NOT EXISTS AbstractSubmissions (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            PresentationType TEXT NOT NULL DEFAULT 'No Preference',
            Title TEXT NOT NULL,
            Authors TEXT NOT NULL,
            Affiliations TEXT,
            CorrespondingAuthor TEXT NOT NULL,
            CorrespondingEmail TEXT NOT NULL,
            AbstractText TEXT NOT NULL,
            Keywords TEXT,
            SubmittedAt TEXT NOT NULL DEFAULT (datetime('now')),
            Status TEXT NOT NULL DEFAULT 'Pending'
        )";
    createAbstracts.ExecuteNonQuery();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Support virtual directory (set "PathBase" in appsettings.json, e.g. "/acsel")
var pathBase = app.Configuration["PathBase"];
if (!string.IsNullOrEmpty(pathBase))
    app.UsePathBase(pathBase);

// MapStaticAssets fingerprints static files for cache-busting. HTML documents need
// stable URLs, so redirect any previously published fingerprinted HTML URL to its
// canonical equivalent before serving static files.
var canonicalHtmlPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["index"] = "/",
    ["about"] = "/about.html",
    ["programme"] = "/programme.html",
    ["speakers"] = "/speakers.html",
    ["travel"] = "/travel.html",
    ["sponsorship"] = "/sponsorship.html",
    ["register"] = "/Register/International",
    ["submit"] = "/Submit"
};

var legacyHtmlPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["/index.html"] = "/",
    ["/register.html"] = "/Register/International",
    ["/submit.html"] = "/Submit"
};

var fingerprintedHtmlPath = new Regex(
    @"^/(?<page>.+)\.[a-z0-9_-]{10}\.html$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;
        string? canonicalPath = null;

        if (legacyHtmlPaths.TryGetValue(requestPath, out var legacyCanonicalPath))
        {
            canonicalPath = legacyCanonicalPath;
        }
        else
        {
            var match = fingerprintedHtmlPath.Match(requestPath);
            if (match.Success && canonicalHtmlPaths.TryGetValue(match.Groups["page"].Value, out var fingerprintCanonicalPath))
                canonicalPath = fingerprintCanonicalPath;
        }

        if (canonicalPath is not null)
        {
            context.Response.Redirect($"{context.Request.PathBase}{canonicalPath}{context.Request.QueryString}", permanent: true);
            return;
        }
    }

    await next();
});

// HTML files are excluded from MapStaticAssets so new fingerprinted document URLs
// are not generated. Serve only those excluded files here; CSS, JS, and images
// continue through MapStaticAssets with its compression and cache optimizations.
app.UseWhen(
    context => context.Request.Path.Value?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true,
    htmlFiles => htmlFiles.UseStaticFiles());

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
