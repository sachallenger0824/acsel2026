using Microsoft.EntityFrameworkCore;
using AcselApp.Data;

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

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
