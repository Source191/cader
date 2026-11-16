using Source191.Cader.Adapters.Claude;
using Source191.Cader.Domain;

namespace Source191.Cader.Infrastructure.Tests;

public sealed class ClaudeTranscriptsShould : IDisposable
{
    private const string UserLine = """{"type":"user","message":{"role":"user","content":"Add the Northwind orders export"},"sessionId":"5a4a7350-31ac-49b6-899f-c7d832d8ef63"}""";
    private const string TitleAndName = """
        {"type":"ai-title","aiTitle":"Northwind orders export","sessionId":"5a4a7350-31ac-49b6-899f-c7d832d8ef63"}
        {"type":"agent-name","agentName":"Northwind orders export","sessionId":"5a4a7350-31ac-49b6-899f-c7d832d8ef63"}

        """;

    private readonly string home = Path.Combine(Path.GetTempPath(), "test-transcripts-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void FindASessionsTranscriptInAnyProjectFolder()
    {
        string project = Path.Combine(home, "projects", "C--Users-me-source-repos-Northwind-Orders");
        Directory.CreateDirectory(project);
        File.WriteAllText(Path.Combine(project, "5a4a7350-31ac-49b6-899f-c7d832d8ef63.jsonl"), UserLine + "\n");
        ClaudeTranscripts transcripts = new(home);

        transcripts.HasMessages(SessionId.From("5a4a7350-31ac-49b6-899f-c7d832d8ef63")).ShouldBeTrue();
        transcripts.HasMessages(SessionId.From("00000000-0000-0000-0000-000000000000")).ShouldBeFalse();
    }

    [Fact]
    public void LookOnlyInTheFolderItWasGiven()
    {
        string other = Path.Combine(home, "other-account");
        string project = Path.Combine(home, "projects", "C--Users-me-source-repos-Northwind-Orders");
        Directory.CreateDirectory(project);
        Directory.CreateDirectory(other);
        File.WriteAllText(Path.Combine(project, "5a4a7350-31ac-49b6-899f-c7d832d8ef63.jsonl"), UserLine + "\n");
        SessionId session = SessionId.From("5a4a7350-31ac-49b6-899f-c7d832d8ef63");

        new ClaudeTranscripts(home).HasMessages(session).ShouldBeTrue();
        new ClaudeTranscripts(other).HasMessages(session).ShouldBeFalse();
    }

    [Fact]
    public void TreatATranscriptWithOnlyATitleAndANameAsNoMessages()
    {
        string project = Path.Combine(home, "projects", "C--Users-me-source-repos-Northwind-Orders");
        Directory.CreateDirectory(project);
        File.WriteAllText(Path.Combine(project, "5a4a7350-31ac-49b6-899f-c7d832d8ef63.jsonl"), TitleAndName);
        ClaudeTranscripts transcripts = new(home);

        transcripts.HasMessages(SessionId.From("5a4a7350-31ac-49b6-899f-c7d832d8ef63")).ShouldBeFalse();
    }

    [Fact]
    public void FindAnAssistantLineAfterLinesThatAreNotJson()
    {
        string project = Path.Combine(home, "projects", "C--Users-me-source-repos-Northwind-Orders");
        Directory.CreateDirectory(project);
        File.WriteAllText(Path.Combine(project, "5a4a7350-31ac-49b6-899f-c7d832d8ef63.jsonl"), """
            {"type":"ai-ti

            {}
            ["user"]
            {"type":3}
            {"type":"assistant","message":{"role":"assistant","content":"Done."}}

            """);
        ClaudeTranscripts transcripts = new(home);

        transcripts.HasMessages(SessionId.From("5a4a7350-31ac-49b6-899f-c7d832d8ef63")).ShouldBeTrue();
    }

    [Fact]
    public void TreatATranscriptItCannotReadAsHavingMessages()
    {
        string project = Path.Combine(home, "projects", "C--Users-me-source-repos-Northwind-Orders");
        Directory.CreateDirectory(project);
        string path = Path.Combine(project, "5a4a7350-31ac-49b6-899f-c7d832d8ef63.jsonl");
        File.WriteAllText(path, TitleAndName);
        ClaudeTranscripts transcripts = new(home);

        using FileStream locked = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        transcripts.HasMessages(SessionId.From("5a4a7350-31ac-49b6-899f-c7d832d8ef63")).ShouldBeTrue();
    }

    [Fact]
    public void ReadATranscriptClaudeIsStillWriting()
    {
        string project = Path.Combine(home, "projects", "C--Users-me-source-repos-Northwind-Orders");
        Directory.CreateDirectory(project);
        string path = Path.Combine(project, "5a4a7350-31ac-49b6-899f-c7d832d8ef63.jsonl");
        File.WriteAllText(path, TitleAndName);
        ClaudeTranscripts transcripts = new(home);

        using FileStream writing = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

        transcripts.HasMessages(SessionId.From("5a4a7350-31ac-49b6-899f-c7d832d8ef63")).ShouldBeFalse();
    }

    [Fact]
    public void TreatAMissingProjectsFolderAsNoMessages()
    {
        ClaudeTranscripts transcripts = new(home);

        transcripts.HasMessages(SessionId.From("5a4a7350-31ac-49b6-899f-c7d832d8ef63")).ShouldBeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(home))
        {
            Directory.Delete(home, recursive: true);
        }
    }
}