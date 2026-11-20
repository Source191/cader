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

        await current.SaveAsync(Saved);

        current.Value.ShouldBe(Saved);
    }

    [Fact]
    public async Task WriteTheSavedSettingsThroughThePort()
    {
        InMemoryState state = new();
        CurrentSettings current = await CurrentSettings.LoadAsync(state, Home);

        await current.SaveAsync(Saved);

        (await CurrentSettings.LoadAsync(state, Home)).Value.ShouldBe(Saved);
    }

    [Fact]
    public async Task SavePaneHeightsOverTheCurrentSettings()
    {
        InMemoryState state = new();
        CurrentSettings current = new(Saved, state);

        await current.SavePaneHeightsAsync(new PaneHeights(Sessions: 4, Worktrees: 20));

        current.Value.ShouldBe(Saved.WithPaneHeights(new PaneHeights(Sessions: 4, Worktrees: 20)));
        (await CurrentSettings.LoadAsync(state, Home)).Value.ShouldBe(current.Value);
    }

    [Fact]
    public async Task KeepThePaneHeightsThroughASaveOfTheOtherSettings()
    {
        InMemoryState state = new();
        CurrentSettings current = new(Saved, state);
        await current.SavePaneHeightsAsync(new PaneHeights(Sessions: 4, Worktrees: 20));

        await current.SaveAsync(Saved.WithUpdateCheckToggled());

        current.Value.ShouldBe(Saved.WithUpdateCheckToggled().WithPaneHeights(new PaneHeights(Sessions: 4, Worktrees: 20)));
        (await CurrentSettings.LoadAsync(state, Home)).Value.ShouldBe(current.Value);
    }

    [Fact]
    public async Task WriteTwoSavesOneAfterTheOther()
    {
        InMemoryState state = new();
        CurrentSettings current = new(Saved, state);
        TaskCompletionSource firstWrite = new(TaskCreationOptions.RunContinuationsAsynchronously);
        state.HoldNextConfigWrite(firstWrite.Task);
        Settings dialog = Saved.WithDiagnosticLogToggled();

        Task dialogSave = current.SaveAsync(dialog);
        Task paneSave = current.SavePaneHeightsAsync(new PaneHeights(Sessions: 4, Worktrees: 20));
        await Task.Delay(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);
        state.ConfigWrites.ShouldBe(0, "the pane's save waits for the dialog's write");
        firstWrite.SetResult();
        await Task.WhenAll(dialogSave, paneSave);

        state.ConfigWrites.ShouldBe(2);
        Settings expected = dialog.WithPaneHeights(new PaneHeights(Sessions: 4, Worktrees: 20));
        current.Value.ShouldBe(expected);
        (await CurrentSettings.LoadAsync(state, Home)).Value.ShouldBe(expected);
    }
}