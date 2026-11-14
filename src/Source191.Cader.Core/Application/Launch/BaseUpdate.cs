using Source191.Cader.Domain;

namespace Source191.Cader.Application.Launch;

/// <summary>What bringing the base up to date before a new worktree branched from it came to; only a case other than <see cref="Current"/> has words.</summary>
public abstract record BaseUpdate
{
    /// <summary>Fetched, and fast-forwarded where Pull asked, or nothing was asked or needed.</summary>
    public sealed record Current : BaseUpdate;

    /// <summary>The fetch failed or ran out of time, so the worktree branched from <paramref name="Base"/> as last fetched; the reason is git's words or the time limit's.</summary>
    public sealed record NotFetched(RemoteBranch Base, string Reason) : BaseUpdate;

    /// <summary>The fetch worked, but the local branch was left where it was.</summary>
    public sealed record NotFastForwarded(BranchName Branch, FastForwardRefusal Refusal) : BaseUpdate;
}