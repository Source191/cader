using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.TestDoubles;

/// <summary>A stand-in for Claude Code's transcript files: the sessions that have been sent at least one message.</summary>
public sealed class FakeTranscripts : ITranscripts
{
    private readonly HashSet<SessionId> withMessages = [];

    public FakeTranscripts WithMessages(SessionId sessionId)
    {
        withMessages.Add(sessionId);
        return this;
    }

    public bool HasMessages(SessionId sessionId)
    {
        return withMessages.Contains(sessionId);
    }
}