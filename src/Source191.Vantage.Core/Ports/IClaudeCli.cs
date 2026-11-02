using Source191.Vantage.Domain;

namespace Source191.Vantage.Ports;

/// <summary>
/// The <see href="https://code.claude.com/docs/en/cli-reference">documented Claude Code CLI surface</see> Vantage
/// depends on. The files Vantage reads, such as transcripts and settings.json, have their own ports.
/// The convention: ListSessionsAsync, StopAsync and RemoveAsync throw <see cref="ClaudeCliException"/> with claude's own
/// words when claude exits with an error; StartBackgroundAsync and ResumeBackgroundAsync answer a result in every case;
/// VersionAsync answers NotRead; AttachAsync answers the exit code.
/// </summary>
public interface IClaudeCli
{
    /// <summary>Throws <see cref="ClaudeCliException"/> when claude exits with an error, or a JSON exception when its output is not the listing.</summary>
    Task<SessionListing> ListSessionsAsync(CancellationToken cancellationToken);

    /// <summary>Answers a <see cref="StartResult"/> in every case, including a refusal.</summary>
    Task<StartResult> StartBackgroundAsync(string directory, SessionName name, OpeningPrompt openingPrompt, CancellationToken cancellationToken);

    /// <summary>The attach's exit code; never throws for a non-zero one.</summary>
    Task<int> AttachAsync(string jobId, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="ClaudeCliException"/> when claude refuses, for example for a job it does not know.</summary>
    Task StopAsync(string jobId, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="ClaudeCliException"/> when claude refuses, for example for a job it does not know.</summary>
    Task RemoveAsync(string jobId, CancellationToken cancellationToken);

    /// <summary>Continues a saved conversation under the supervisor, under the display name when the session is Named and with none when it is Unnamed.</summary>
    Task<ResumeResult> ResumeBackgroundAsync(string sessionId, string directory, ListedName name, CancellationToken cancellationToken);

    /// <summary>The Claude Code version, such as "2.1.310", as printed; NotRead when the CLI cannot be run, is cancelled or prints none.</summary>
    Task<ClaudeVersionReport> VersionAsync(CancellationToken cancellationToken);
}