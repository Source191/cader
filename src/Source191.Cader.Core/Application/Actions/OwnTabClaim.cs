using Source191.Cader.Domain;

namespace Source191.Cader.Application.Actions;

/// <summary>Cader's own tab claimed in a colour, or left unregistered, when it is titled alone.</summary>
public abstract record OwnTabClaim
{
    public sealed record Claimed(TabColour Colour) : OwnTabClaim;

    public sealed record Unregistered : OwnTabClaim;
}