using InfoTrack.Api.Data;
using InfoTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

// F# module InfoTrack.Scraper.Types compiles to a static class — use 'using static'
// to bring Solicitor, ParseHealth, SiteConfig, LocationResult, SolicitorModule into scope
using static InfoTrack.Scraper.Types;

namespace InfoTrack.Api.Services;

public class ScraperOrchestrator : IScraperOrchestrator
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScraperOrchestrator> _logger;
    private readonly IScrapeRunRegistry _registry;

    private static readonly SiteConfig[] Sites =
    [
        InfoTrack.Scraper.Sites.SolicitorsCom.config
    ];

    public ScraperOrchestrator(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory factory,
        ILogger<ScraperOrchestrator> logger,
        IScrapeRunRegistry registry)
    {
        _scopeFactory = scopeFactory;
        _httpClient = factory.CreateClient("scraper");
        _logger = logger;
        _registry = registry;
    }

    public async Task<int> StartScrapeAsync(string[] locations, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var run = new SearchRun
        {
            StartedAt = DateTime.UtcNow,
            Locations = locations,
            Status = "running"
        };

        db.SearchRuns.Add(run);
        await db.SaveChangesAsync(ct);

        var runCt = _registry.Register(run.Id, out _);
        _ = Task.Run(() => ExecuteScrapeAsync(run.Id, locations, runCt), CancellationToken.None);

        return run.Id;
    }

    public async Task<SearchRun?> GetRunAsync(int runId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.SearchRuns
            .Include(r => r.Results)
            .FirstOrDefaultAsync(r => r.Id == runId);
    }

    private async Task ExecuteScrapeAsync(int runId, string[] locations, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var previousRun = await db.SearchRuns
                .Where(r => r.Status == "completed" && r.Id != runId)
                .OrderByDescending(r => r.CompletedAt)
                .Include(r => r.Results)
                .FirstOrDefaultAsync(ct);

            var previousSolicitors = previousRun?.Results.Select(MapToFSharp).ToList() ?? [];

            foreach (var site in Sites)
            {
                var locationResults = await InfoTrack.Scraper.Pipeline.ScrapeAllLocationsAsync(
                    _httpClient,
                    site,
                    locations.ToFSharpList(),
                    ct);

                foreach (var locationResult in locationResults)
                {
                    var solicitors = ExtractSolicitors(locationResult);

                    var diff = InfoTrack.Scraper.Pipeline.DiffSolicitors(
                        previousSolicitors.ToFSharpList(),
                        solicitors.ToFSharpList());

                    var newNames = diff.NewSolicitors.Select(s => s.Name).ToHashSet();

                    await UpsertBaselineAsync(db, locationResult, solicitors.Count, GetHealthLabel(locationResult.Health), ct);

                    var records = solicitors.Select(s => new SolicitorRecord
                    {
                        SearchRunId = runId,
                        Name = s.Name,
                        Location = s.Location,
                        SiteId = site.SiteId,
                        Phone = s.Phone.ValueOrDefault(),
                        Email = s.Email.ValueOrDefault(),
                        Address = s.Address.ValueOrDefault(),
                        Rating = s.Rating.ValueOrDefault(),
                        ReviewCount = s.ReviewCount.ValueOrDefault(),
                        Website = s.Website.ValueOrDefault(),
                        IsNew = newNames.Contains(s.Name),
                        ParseHealth = GetHealthLabel(locationResult.Health)
                    });

                    db.SolicitorRecords.AddRange(records);
                }
            }

            var run = await db.SearchRuns.FindAsync([runId], ct);
            if (run is not null)
            {
                run.Status = "completed";
                run.CompletedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            var status = ex is OperationCanceledException ? "cancelled" : "failed";
            if (status == "failed")
                _logger.LogError(ex, "Scrape run {RunId} failed", runId);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var run = await db.SearchRuns.FindAsync([runId]);
                if (run is not null)
                {
                    run.Status = status;
                    run.CompletedAt = DateTime.UtcNow;
                }
                await db.SaveChangesAsync();
            }
            catch (Exception inner)
            {
                _logger.LogError(inner, "Failed to mark run {RunId} as {Status}", runId, status);
            }
        }
        finally
        {
            _registry.Unregister(runId);
        }
    }

    public Task<bool> CancelScrapeAsync(int runId)
        => Task.FromResult(_registry.Cancel(runId));

    private static List<Solicitor> ExtractSolicitors(LocationResult result)
    {
        return result.Health switch
        {
            ParseHealth.Healthy h => [.. h.Item],
            ParseHealth.Degraded d => [.. d.Item1],
            _ => []
        };
    }

    private static string GetHealthLabel(ParseHealth health) =>
        health switch
        {
            ParseHealth.Healthy _ => "healthy",
            ParseHealth.Degraded _ => "degraded",
            ParseHealth.StructureChanged _ => "structure_changed",
            ParseHealth.Empty _ => "empty",
            _ => "unknown"
        };

    private static async Task UpsertBaselineAsync(
        AppDbContext db,
        LocationResult result,
        int count,
        string health,
        CancellationToken ct)
    {
        var baseline = await db.LocationBaselines
            .FirstOrDefaultAsync(b =>
                b.Location == result.Location &&
                b.SiteId == result.SiteId, ct);

        if (baseline is null)
        {
            db.LocationBaselines.Add(new LocationBaseline
            {
                Location = result.Location,
                SiteId = result.SiteId,
                LastKnownCount = count,
                LastHealth = health,
                LastVerified = DateTime.UtcNow
            });
        }
        else
        {
            baseline.LastKnownCount = count;
            baseline.LastHealth = health;
            baseline.LastVerified = DateTime.UtcNow;
        }
    }

    private static Solicitor MapToFSharp(SolicitorRecord r) =>
        SolicitorModule.Create(
            r.Name,
            r.Location,
            r.Phone.ToFSharpOption(),
            r.Email.ToFSharpOption(),
            r.Address.ToFSharpOption(),
            r.Rating.ToFSharpOption(),
            r.ReviewCount.ToFSharpOption(),
            r.Website.ToFSharpOption());
}