using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

public sealed class SetupFactsReaderShould
{
    private const string SettingsFile = @"C:\Users\me\AppData\Local\Cader\config.json";

    private readonly DiagnosticLog log = new(new FakeTimeProvider(Fixtures.Noon), []);

    [Fact]
    public async Task ReadEverythingSetUp()
    {
        FakeTerminalFragment fragment = new FakeTerminalFragment().Installed();
        InMemoryState state = new();
        DateTimeOffset nine = new(2026, 10, 30, 9, 12, 0, TimeSpan.Zero);
        await state.WriteUpdateCheckAsync(new UpdateCheckRecord.Checked(nine, new NewestRelease.Found(ReleaseVersions.Version("0.18.0"))));
        SetupFactsReader reader = new(fragment, new FakeClaudeSettings(new NotificationChannelState.Configured(NotificationChannelState.TerminalBell)), state, SettingsFile, log);

        SetupFacts facts = await reader.ReadAsync(TestContext.Current.CancellationToken);

        facts.ShouldBe(new SetupFacts(new SetupState.At(SettingsFile), new SetupState.At(fragment.Location), new SetupState.Enabled(), new SetupState.Checked(nine, new NewestRelease.Found(ReleaseVersions.Version("0.18.0")))));
    }

    [Fact]
    public async Task ReadNothingSetUp()
    {
        SetupFactsReader reader = new(new FakeTerminalFragment(), new FakeClaudeSettings(new NotificationChannelState.Unset()), new InMemoryState(), SettingsFile, log);

        SetupFacts facts = await reader.ReadAsync(TestContext.Current.CancellationToken);

        facts.ShouldBe(new SetupFacts(new SetupState.At(SettingsFile), new SetupState.NotSetUp(), new SetupState.NotSetUp(), new SetupState.Never()));
    }

    [Fact]
    public async Task CallAMissingClaudeSettingsFileNotSetUp()
    {
        SetupFactsReader reader = new(new FakeTerminalFragment(), new FakeClaudeSettings(new NotificationChannelState.Missing()), new InMemoryState(), SettingsFile, log);

        SetupFacts facts = await reader.ReadAsync(TestContext.Current.CancellationToken);

        facts.ClaudeBell.ShouldBe(new SetupState.NotSetUp());
    }

    [Fact]
    public async Task NameAnotherChannel()
    {
        SetupFactsReader reader = new(new FakeTerminalFragment(), new FakeClaudeSettings(new NotificationChannelState.Configured("iterm2")), new InMemoryState(), SettingsFile, log);

        (await reader.ReadAsync(TestContext.Current.CancellationToken)).ClaudeBell.ShouldBe(new SetupState.OtherChannel("iterm2"));
    }

    [Fact]
    public async Task KeepTheOtherFactsWhenClaudesSettingsAreUnreadable()
    {
        SetupFactsReader reader = new(new FakeTerminalFragment().Installed(), new FakeClaudeSettings(new NotificationChannelState.Unreadable("not JSON")), new InMemoryState(), SettingsFile, log);

        SetupFacts facts = await reader.ReadAsync(TestContext.Current.CancellationToken);

        facts.ClaudeBell.ShouldBe(new SetupState.Unreadable());
        facts.TerminalProfiles.ShouldBeOfType<SetupState.At>();
    }

    [Fact]
    public async Task KeepTheOtherFactsWhenAReadThrows()
    {
        FakeClaudeSettings claude = new() { FailTheReadWith = new IOException("locked") };
        SetupFactsReader reader = new(new FakeTerminalFragment().Installed(), claude, new InMemoryState(), SettingsFile, log);

        SetupFacts facts = await reader.ReadAsync(TestContext.Current.CancellationToken);

        facts.ClaudeBell.ShouldBe(new SetupState.Unreadable());
        facts.TerminalProfiles.ShouldBeOfType<SetupState.At>();
    }

    [Fact]
    public async Task RecordAnUnreadablePiece()
    {
        FakeClaudeSettings claude = new() { FailTheReadWith = new IOException("locked") };
        SetupFactsReader reader = new(new FakeTerminalFragment().Installed(), claude, new InMemoryState(), SettingsFile, log);

        await reader.ReadAsync(TestContext.Current.CancellationToken);

        Failed failed = log.Snapshot().Single().Event.ShouldBeOfType<Failed>();
        failed.Site.ShouldBe(FailureSite.Load);
        failed.What.ShouldBe(CodeName.For(typeof(SetupFactsReader)));
    }
}