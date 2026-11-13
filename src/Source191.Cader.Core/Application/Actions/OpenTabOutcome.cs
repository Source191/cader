using Source191.Cader.Application.Effects;

using Source191.Cader.Domain;

namespace Source191.Cader.Application.Actions;

/// <summary>What opening a session's tab did: opened one, switched to the open one, or could not.</summary>
public abstract record OpenTabOutcome : Outcome
{
    /// <summary>Opened in the named colour; reused when every palette colour was already taken.</summary>
    public sealed record Opened(ListedName Name, string Colour, bool ColourReused) : OpenTabOutcome;

    public sealed record SwitchedTo(ListedName Name) : OpenTabOutcome;

    public sealed record NotFound(ListedName Name) : OpenTabOutcome;
}