using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// The fetch of origin's default branch the wizard starts early, once a repository is chosen, and the launch's own fetch,
/// which joins it. One early fetch per repository runs at a time, on a pool thread with no cancellation (the git time
/// limit still ends it), and never faults: git's refusals and the time limit only mark it failed. The launch waits for
/// one still running, or takes one that succeeded within the last minute for the same remote branch, and otherwise
/// fetches itself, so its own failure still reaches the status line with git's words. The wizard starts its fetch on the
/// UI thread while launches read on pool threads, so the table sits behind a lock.
/// </summary>
public sealed class BaseFetches(IGit git, TimeProvider clock)
{
    private static readonly TimeSpan Fresh = TimeSpan.FromMinutes(1);

    private readonly Lock gate = new();
    private readonly Dictionary<FolderPath, Task<EarlyFetch>> early = [];

    /// <summary>Starts fetching the repository's default branch from origin unless a fetch is already running there; the task ends when it does, and never faults.</summary>
    public Task Begin(FolderPath repository)
    {
        lock (gate)
        {
            if (early.TryGetValue(repository, out Task<EarlyFetch>? running) && !running.IsCompleted)
            {
                return running;
            }

            Task<EarlyFetch> started = Task.Run(() => FetchEarlyAsync(repository));
            early[repository] = started;

            return started;
        }
    }

    /// <summary>The launch's fetch: the early one when it is running, or succeeded within the last minute, for this remote branch; otherwise git's own, whose failure is thrown.</summary>
    public async Task FetchAsync(FolderPath repository, RemoteBranch remoteBranch, CancellationToken cancellationToken)
    {
        Task<EarlyFetch>? begun;
        lock (gate)
        {
            begun = early.GetValueOrDefault(repository);
        }

        if (begun is not null && await begun.WaitAsync(cancellationToken) is EarlyFetch.Fetched fetched && fetched.Branch == remoteBranch && clock.GetUtcNow() - fetched.At < Fresh)
        {
            return;
        }

        await git.FetchBranchAsync(repository, remoteBranch, cancellationToken);
    }

    private async Task<EarlyFetch> FetchEarlyAsync(FolderPath repository)
    {
        try
        {
            string? remoteDefault = await git.DefaultRemoteBranchAsync(repository.Value, CancellationToken.None);
            if (!RemoteBranch.TryParse(remoteDefault, out RemoteBranch? remoteBranch))
            {
                return new EarlyFetch.NotFetched();
            }

            await git.FetchBranchAsync(repository, remoteBranch, CancellationToken.None);

            return new EarlyFetch.Fetched(remoteBranch, clock.GetUtcNow());
        }
        catch (Exception failure) when (failure is GitException or ProcessTimedOutException)
        {
            return new EarlyFetch.NotFetched();
        }
    }

    /// <summary>What an early fetch came to: the branch it fetched and when it finished, or nothing fetched.</summary>
    private abstract record EarlyFetch
    {
        public sealed record Fetched(RemoteBranch Branch, DateTimeOffset At) : EarlyFetch;

        public sealed record NotFetched : EarlyFetch;
    }
}
