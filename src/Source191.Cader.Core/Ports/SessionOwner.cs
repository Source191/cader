using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>The organisation a session's transcript says owns it, or that the transcript records none (an older Claude Code, or no transcript yet).</summary>
public abstract record SessionOwner
{
    public sealed record Recorded(OrganisationId Organisation) : SessionOwner;

    public sealed record NotRecorded : SessionOwner;
}
