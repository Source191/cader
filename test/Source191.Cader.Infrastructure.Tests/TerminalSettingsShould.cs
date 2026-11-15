using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Domain;
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

    [Fact]
    public async Task AddAnEntryAfterTheLastElementIndentedLikeTheFirst()
    {
        string original = Lf($$"""
            {
                "profiles": {
                    "list": [
                        {
                            "guid": "{{PowerShellId}}",
                            "name": "Windows PowerShell"
                        }
                    ]
                }
            }

            """);
        Given(settings.Location, original);

        await settings.AddProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        ShouldHold(original, at: "            }", inserted: $",\n            {SessionEntry}", withByteOrderMark: false);
    }

    [Fact]
    public async Task AddEachEntryOnALineOfItsOwn()
    {
        string original = Lf($$"""
            {
                "profiles": {
                    "list": [
                        { "guid": "{{PowerShellId}}" }
                    ]
                }
            }
            """);
        Given(settings.Location, original);

        await settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        ShouldHold(original, at: "\" }", inserted: $",\n            {CaderEntry},\n            {SessionEntry}", withByteOrderMark: false);
    }

    [Fact]
    public async Task AddToAnEmptyListAfterItsOpeningBracketTwelveSpacesIn()
    {
        string original = Lf("""
            {
                "profiles":
                {
                    "list": []
                }
            }
            """);
        Given(settings.Location, original);

        await settings.AddProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        ShouldHold(original, at: "\"list\": [", inserted: $"\n            {SessionEntry}", withByteOrderMark: false);
    }

    [Fact]
    public async Task AddToAListOnOneLine()
    {
        string original = $$"""{ "profiles": { "list": [ { "guid": "{{PowerShellId}}" } ] } }""";
        Given(settings.Location, original);

        await settings.AddProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        ShouldHold(original, at: "\" }", inserted: $",\n{SessionEntry}", withByteOrderMark: false);
    }

    [Fact]
    public async Task AddToABareProfilesArrayAndNotToAListNestedElsewhere()
    {
        string original = Lf($$"""
            {
                "schemes": { "list": [] },
                "profiles": [
                  { "guid": "{{PowerShellId}}", "list": [] }
                ]
            }
            """);
        Given(settings.Location, original);

        await settings.AddProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        ShouldHold(original, at: "[] }", inserted: $",\n      {SessionEntry}", withByteOrderMark: false);
    }

    [Fact]
    public async Task KeepCommentsBetweenAndAfterTheElements()
    {
        string original = Lf($$"""
            // My Windows Terminal settings
            {
                "profiles": {
                    "list": [
                        // first
                        { "guid": "{{PowerShellId}}" }, // the shell
                        /* second */ { "guid": "{{CaderId}}" } // the console
                    ]
                }
            }
            """);
        Given(settings.Location, original);

        await settings.AddProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        ShouldHold(original, at: $"{CaderId}\" }}", inserted: $",\n            {SessionEntry}", withByteOrderMark: false);
    }

    [Fact]
    public async Task KeepATrailingComma()
    {
        string original = Lf($$"""
            {
                "profiles": {
                    "list": [
                        { "guid": "{{PowerShellId}}" },
                    ],
                },
            }
            """);
        Given(settings.Location, original);

        await settings.AddProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        ShouldHold(original, at: "\" }", inserted: $",\n            {SessionEntry}", withByteOrderMark: false);
    }

    [Fact]
    public async Task KeepCrlfLineEndingsAndAByteOrderMark()
    {
        string original = $$"""
            {
                "profiles": {
                    "list": [
                        { "guid": "{{PowerShellId}}" }
                    ]
                }
            }
            """.ReplaceLineEndings("\r\n");
        Given(settings.Location, original, withByteOrderMark: true);

        await settings.AddProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        ShouldHold(original, at: "\" }", inserted: $",\r\n            {SessionEntry}", withByteOrderMark: true);
    }

    [Fact]
    public async Task EscapeAQuoteInAName()
    {
        string original = Lf("""
            {
                "profiles": [
                    { "name": "mine" }
                ]
            }
            """);
        Given(settings.Location, original);
        TerminalProfileEntry quoted = new(TerminalProfileId.ForFragment("Cader", "Say \"hi\""), "Say \"hi\"", "Cader");

        await settings.AddProfileEntriesAsync([quoted], TestContext.Current.CancellationToken);

        ShouldHold(original, at: "\" }", inserted: $",\n        {quoted.Json}", withByteOrderMark: false);
        quoted.Json.ShouldContain("\"name\": \"Say \\\"hi\\\"\"");
    }

    [Fact]
    public async Task ListTheAddedIdsWhenReadAfterwards()
    {
        Given(StateFile, State);
        Given(settings.Location, Lf($$"""
            {
                "profiles": { "list": [ { "guid": "{{PowerShellId}}" } ] }
            }
            """));

        await settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        ShouldRead(await settings.ReadAsync(TestContext.Current.CancellationToken), generated: [PowerShellId, CaderId, SessionId], listed: [PowerShellId, CaderId, SessionId]);
    }

    [Theory]
    [InlineData("{ \"theme\": \"dark\" } // no profiles")]
    [InlineData("{ \"profiles\": [ ")]
    [InlineData("{ \"profiles\": { \"defaults\": {} }, \"profiles\": [] }")]
    public async Task RefuseAFileWithNoReadableProfileListAndLeaveItAlone(string original)
    {
        Given(settings.Location, original);

        IOException refused = await Should.ThrowAsync<IOException>(() => settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken));

        refused.Message.ShouldContain(settings.Location);
        ShouldHold(original, at: string.Empty, inserted: string.Empty, withByteOrderMark: false);
    }

    [Fact]
    public async Task RefuseASettingsFileThatIsALinkAndWriteNothing()
    {
        string original = """{ "profiles": [] }""";
        string dotfiles = Path.Combine(root, "dotfiles");
        Directory.CreateDirectory(dotfiles);
        string target = Path.Combine(dotfiles, "settings.json");
        Given(target, original);
        try
        {
            File.CreateSymbolicLink(settings.Location, target);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Assert.Skip("Windows refused a symbolic link: it needs Developer Mode or an administrator.");
        }

        await Should.ThrowAsync<SettingsFileIsALinkException>(() => settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken));

        new FileInfo(settings.Location).LinkTarget.ShouldBe(target);
        (await File.ReadAllTextAsync(target, TestContext.Current.CancellationToken)).ShouldBe(original);
        Directory.EnumerateFiles(root).Select(Path.GetFileName).ShouldBe(["settings.json"]);
    }

    [Fact]
    public async Task RefuseAReadOnlyFileAndLeaveItAlone()
    {
        string original = """{ "profiles": [] }""";
        Given(settings.Location, original);
        File.SetAttributes(settings.Location, FileAttributes.ReadOnly);

        await Should.ThrowAsync<UnauthorizedAccessException>(() => settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken));

        ShouldHold(original, at: string.Empty, inserted: string.Empty, withByteOrderMark: false);
    }

    private static string CaderEntry => $$"""{ "guid": "{{CaderId}}", "hidden": false, "name": "Cader", "source": "Cader" }""";

    private static string SessionEntry => $$"""{ "guid": "{{SessionId}}", "hidden": false, "name": "Cader session", "source": "Cader" }""";

    private static string Lf(string text) => text.ReplaceLineEndings("\n");

    /// <summary>
    /// The file is the original with exactly <paramref name="inserted"/> after the last place <paramref name="at"/>
    /// ends, byte for byte, so taking the insertion out gives the original back.
    /// </summary>
    private void ShouldHold(string original, string at, string inserted, bool withByteOrderMark)
    {
        int position = original.LastIndexOf(at, StringComparison.Ordinal) + at.Length;
        string expected = original.Insert(position, inserted);
        byte[] expectedBytes = [.. withByteOrderMark ? Encoding.UTF8.Preamble.ToArray() : [], .. Encoding.UTF8.GetBytes(expected)];

        Encoding.UTF8.GetString(File.ReadAllBytes(settings.Location)).ShouldBe(Encoding.UTF8.GetString(expectedBytes));
        File.ReadAllBytes(settings.Location).ShouldBe(expectedBytes);
        Directory.EnumerateFiles(root).Select(Path.GetFileName).ShouldNotContain(name => name!.Contains("tmp", StringComparison.OrdinalIgnoreCase));
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