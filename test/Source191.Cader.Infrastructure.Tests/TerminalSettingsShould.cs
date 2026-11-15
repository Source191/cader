using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
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

    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 30, 15, TimeSpan.Zero);

    private readonly string root = Path.Combine(Path.GetTempPath(), "test-terminal-" + Guid.NewGuid().ToString("N"));
    private readonly TerminalSettings settings;

    public TerminalSettingsShould()
    {
        Directory.CreateDirectory(root);
        settings = new TerminalSettings(root, new FakeTimeProvider(Now));
    }

    private string StateFile => Path.Combine(root, "state.json");

    private string Backup => Path.Combine(root, "settings.json.2026-11-15T09-30-15.cader.backup");

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

    [Theory]
    [InlineData($$"""{ "guid": "{{CaderId}}", "hidden": false, "name": "Cader", "source": "Cader" }""")]
    [InlineData("""{ "guid": "{3D7A06D2-710F-51D9-90C9-DACAE2C3AB46}", "hidden": false, "name": "Cader", "source": "Cader" }""")]
    [InlineData($$"""{ "source": "Cader", "name": "Cader", "hidden": false, "guid": "{{CaderId}}" }""")]
    public async Task ListAnEntryExactlyAsSetupAddsItAsAdded(string entry)
    {
        Given(StateFile, State);
        Given(settings.Location, $$"""{ "profiles": [ { "guid": "{{PowerShellId}}" }, {{entry}} ] }""");

        TerminalProfileRecords records = await settings.ReadAsync(TestContext.Current.CancellationToken);

        records.ShouldBeOfType<TerminalProfileRecords.Read>().Added.Select(id => id.Text).ShouldBe([CaderId]);
    }

    /// <summary>Terminal adds properties to an entry when someone customises the profile, and that entry is theirs.</summary>
    [Theory]
    [InlineData($$"""{ "guid": "{{CaderId}}", "hidden": false, "name": "Cader", "source": "Cader", "icon": "mine.ico" }""")]
    [InlineData($$"""{ "guid": "{{CaderId}}", "hidden": true, "name": "Cader", "source": "Cader" }""")]
    [InlineData($$"""{ "guid": "{{CaderId}}", "hidden": "false", "name": "Cader", "source": "Cader" }""")]
    [InlineData($$"""{ "guid": "{{CaderId}}", "hidden": false, "name": "Mine", "source": "Cader" }""")]
    [InlineData($$"""{ "guid": "{{CaderId}}", "hidden": false, "name": "Cader session", "source": "Cader" }""")]
    [InlineData($$"""{ "guid": "{{CaderId}}", "hidden": false, "name": "Cader", "source": "Other" }""")]
    [InlineData($$"""{ "guid": "{{CaderId}}", "name": "Cader", "source": "Cader" }""")]
    [InlineData($$"""{ "guid": "{{CaderId}}", "hidden": false, "name": "Cader", "name": "Cader" }""")]
    [InlineData($$"""{ "guid": "{{PowerShellId}}", "hidden": false, "name": "Cader", "source": "Cader" }""")]
    public async Task LeaveOutOfAddedAnEntryNotExactlyAsSetupAddsIt(string entry)
    {
        Given(StateFile, State);
        Given(settings.Location, $$"""{ "profiles": [ {{entry}} ] }""");

        TerminalProfileRecords records = await settings.ReadAsync(TestContext.Current.CancellationToken);

        records.ShouldBeOfType<TerminalProfileRecords.Read>().Added.ShouldBeEmpty();
    }

    [Fact]
    public async Task ListBothAddedEntriesOnceAdded()
    {
        Given(StateFile, State);
        Given(settings.Location, """{ "profiles": { "list": [] } }""");

        await settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        TerminalProfileRecords.Read read = (await settings.ReadAsync(TestContext.Current.CancellationToken)).ShouldBeOfType<TerminalProfileRecords.Read>();
        read.Added.Select(id => id.Text).ShouldBe([CaderId, SessionId], ignoreOrder: true);
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

    [Fact]
    public async Task KeepACopyOfTheFileAsItWasNamedWithTheUtcTime()
    {
        string original = Lf("""
            {
                "profiles": [ { "name": "mine" } ]
            }
            """);
        Given(settings.Location, original, withByteOrderMark: true);

        TerminalSettingsEdit edit = await settings.AddProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        edit.ShouldBe(new TerminalSettingsEdit.Written(Backup));
        (await File.ReadAllBytesAsync(Backup, TestContext.Current.CancellationToken)).ShouldBe([.. Encoding.UTF8.Preamble.ToArray(), .. Encoding.UTF8.GetBytes(original)]);
    }

    /// <summary>File.Replace keeps the replaced file's creation time and access rules, which moving a new file over it would not.</summary>
    [Fact]
    [SupportedOSPlatform("windows")]
    public async Task KeepTheFilesCreationTimeAndAccessRules()
    {
        Given(settings.Location, """{ "profiles": [] }""");
        DateTime created = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        File.SetCreationTimeUtc(settings.Location, created);
        FileInfo file = new(settings.Location);
        FileSecurity security = file.GetAccessControl();
        FileSystemAccessRule guests = new(new SecurityIdentifier(WellKnownSidType.BuiltinGuestsSid, null), FileSystemRights.Read, AccessControlType.Allow);
        security.AddAccessRule(guests);
        file.SetAccessControl(security);

        await settings.AddProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        File.GetCreationTimeUtc(settings.Location).ShouldBe(created);
        new FileInfo(settings.Location).GetAccessControl().GetAccessRules(includeExplicit: true, includeInherited: false, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>()
            .ShouldContain(rule => rule.IdentityReference == guests.IdentityReference && rule.AccessControlType == AccessControlType.Allow && rule.FileSystemRights.HasFlag(FileSystemRights.Read));
    }

    [Fact]
    public async Task WriteNothingWhenTheFileChangesWhileItIsWritten()
    {
        Given(settings.Location, """{ "profiles": [] }""");
        string saved = """{ "profiles": [], "theme": "light" }""";
        TerminalSettings racing = new(root, new SavingClock(settings.Location, saved));

        TerminalSettingsEdit edit = await racing.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        edit.ShouldBe(new TerminalSettingsEdit.ChangedMeanwhile());
        (await File.ReadAllTextAsync(settings.Location, TestContext.Current.CancellationToken)).ShouldBe(saved);
        Directory.EnumerateFiles(root).Select(Path.GetFileName).ShouldBe(["settings.json"]);
    }

    /// <summary>The text goes into the first "profiles", but System.Text.Json reads the last one.</summary>
    [Fact]
    public async Task WriteNothingWhenTheNewFileWouldNotHoldTheListExpected()
    {
        string original = $$"""{ "profiles": { "list": [ { "guid": "{{PowerShellId}}" } ] }, "profiles": [] }""";
        Given(settings.Location, original);

        TerminalSettingsEdit edit = await settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        edit.ShouldBe(new TerminalSettingsEdit.NotVerified());
        ShouldHold(original, at: string.Empty, inserted: string.Empty, withByteOrderMark: false);
        Directory.EnumerateFiles(root).Select(Path.GetFileName).ShouldBe(["settings.json"]);
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

        (await settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken)).ShouldBe(new TerminalSettingsEdit.IsALink());

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

        (await settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken)).ShouldBe(new TerminalSettingsEdit.IsReadOnly());

        ShouldHold(original, at: string.Empty, inserted: string.Empty, withByteOrderMark: false);
        File.GetAttributes(settings.Location).HasFlag(FileAttributes.ReadOnly).ShouldBeTrue();
        Directory.EnumerateFiles(root).Select(Path.GetFileName).ShouldBe(["settings.json"]);
    }

    /// <summary>Every shape setup adds to, so removing what it added gives back the file byte for byte.</summary>
    public static TheoryData<string, bool> ShapesAddedTo => new()
    {
        { Lf($$"""
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

            """), false },
        { Lf("""
            {
                "profiles":
                {
                    "list": []
                }
            }
            """), false },
        { $$"""{ "profiles": { "list": [ { "guid": "{{PowerShellId}}" } ] } }""", false },
        { Lf($$"""
            {
                "schemes": { "list": [] },
                "profiles": [
                  { "guid": "{{PowerShellId}}", "list": [] }
                ]
            }
            """), false },
        { Lf($$"""
            // My Windows Terminal settings
            {
                "profiles": {
                    "list": [
                        // first
                        { "guid": "{{PowerShellId}}" }, // the shell
                        /* second */ { "guid": "{{CaderId}}", "icon": "mine.ico" } // the console
                    ]
                }
            }
            """), false },
        { Lf($$"""
            {
                "profiles": {
                    "list": [
                        { "guid": "{{PowerShellId}}" },
                    ],
                },
            }
            """), false },
        { $$"""
            {
                "profiles": {
                    "list": [
                        { "guid": "{{PowerShellId}}" }
                    ]
                }
            }
            """.ReplaceLineEndings("\r\n"), true },
    };

    [Theory]
    [MemberData(nameof(ShapesAddedTo))]
    public async Task GiveBackTheOriginalBytesWhenWhatSetupAddedIsRemoved(string original, bool withByteOrderMark)
    {
        Given(settings.Location, original, withByteOrderMark);
        await settings.AddProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        TerminalSettingsEdit edit = await settings.RemoveProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        edit.ShouldBe(new TerminalSettingsEdit.Written(Backup));
        ShouldHold(original, at: string.Empty, inserted: string.Empty, withByteOrderMark);
    }

    [Theory]
    [InlineData("first", "[\n    CADER,\n    { \"guid\": \"PS\" }\n]", "[\n    { \"guid\": \"PS\" }\n]")]
    [InlineData("a middle", "[\n    { \"guid\": \"PS\" },\n    CADER,\n    { \"name\": \"mine\" }\n]", "[\n    { \"guid\": \"PS\" },\n    { \"name\": \"mine\" }\n]")]
    [InlineData("the last", "[\n    { \"guid\": \"PS\" },\n    CADER\n]", "[\n    { \"guid\": \"PS\" }\n]")]
    [InlineData("the only", "[\n    CADER\n]", "[\n]")]
    [InlineData("the only, with a trailing comma", "[\n    CADER,\n]", "[\n]")]
    [InlineData("the only, on one line", "[ CADER ]", "[ ]")]
    [InlineData("a comma in a comment before it", "[\n    { \"guid\": \"PS\" } /* , */ ,\n    CADER\n]", "[\n    { \"guid\": \"PS\" } /* , */ \n]")]
    [InlineData("a comma in a comment after it", "[\n    CADER // mine, too\n    , { \"guid\": \"PS\" }\n]", "[ // mine, too\n     { \"guid\": \"PS\" }\n]")]
    [InlineData("the last, after a line comment", "[\n    { \"guid\": \"PS\" }, // the shell\n    CADER\n]", "[\n    { \"guid\": \"PS\" } // the shell\n]")]
    [InlineData("the last, over several lines", "[\n    { \"guid\": \"PS\" },\n    CADERLINES\n]", "[\n    { \"guid\": \"PS\" }\n]")]
    [InlineData("a middle, over several lines", "[\n    { \"guid\": \"PS\" },\n    CADERLINES,\n    { \"name\": \"mine\" }\n]", "[\n    { \"guid\": \"PS\" },\n    { \"name\": \"mine\" }\n]")]
    [InlineData("the only, over several lines", "[\n    CADERLINES\n]", "[\n]")]
    [InlineData("both, apart", "[\n    SESSION,\n    { \"guid\": \"PS\" },\n    CADER\n]", "[\n    { \"guid\": \"PS\" }\n]")]
    public async Task RemoveAnEntryAndOneCommaKeepingEveryOtherByte(string which, string list, string remaining)
    {
        _ = which;
        Given(settings.Location, ProfilesOf(list));

        TerminalSettingsEdit edit = await settings.RemoveProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        edit.ShouldBe(new TerminalSettingsEdit.Written(Backup));
        (await File.ReadAllTextAsync(settings.Location, TestContext.Current.CancellationToken)).ShouldBe(ProfilesOf(remaining));
    }

    [Fact]
    public async Task RemoveOnlyTheEntriesAskedFor()
    {
        Given(settings.Location, ProfilesOf("[\n    CADER,\n    SESSION\n]"));

        await settings.RemoveProfileEntriesAsync([TerminalProfiles.Entries[1]], TestContext.Current.CancellationToken);

        (await File.ReadAllTextAsync(settings.Location, TestContext.Current.CancellationToken)).ShouldBe(ProfilesOf("[\n    CADER\n]"));
    }

    [Theory]
    [InlineData("""[ { "guid": "PS" } ]""")]
    [InlineData($$"""[ { "guid": "{{CaderId}}", "hidden": false, "name": "Cader", "source": "Cader", "icon": "mine.ico" } ]""")]
    public async Task AnswerUnchangedAndLeaveTheFileAloneWithNothingToRemove(string list)
    {
        string original = ProfilesOf(list);
        Given(settings.Location, original);

        TerminalSettingsEdit edit = await settings.RemoveProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        edit.ShouldBe(new TerminalSettingsEdit.Unchanged());
        ShouldHold(original, at: string.Empty, inserted: string.Empty, withByteOrderMark: false);
        Directory.EnumerateFiles(root).Select(Path.GetFileName).ShouldBe(["settings.json"]);
    }

    /// <summary>
    /// The first and third: the walker edits the first "profiles" and System.Text.Json reads the last. The second: taking
    /// the line break before the entry would pull the closing bracket into the comment.
    /// </summary>
    [Theory]
    [InlineData($$"""{ "profiles": { "list": [ {{"CADER"}} ] }, "profiles": [] }""")]
    [InlineData("{ \"profiles\": [\n    { \"guid\": \"PS\" }, // the shell\n    CADER ] }")]
    [InlineData("{ \"profiles\": { \"list\": [] }, \"profiles\": [ CADER ] }")]
    public async Task WriteNothingWhenTheFileWithoutTheEntryWouldNotHoldTheListExpected(string text)
    {
        string original = Lf(text.Replace("CADER", CaderEntry, StringComparison.Ordinal).Replace("PS", PowerShellId, StringComparison.Ordinal));
        Given(settings.Location, original);

        TerminalSettingsEdit edit = await settings.RemoveProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        edit.ShouldBe(new TerminalSettingsEdit.NotVerified());
        ShouldHold(original, at: string.Empty, inserted: string.Empty, withByteOrderMark: false);
        Directory.EnumerateFiles(root).Select(Path.GetFileName).ShouldBe(["settings.json"]);
    }

    [Fact]
    public async Task RefuseToRemoveFromAReadOnlyFile()
    {
        string original = ProfilesOf("[ CADER ]");
        Given(settings.Location, original);
        File.SetAttributes(settings.Location, FileAttributes.ReadOnly);

        (await settings.RemoveProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken)).ShouldBe(new TerminalSettingsEdit.IsReadOnly());

        ShouldHold(original, at: string.Empty, inserted: string.Empty, withByteOrderMark: false);
    }

    [Fact]
    public async Task RefuseToRemoveFromALink()
    {
        string original = ProfilesOf("[ CADER ]");
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

        (await settings.RemoveProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken)).ShouldBe(new TerminalSettingsEdit.IsALink());

        (await File.ReadAllTextAsync(target, TestContext.Current.CancellationToken)).ShouldBe(original);
    }

    [Fact]
    public async Task RemoveNothingWhenTheFileChangesWhileItIsWritten()
    {
        Given(settings.Location, ProfilesOf("[ CADER ]"));
        string saved = """{ "profiles": [], "theme": "light" }""";
        TerminalSettings racing = new(root, new SavingClock(settings.Location, saved));

        TerminalSettingsEdit edit = await racing.RemoveProfileEntriesAsync(TerminalProfiles.Entries, TestContext.Current.CancellationToken);

        edit.ShouldBe(new TerminalSettingsEdit.ChangedMeanwhile());
        (await File.ReadAllTextAsync(settings.Location, TestContext.Current.CancellationToken)).ShouldBe(saved);
        Directory.EnumerateFiles(root).Select(Path.GetFileName).ShouldBe(["settings.json"]);
    }

    /// <summary>
    /// A settings file whose bare profile list is <paramref name="list"/>, CADER, SESSION and PS standing for the entries and
    /// PowerShell's id, and CADERLINES for the Cader entry as Terminal writes one over several lines.
    /// </summary>
    private static string ProfilesOf(string list)
    {
        string expanded = list.Replace("CADERLINES", CaderEntryOverLines, StringComparison.Ordinal).Replace("CADER", CaderEntry, StringComparison.Ordinal).Replace("SESSION", SessionEntry, StringComparison.Ordinal).Replace("\"PS\"", $"\"{PowerShellId}\"", StringComparison.Ordinal);

        return $"{{ \"profiles\": {expanded} }}";
    }

    private static string CaderEntryOverLines => $"{{\n        \"guid\": \"{CaderId}\",\n        \"hidden\": false,\n        \"name\": \"Cader\",\n        \"source\": \"Cader\"\n    }}";

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

    /// <summary>
    /// Stands in for Windows Terminal saving its settings while Cader writes them: the write reads the clock after it has
    /// read the file and before it replaces it, and this clock saves the file when read.
    /// </summary>
    private sealed class SavingClock(string file, string saved) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            File.WriteAllText(file, saved);

            return Now;
        }
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