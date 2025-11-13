using HolidayApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HolidayApp.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Holiday> Holidays { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Holiday>()
                .HasIndex(h => new { h.CountryCode, h.Date });

            modelBuilder.Entity<Holiday>()
                .HasIndex(h => h.CountryCode);

            // Ensure deduplication by country + date + local name
            modelBuilder.Entity<Holiday>()
                .HasAlternateKey(h => new { h.CountryCode, h.Date, h.LocalName });
        }
    }
}
