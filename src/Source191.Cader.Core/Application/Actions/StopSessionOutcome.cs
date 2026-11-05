using Source191.Cader.Application.Effects;

using Source191.Cader.Domain;

namespace Source191.Cader.Application.Actions;

/// <summary>Whether claude stopped the session.</summary>
public abstract record StopSessionOutcome : Outcome
{
    /// <summary>The session was stopped; it stays listed and can be resumed. TabClosed when a session tab showing it was closed too.</summary>
    public sealed record Stopped(ListedName Name, bool TabClosed) : StopSessionOutcome;

    /// <summary>Claude refused with this sentence; the session still runs and its tab stays open.</summary>
    public sealed record NotStopped(ListedName Name, string Failure) : StopSessionOutcome;
}