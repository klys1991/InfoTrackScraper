using InfoTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoTrack.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<SearchRun> SearchRuns => Set<SearchRun>();
    public DbSet<SolicitorRecord> SolicitorRecords => Set<SolicitorRecord>();
    public DbSet<LocationBaseline> LocationBaselines => Set<LocationBaseline>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchRun>()
            .Property(s => s.Locations)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

        modelBuilder.Entity<SolicitorRecord>()
            .HasIndex(s => new { s.SearchRunId, s.Location });

        modelBuilder.Entity<LocationBaseline>()
            .HasIndex(b => new { b.Location, b.SiteId })
            .IsUnique();
    }
}