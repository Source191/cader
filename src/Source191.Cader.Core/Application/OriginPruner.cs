using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Collections.Concurrent;

namespace Source191.Cader.Application;

/// <summary>
/// Prunes origin's deleted branches from the records of each repository a scan showed an origin row in, at most once an
/// hour per repository counted from the attempt, so an offline machine or a repository needing sign-in costs one quick
/// failure an hour. A repository is claimed before git runs, so overlapping scans never prune it twice. A git failure
/// is recorded once per repository in this process run and never shown; nothing awaits a prune but its facts.
/// </summary>
public sealed class OriginPruner(IGit git, TimeProvider clock, DiagnosticLog log)
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly ConcurrentDictionary<FolderPath, DateTimeOffset> attempts = new();
    private readonly OncePerFolder failures = new(log, FailureSite.BackgroundTask, CodeName.For(typeof(OriginPruner)));

    public Task PruneDueAsync(IEnumerable<FolderPath> repositories)
    {
        DateTimeOffset now = clock.GetUtcNow();

        return Task.WhenAll([.. repositories.Distinct().Where(repository => TryClaim(repository, now)).Select(PruneAsync)]);
    }

    private bool TryClaim(FolderPath repository, DateTimeOffset now) =>
        attempts.TryGetValue(repository, out DateTimeOffset last)
            ? now - last >= Interval && attempts.TryUpdate(repository, now, last)
            : attempts.TryAdd(repository, now);

    // The scan's token ends with the scan; the process runner's time limit bounds the prune.
    private Task<bool> PruneAsync(FolderPath repository) =>
        failures.RunAsync(repository, async () => { await git.PruneOriginAsync(repository, CancellationToken.None); return true; }, exception => exception is GitException, _ => false);
}
