namespace Source191.Cader.Domain;

/// <summary>A local branch's upstream as git last recorded it: none set, a remote branch it tracks, or one deleted on origin by the last fetch with prune.</summary>
public abstract record Upstream
{
    public sealed record None : Upstream;

    public sealed record Tracking(RemoteBranch Branch) : Upstream;

    public sealed record Gone : Upstream;
}
