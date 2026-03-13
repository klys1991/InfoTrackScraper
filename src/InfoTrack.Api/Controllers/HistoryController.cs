using InfoTrack.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfoTrack.Api.Controllers;

[ApiController]
[Route("api/history")]
public class HistoryController : ControllerBase
{
    private readonly AppDbContext _db;

    public HistoryController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var runs = await _db.SearchRuns
            .OrderByDescending(r => r.StartedAt)
            .Take(20)
            .Select(r => new
            {
                r.Id,
                r.StartedAt,
                r.CompletedAt,
                r.Status,
                r.Locations,
                ResultCount = r.Results.Count,
                NewCount = r.Results.Count(x => x.IsNew)
            })
            .ToListAsync(ct);

        return Ok(runs);
    }

    [HttpGet("{runId:int}/results")]
    public async Task<IActionResult> GetResults(
        int runId,
        [FromQuery] string? location,
        [FromQuery] string? sortBy,
        CancellationToken ct)
    {
        var query = _db.SolicitorRecords
            .Where(r => r.SearchRunId == runId);

        if (!string.IsNullOrEmpty(location))
            query = query.Where(r => r.Location == location);

        query = sortBy switch
        {
            "rating" => query.OrderByDescending(r => r.Rating),
            "name" => query.OrderBy(r => r.Name),
            "location" => query.OrderBy(r => r.Location),
            _ => query.OrderBy(r => r.Location).ThenBy(r => r.Name)
        };

        var results = await query.ToListAsync(ct);
        return Ok(results);
    }

    [HttpGet("{runId:int}/summary")]
    public async Task<IActionResult> GetSummary(int runId, CancellationToken ct)
    {
        var run = await _db.SearchRuns.FindAsync([runId], ct);
        if (run is null) return NotFound();

        var summary = await _db.SolicitorRecords
            .Where(r => r.SearchRunId == runId)
            .GroupBy(r => r.Location)
            .Select(g => new
            {
                Location = g.Key,
                Total = g.Count(),
                NewCount = g.Count(r => r.IsNew),
                AverageRating = g.Average(r => r.Rating),
                ParseHealth = g.Select(r => r.ParseHealth).FirstOrDefault() ?? "empty"
            })
            .ToListAsync(ct);

        // Locations that were searched but returned no solicitors — surface their health from the baseline
        var foundLocations = summary.Select(s => s.Location).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var emptyLocations = run.Locations.Where(l => !foundLocations.Contains(l)).ToList();

        if (emptyLocations.Count > 0)
        {
            var baselines = await _db.LocationBaselines
                .Where(b => emptyLocations.Contains(b.Location))
                .ToDictionaryAsync(b => b.Location, ct);

            var emptyEntries = emptyLocations.Select(l => new
            {
                Location = l,
                Total = 0,
                NewCount = 0,
                AverageRating = (double?)null,
                ParseHealth = baselines.TryGetValue(l, out var b) ? b.LastHealth : "empty"
            });

            return Ok(summary.Concat(emptyEntries));
        }

        return Ok(summary);
    }
}