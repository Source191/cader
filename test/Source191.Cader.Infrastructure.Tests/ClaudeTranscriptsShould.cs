using Source191.Cader.Adapters.Claude;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

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

    [Fact]
    public void ReadTheOwnerFromTheSessionOwnerLine()
    {
        WriteTranscript(TitleAndName + OwnerLine(NorthwindOrganisation) + UserLine + "\n");

        new ClaudeTranscripts(home).OwnerOf(Session).ShouldBe(new SessionOwner.Recorded(OrganisationId.From(NorthwindOrganisation)));
    }

    [Fact]
    public void TakeTheFirstOwnerLine()
    {
        WriteTranscript(OwnerLine(NorthwindOrganisation) + OwnerLine(ContosoOrganisation));

        new ClaudeTranscripts(home).OwnerOf(Session).ShouldBe(new SessionOwner.Recorded(OrganisationId.From(NorthwindOrganisation)));
    }

    [Fact]
    public void FindAnOwnerLineAfterManyMessages()
    {
        WriteTranscript(string.Concat(Enumerable.Repeat(UserLine + "\n", 2000)) + OwnerLine(NorthwindOrganisation));

        new ClaudeTranscripts(home).OwnerOf(Session).ShouldBe(new SessionOwner.Recorded(OrganisationId.From(NorthwindOrganisation)));
    }

    [Fact]
    public void RecordNoOwnerForATranscriptFromAnOlderClaude()
    {
        WriteTranscript(TitleAndName + UserLine + "\n");

        new ClaudeTranscripts(home).OwnerOf(Session).ShouldBe(new SessionOwner.NotRecorded());
    }

    [Theory]
    [InlineData("""{"type":"session-owner","accountUuid":"a1b2c3d4-0000-4000-8000-000000000001"}""")]
    [InlineData("""{"type":"session-owner","organizationUuid":"northwind"}""")]
    [InlineData("""{"type":"session-owner","organizationUuid":7}""")]
    [InlineData("""{"type":"session-own""")]
    public void RecordNoOwnerForALineItCannotRead(string line)
    {
        WriteTranscript(line + "\n" + UserLine + "\n");

        new ClaudeTranscripts(home).OwnerOf(Session).ShouldBe(new SessionOwner.NotRecorded());
    }

    [Fact]
    public void RecordNoOwnerWithoutATranscript()
    {
        new ClaudeTranscripts(home).OwnerOf(Session).ShouldBe(new SessionOwner.NotRecorded());
    }

    [Fact]
    public void RecordNoOwnerForATranscriptItCannotRead()
    {
        string path = WriteTranscript(OwnerLine(NorthwindOrganisation));

        using FileStream locked = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        new ClaudeTranscripts(home).OwnerOf(Session).ShouldBe(new SessionOwner.NotRecorded());
    }

    [Fact]
    public void KeepAnOwnerOnceRead()
    {
        string path = WriteTranscript(OwnerLine(NorthwindOrganisation));
        ClaudeTranscripts transcripts = new(home);
        transcripts.OwnerOf(Session);

        File.Delete(path);

        transcripts.OwnerOf(Session).ShouldBe(new SessionOwner.Recorded(OrganisationId.From(NorthwindOrganisation)));
    }

    [Fact]
    public void ReadATranscriptWithNoOwnerAgainOnlyOnceItChanges()
    {
        string path = WriteTranscript(UserLine + "\n");
        DateTime written = File.GetLastWriteTimeUtc(path);
        ClaudeTranscripts transcripts = new(home);
        transcripts.OwnerOf(Session);

        // Same length and time, so the answer kept is used; then a real append is read.
        File.WriteAllText(path, UserLine.Replace("orders", "ORDERS", StringComparison.Ordinal) + "\n");
        File.SetLastWriteTimeUtc(path, written);
        transcripts.OwnerOf(Session).ShouldBe(new SessionOwner.NotRecorded());

        File.AppendAllText(path, OwnerLine(NorthwindOrganisation));
        transcripts.OwnerOf(Session).ShouldBe(new SessionOwner.Recorded(OrganisationId.From(NorthwindOrganisation)));
    }

    [Fact]
    public void FindAMessageAfterALongLineWithoutDecodingIt()
    {
        WriteTranscript(LongLine("summary") + UserLine + "\n");
        ClaudeTranscripts transcripts = new(home);
        bool found = false;

        long allocated = Allocations.Of(() => found = transcripts.HasMessages(Session));

        found.ShouldBeTrue();
        allocated.ShouldBeLessThan(32 * 1024);
    }

    private const string NorthwindOrganisation = "6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91";
    private const string ContosoOrganisation = "0b9d4e2f-7a6c-4f3e-8d1b-5c2a9e7f4b60";

    private static readonly SessionId Session = SessionId.From("5a4a7350-31ac-49b6-899f-c7d832d8ef63");

    private static string OwnerLine(string organisation) => $$"""{"type":"session-owner","accountUuid":"a1b2c3d4-0000-4000-8000-000000000001","organizationUuid":"{{organisation}}","sessionId":"5a4a7350-31ac-49b6-899f-c7d832d8ef63"}""" + "\n";

    /// <summary>A line of about 200 KB of the given type, as a long pasted message or summary makes.</summary>
    private static string LongLine(string type) => $$"""{"type":"{{type}}","text":"{{new string('x', 200 * 1024)}}"}""" + "\n";

    private string WriteTranscript(string text)
    {
        string project = Path.Combine(home, "projects", "C--Users-me-source-repos-Northwind-Orders");
        Directory.CreateDirectory(project);
        string path = Path.Combine(project, "5a4a7350-31ac-49b6-899f-c7d832d8ef63.jsonl");
        File.WriteAllText(path, text);

        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(home))
        {
            Directory.Delete(home, recursive: true);
        }
    }
}