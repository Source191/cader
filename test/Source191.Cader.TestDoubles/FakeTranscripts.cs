using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.TestDoubles;

/// <summary>A stand-in for Claude Code's transcript files: the sessions that have been sent at least one message, and the owner each records.</summary>
public sealed class FakeTranscripts : ITranscripts
{
    private readonly HashSet<SessionId> withMessages = [];
    private readonly Dictionary<SessionId, OrganisationId> owners = [];

    public FakeTranscripts WithMessages(SessionId sessionId)
    {
        withMessages.Add(sessionId);
        return this;
    }

    public FakeTranscripts WithOwner(SessionId sessionId, OrganisationId organisation)
    {
        owners[sessionId] = organisation;
        return this;
    }

    public bool HasMessages(SessionId sessionId)
    {
        return withMessages.Contains(sessionId);
    }

    public SessionOwner OwnerOf(SessionId sessionId)
    {
        return owners.TryGetValue(sessionId, out OrganisationId? organisation) ? new SessionOwner.Recorded(organisation) : new SessionOwner.NotRecorded();
    }
}
