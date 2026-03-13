using InfoTrack.Api.Models;

namespace InfoTrack.Api.Services;

public interface IScraperOrchestrator
{
    Task<int> StartScrapeAsync(string[] locations, CancellationToken ct = default);

    Task<SearchRun?> GetRunAsync(int runId);

    Task<bool> CancelScrapeAsync(int runId);
}