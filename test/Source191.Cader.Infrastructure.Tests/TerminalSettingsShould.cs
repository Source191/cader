using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Ports;
using System.Text;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>The real file system behind ITerminalSettings, on a throwaway folder standing in for Windows Terminal's LocalState.</summary>
[Trait("Category", "Integration")]
public sealed class TerminalSettingsShould : IDisposable
{
    private const string CaderId = "{3d7a06d2-710f-51d9-90c9-dacae2c3ab46}";
    private const string SessionId = "{cdf8708d-a821-5c97-95d0-fea560807dea}";
    private const string PowerShellId = "{61c54bbd-c2c6-5271-96e7-009a87ff44bf}";

    private const string State = $$"""
        {
            "generatedProfiles": [ "{{PowerShellId}}", "{{CaderId}}", "{{SessionId}}" ]
        }
        """;

    private readonly string root = Path.Combine(Path.GetTempPath(), "test-terminal-" + Guid.NewGuid().ToString("N"));
    private readonly TerminalSettings settings;

    public TerminalSettingsShould()
    {
        Directory.CreateDirectory(root);
        settings = new TerminalSettings(root);
    }

    private string StateFile => Path.Combine(root, "state.json");

    [Fact]
    public void LiveInTheLocalStateFolder()
    {
        settings.Location.ShouldBe(Path.Combine(root, "settings.json"));
    }

    [Fact]
    public async Task AnswerMissingWithNoStateFile()
    {
        Given(settings.Location, "{ \"profiles\": { \"list\": [] } }");

        (await settings.ReadAsync(TestContext.Current.CancellationToken)).ShouldBe(new TerminalProfileRecords.Missing());
    }

    [Fact]
    public async Task AnswerMissingWithNoSettingsFile()
    {
        Given(StateFile, State);

        (await settings.ReadAsync(TestContext.Current.CancellationToken)).ShouldBe(new TerminalProfileRecords.Missing());
    }

    [Fact]
    public async Task ReadTheGeneratedAndListedIdsHiddenEntriesIncluded()
    {
        Given(StateFile, State);
        Given(settings.Location, $$"""
            {
                "profiles": {
                    "defaults": {},
                    "list": [
                        { "guid": "{{PowerShellId}}", "name": "Windows PowerShell" },
                        { "guid": "{{CaderId}}", "hidden": true, "name": "Cader", "source": "Cader" }
                    ]
                }
            }
            """);

        TerminalProfileRecords records = await settings.ReadAsync(TestContext.Current.CancellationToken);

        ShouldRead(records, generated: [PowerShellId, CaderId, SessionId], listed: [PowerShellId, CaderId]);
    }

    [Fact]
    public async Task ReadABareProfilesArray()
    {
        Given(StateFile, State);
        Given(settings.Location, $$"""{ "profiles": [ { "guid": "{{SessionId}}" } ] }""");

        TerminalProfileRecords records = await settings.ReadAsync(TestContext.Current.CancellationToken);

        ShouldRead(records, generated: [PowerShellId, CaderId, SessionId], listed: [SessionId]);
    }

    [Fact]
    public async Task ReadAFileWithCommentsATrailingCommaAndAByteOrderMark()
    {
        Given(StateFile, State);
        Given(settings.Location, $$"""
            // Windows Terminal's settings
            {
                "profiles": {
                    /* mine */
                    "list": [
                        { "guid": "{{PowerShellId}}" }, // the shell
                    ],
                },
            }
            """, withByteOrderMark: true);

        TerminalProfileRecords records = await settings.ReadAsync(TestContext.Current.CancellationToken);

        ShouldRead(records, generated: [PowerShellId, CaderId, SessionId], listed: [PowerShellId]);
    }

    [Fact]
    public async Task LeaveOutAnEntryWhoseGuidDoesNotParse()
    {
        Given(StateFile, State);
        Given(settings.Location, $$"""{ "profiles": { "list": [ { "guid": "not a guid" }, { "name": "no guid" }, 7, { "guid": "{{CaderId}}" } ] } }""");

        TerminalProfileRecords records = await settings.ReadAsync(TestContext.Current.CancellationToken);

        ShouldRead(records, generated: [PowerShellId, CaderId, SessionId], listed: [CaderId]);
    }

    [Fact]
    public async Task ReadAStateFileWithNoGeneratedProfilesAsNoneGenerated()
    {
        Given(StateFile, "{}");
        Given(settings.Location, """{ "profiles": [] }""");

        TerminalProfileRecords records = await settings.ReadAsync(TestContext.Current.CancellationToken);

        ShouldRead(records, generated: [], listed: []);
    }

    [Theory]
    [InlineData("{ \"profiles\": [ ", State)]
    [InlineData("{ \"profiles\": [] }", "not json")]
    [InlineData("{ \"theme\": \"dark\" }", State)]
    [InlineData("{ \"profiles\": { \"defaults\": {} } }", State)]
    [InlineData("[]", State)]
    public async Task AnswerUnreadableWithTheReason(string settingsText, string stateText)
    {
        Given(StateFile, stateText);
        Given(settings.Location, settingsText);

        TerminalProfileRecords records = await settings.ReadAsync(TestContext.Current.CancellationToken);

        records.ShouldBeOfType<TerminalProfileRecords.Unreadable>().Reason.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>Written without a byte order mark unless asked, as Terminal writes its files.</summary>
    private static void Given(string file, string text, bool withByteOrderMark = false)
    {
        File.WriteAllText(file, text, new UTF8Encoding(withByteOrderMark));
    }

    private static void ShouldRead(TerminalProfileRecords records, string[] generated, string[] listed)
    {
        TerminalProfileRecords.Read read = records.ShouldBeOfType<TerminalProfileRecords.Read>();
        read.Generated.Select(id => id.Text).ShouldBe(generated, ignoreOrder: true);
        read.Listed.Select(id => id.Text).ShouldBe(listed, ignoreOrder: true);
    }

    public void Dispose()
    {
        foreach (string file in Directory.EnumerateFiles(root))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(root, recursive: true);
    }
}
