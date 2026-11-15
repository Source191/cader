using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>
/// One branch from <see cref="IGit.ListBranchesAsync"/>: a local branch with its upstream, or a branch origin has, as of
/// the last fetch, with no local branch of its name. TipAt is its tip's commit time, and Mine says the tip was authored
/// with the repository's git user email; the email itself never leaves the git adapter.
/// </summary>
public abstract record GitBranch(BranchName Name, DateTimeOffset TipAt, bool Mine)
{
    public sealed record Local(BranchName Name, Upstream Upstream, DateTimeOffset TipAt, bool Mine) : GitBranch(Name, TipAt, Mine);

    public sealed record OriginOnly(BranchName Name, DateTimeOffset TipAt, bool Mine) : GitBranch(Name, TipAt, Mine);
}
