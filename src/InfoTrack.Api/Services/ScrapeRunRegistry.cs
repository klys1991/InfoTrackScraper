using System.Collections.Concurrent;

namespace InfoTrack.Api.Services;

public interface IScrapeRunRegistry
{
    CancellationToken Register(int runId, out CancellationTokenSource cts);

    bool Cancel(int runId);

    void Unregister(int runId);
}

public class ScrapeRunRegistry : IScrapeRunRegistry
{
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _running = new();

    public CancellationToken Register(int runId, out CancellationTokenSource cts)
    {
        cts = new CancellationTokenSource();
        _running[runId] = cts;
        return cts.Token;
    }

    public bool Cancel(int runId)
    {
        if (_running.TryGetValue(runId, out var cts))
        {
            cts.Cancel();
            return true;
        }
        return false;
    }

    public void Unregister(int runId)
    {
        if (_running.TryRemove(runId, out var cts))
            cts.Dispose();
    }
}