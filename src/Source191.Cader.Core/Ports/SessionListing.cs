using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>What the CLI's listing holds: the sessions, and apart from them the automated runs.</summary>
public sealed record SessionListing(IReadOnlyList<Session> Sessions, IReadOnlyList<AutomatedRun> AutomatedRuns)
{
    public static SessionListing Empty { get; } = new([], []);
}