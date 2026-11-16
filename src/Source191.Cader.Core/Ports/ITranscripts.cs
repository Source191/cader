using Source191.Cader.Domain;
namespace Source191.Cader.Ports;

/// <summary>Claude Code's transcript files, one per session; a session has messages once its transcript holds a user or assistant line.</summary>
public interface ITranscripts
{
    bool HasMessages(SessionId sessionId);
}