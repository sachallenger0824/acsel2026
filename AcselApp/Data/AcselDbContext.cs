using Microsoft.EntityFrameworkCore;
using AcselApp.Models;

namespace AcselApp.Data
{
    public class AcselDbContext : DbContext
    {
        public AcselDbContext(DbContextOptions<AcselDbContext> options) : base(options) { }

        public DbSet<UpdateNewsItem> UpdatesNews { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<AbstractSubmission> AbstractSubmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed initial data for UpdatesNews
            modelBuilder.Entity<UpdateNewsItem>().HasData(
                new UpdateNewsItem
                {
                    Id = 1,
                    Title = "Registration is now open",
                    Content = "Registration is now open. Early bird ends 15 August 2026.",
                    PublishDate = new DateTime(2026, 3, 1),
                    IsActive = true
                },
                new UpdateNewsItem
                {
                    Id = 2,
                    Title = "Programme Schedule",
                    Content = "The preliminary programme schedule has been released.",
                    PublishDate = new DateTime(2026, 2, 15),
                    IsActive = true
                }
            );
        }
    }
}
