using Microsoft.EntityFrameworkCore;
using AcselApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

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
        ["ParticipationType"] = "TEXT",
        ["PaperTitle"] = "TEXT",
        ["PaymentMethod"] = "TEXT"
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
