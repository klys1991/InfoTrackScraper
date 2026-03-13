using InfoTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IScraperOrchestrator _orchestrator;

    public SearchController(IScraperOrchestrator orchestrator)
        => _orchestrator = orchestrator;

    [HttpPost]
    public async Task<IActionResult> StartSearch(
        [FromBody] StartSearchRequest request,
        CancellationToken ct)
    {
        if (request.Locations is not { Length: > 0 })
            return BadRequest("At least one location required");

        var runId = await _orchestrator.StartScrapeAsync(request.Locations, ct);
        return Accepted(new { runId });
    }

    [HttpDelete("{runId:int}")]
    public async Task<IActionResult> CancelRun(int runId)
    {
        var cancelled = await _orchestrator.CancelScrapeAsync(runId);
        return cancelled ? NoContent() : NotFound();
    }

    [HttpGet("{runId:int}")]
    public async Task<IActionResult> GetRun(int runId, CancellationToken ct)
    {
        _ = ct; // not needed — GetRunAsync creates its own scope
        var run = await _orchestrator.GetRunAsync(runId);
        if (run is null) return NotFound();

        // Project to a DTO so EF navigation cycles don't cause serialisation issues
        return Ok(new
        {
            run.Id,
            run.StartedAt,
            run.CompletedAt,
            run.Locations,
            run.Status,
            ResultCount = run.Results.Count,
            NewCount = run.Results.Count(r => r.IsNew)
        });
    }
}

public record StartSearchRequest(string[] Locations);