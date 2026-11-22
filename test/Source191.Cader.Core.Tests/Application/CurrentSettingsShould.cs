using Source191.Cader.Application;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

public sealed class CurrentSettingsShould
{
    private static readonly FolderPath Home = FolderPath.From(@"C:\Users\me");
    private static readonly Settings Saved = Settings.Resolve([FolderPath.From(@"D:\work")], new WorktreeRootChoice.At(FolderPath.From(@"D:\worktrees")), EditorChoice.Of(EditorPreset.VisualStudio), WorktreeBase.Head, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

    [Fact]
    public async Task AnswerTheSettingsItWasBuiltWith()
    {
        CurrentSettings current = await CurrentSettings.LoadAsync(new InMemoryState(), Home);

        current.Value.ShouldBe(SettingsFile.From(UserConfig.Missing, Home));
    }

    [Fact]
    public async Task AnswerTheSavedSettingsAfterASave()
    {
        CurrentSettings current = await CurrentSettings.LoadAsync(new InMemoryState(), Home);

        await current.UpdateAsync(_ => Saved);

        current.Value.ShouldBe(Saved);
    }

    [Fact]
    public async Task WriteTheSavedSettingsThroughThePort()
    {
        InMemoryState state = new();
        CurrentSettings current = await CurrentSettings.LoadAsync(state, Home);

        await current.UpdateAsync(_ => Saved);

        (await CurrentSettings.LoadAsync(state, Home)).Value.ShouldBe(Saved);
    }

    [Fact]
    public async Task ApplyTwoChangesInOrder()
    {
        InMemoryState state = new();
        CurrentSettings current = await CurrentSettings.LoadAsync(state, Home);
        TaskCompletionSource written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        state.HoldNextConfigWrite(written.Task);

        Task<Settings> first = current.UpdateAsync(settings => settings.WithUpdateCheckToggled());
        Task<Settings> second = current.UpdateAsync(settings => settings.WithDiagnosticLogToggled());
        written.SetResult();
        await Task.WhenAll(first, second);

        Settings reloaded = (await CurrentSettings.LoadAsync(state, Home)).Value;
        reloaded.CheckForUpdates.ShouldBeFalse();
        reloaded.DiagnosticLog.ShouldBeTrue();
        current.Value.ShouldBe(reloaded);
    }

    [Fact]
    public async Task ReplaceTheSnapshotOnlyAfterTheWrite()
    {
        InMemoryState state = new();
        CurrentSettings current = await CurrentSettings.LoadAsync(state, Home);
        Settings before = current.Value;
        TaskCompletionSource written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        state.HoldNextConfigWrite(written.Task);

        Task<Settings> saving = current.UpdateAsync(_ => Saved);
        Settings whileWriting = current.Value;
        written.SetResult();
        Settings answered = await saving;

        whileWriting.ShouldBe(before);
        answered.ShouldBe(Saved);
        current.Value.ShouldBe(Saved);
    }
}